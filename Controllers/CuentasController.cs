using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PeliculasAPI.DTOs;
using System;
using System.Data;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using PeliculasAPI.Entidades;

namespace PeliculasAPI.Controllers
{
	[ApiController]
	[Route("api/cuentas")]

	public class CuentasController: ControllerBase
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly SignInManager<IdentityUser> _SignInManager;
		private readonly IConfiguration _configuration;
		private readonly ApplicationDbContext context;
		private readonly IMapper mapper;

		public CuentasController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> SignInManager,
			IConfiguration configuration, ApplicationDbContext context, IMapper mapper): base()
		{
			_userManager = userManager;
			_SignInManager = SignInManager;
			_configuration = configuration;
			this.context = context;
			this.mapper = mapper;
		}

		[HttpPost("Crear")]
		public async Task<ActionResult<UserToken>> CreateUser([FromBody] UserInfo model)
		{
			var user = new IdentityUser { UserName = model.Email, Email = model.Email };
			var result = await _userManager.CreateAsync(user, model.Password);

			if (result.Succeeded)
			{
				return await ConstruirToken(model);
			}
			else
			{
				return BadRequest(result.Errors);
			}
		}

		[HttpPost("Login")]
		public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo model)
		{
			var resultado = await _SignInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure:false);

			if (resultado.Succeeded)
			{
				return await ConstruirToken(model);
			}
			else
			{
				return BadRequest("Invalid login attempt");
			}
		}

		[HttpPost("RenovarToken")]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult<UserToken>> Renovar()
		{
			var userInfo = new UserInfo{
				Email = HttpContext.User.Identity.Name
			};

			return await ConstruirToken(userInfo);
		}


		private async Task<UserToken> ConstruirToken(UserInfo userInfo)
		{
			var claims = new List<Claim>()
			{
				new Claim(ClaimTypes.Name, userInfo.Email),
				new Claim(ClaimTypes.Email, userInfo.Email),
			};

			var identityUser = await _userManager.FindByEmailAsync(userInfo.Email);

			claims.Add(new Claim(ClaimTypes.NameIdentifier, identityUser.Id));

			var claimsDB = await _userManager.GetClaimsAsync(identityUser);

			claims.AddRange(claimsDB);

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var expiracion = DateTime.UtcNow.AddYears(1);

			JwtSecurityToken token = new JwtSecurityToken(
				issuer: null,
				audience: null,
				claims: claims,
				expires: expiracion,
				signingCredentials: creds);

			return new UserToken()
			{
				Token = new JwtSecurityTokenHandler().WriteToken(token),
				Expiracion = expiracion
			};
		}
		/*
		[HttpGet("Usuarios")]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="Admin")]
		public async Task<ActionResult<List<Usuario>>> Get()//[FromQuery] PaginacionDTO paginationDTO)
		{
			return await context.Users.ToListAsync();
		}*/

		[HttpGet("Roles")]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
		public async Task<ActionResult<List<string>>> GetRoles([FromQuery] PaginacionDTO paginationDTO)
		{
			return await context.Roles.Select(x => x.Name).ToListAsync();
		}

		[HttpPost("AsignarRol")]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
		public async Task<ActionResult> AsignarRol(EditarRolDTO editarRolDTO)
		{
			var user = await _userManager.FindByIdAsync(editarRolDTO.UsuarioID);
			if (user == null)
			{
				return NotFound();
			}
			await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role,editarRolDTO.NombreRol));
			return NoContent();
		}

		[HttpGet("RemoveRol")]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
		public async Task<ActionResult> RemoverRol(EditarRolDTO editarRolDTO)
		{
			var user = await _userManager.FindByIdAsync(editarRolDTO.UsuarioID);
			if (user == null)
			{
				return NotFound();
			}
			await _userManager.RemoveClaimAsync(user, new Claim(ClaimTypes.Role, editarRolDTO.NombreRol));
			return NoContent();
		}
	}
}
