using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasAPI.Controllers
{
	[Route("api/generos")]
	[ApiController]
	public class GenerosController : ControllerBase
	{
		private readonly ApplicationDbContext context;
		private readonly IMapper mapper;

		public GenerosController(ApplicationDbContext context, IMapper mapper)
		{
			this.context = context;
			this.mapper = mapper;
		}

		public async Task<ActionResult<List<GeneroDTO>>> ObtenerGenerosDTO()
		{
			var entidades = await context.Generos.ToListAsync();

			var dtos = mapper.Map<List<GeneroDTO>>(entidades);

			return dtos;
		}

		[HttpGet("{id:int}",Name = "obtenerGenero")]
		public async Task<ActionResult<GeneroDTO>> Get(int id)
		{
			var entidad = await context.Generos.FirstOrDefaultAsync(x => x.Id == id);

			if (entidad == null)
			{
				return NotFound();
			}

			var generodto = mapper.Map<GeneroDTO>(entidad);

			return generodto;
		}

		[HttpPost]
		public async Task<ActionResult> Post([FromBody] GeneroCreacionDTO generoCreacionDTO)
		{
			var entidad = mapper.Map<Genero>(generoCreacionDTO);

			context.Add(entidad);

			await context.SaveChangesAsync();

			var generoDTO = mapper.Map<GeneroDTO>(entidad);

			return new CreatedAtRouteResult("obtenerGenero", new { id = generoDTO.Id }, generoDTO);
		}

		[HttpPut("{id}")]
		public async Task<ActionResult> Put([FromBody] GeneroCreacionDTO generoCreacionDTO, int id)
		{
			var entidad = mapper.Map<Genero>(generoCreacionDTO);

			entidad.Id = id;
			context.Entry(entidad).State = EntityState.Modified;

			//context.Update(entidad);

			await context.SaveChangesAsync();
			return NoContent();
		}

		[HttpDelete("{id:int}")]
		public async Task<ActionResult> Delete(int id)
		{
			var existe = await context.Generos.AnyAsync(x => x.Id == id);

			if (!existe)
			{
				return NotFound();
			}

			context.Remove(new Genero() { Id = id});

			await context.SaveChangesAsync();

			return NoContent();
		}
	}
}
