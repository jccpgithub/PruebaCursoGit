using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Helpers;
using PeliculasAPI.Servicios;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasAPI.Controllers
{
	[Route("api/actores")]
	[ApiController]
	public class ActoresController : ControllerBase
	{
		private readonly ApplicationDbContext context;
		private readonly IMapper mapper;
		private readonly IAlmacenadorArchivos almacenadorArchivos;
		private readonly string contenedor = "actores";

		public ActoresController(ApplicationDbContext context, IMapper mapper, IAlmacenadorArchivos almacenadorArchivos)
		{
			this.context = context;
			this.mapper = mapper;
			this.almacenadorArchivos = almacenadorArchivos;
		}


		[HttpGet]
		public async Task<ActionResult<List<ActorDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
		{
			var queryable = context.Actores.AsQueryable(); // el queryable que le voy a pasar al método de excepción
			await HttpContext.InsertarParametrosPaginacion(queryable, paginacionDTO.CantidadRegistrosPorPagina);
			// ya he agregado en la cabecera de la respuesta del http el número de páginas 

			var entidades = await queryable.Paginar(paginacionDTO).ToListAsync();

			var entidadesDTO = mapper.Map<List<ActorDTO>>(entidades);

			return entidadesDTO;
		}

		[HttpGet("{id:int}", Name="obtenerActor")]
		public async Task<ActionResult<ActorDTO>> Get(int id)
		{
			var entidad = await context.Actores.FirstOrDefaultAsync(x => x.Id == id);

			if (entidad is null)
			{
				return NotFound();
			}

			return mapper.Map<ActorDTO>(entidad);
		}

		[HttpPost]
		public async Task<ActionResult> Post([FromForm] ActorCreacionDTO actorCreacionDTO)
		{
			var entidad = mapper.Map<Actor>(actorCreacionDTO);

			if (actorCreacionDTO.Foto != null)
			{
				//Enviamos la foto hacia el azureStorage
				using (var memoryStream = new MemoryStream())
				{
					await actorCreacionDTO.Foto.CopyToAsync(memoryStream);
					var contenido = memoryStream.ToArray();
					var extension = Path.GetExtension(actorCreacionDTO.Foto.FileName);
					entidad.Foto = await almacenadorArchivos.GuardarArhivo(contenido, extension, contenedor, actorCreacionDTO.Foto.ContentType);
				}
			}

			context.Add(entidad);
			await context.SaveChangesAsync();

			var dto = mapper.Map<ActorDTO>(entidad);
			// El id lo tiene la entidad
			return new CreatedAtRouteResult("obtenerActor", new { id = entidad.Id }, dto);
		}


		[HttpPut("{id:int}")]
		public async Task<ActionResult> Put([FromForm] ActorCreacionDTO actorCreacionDTO, int id)
		{
			var actorDB = await context.Actores.FirstOrDefaultAsync(x => x.Id == id);

			if (actorDB == null)
			{
				return NotFound();
			}

			actorDB = mapper.Map(actorCreacionDTO, actorDB);

			if (actorCreacionDTO.Foto != null)
			{
				//Enviamos la foto hacia el azureStorage
				using (var memoryStream = new MemoryStream())
				{
					await actorCreacionDTO.Foto.CopyToAsync(memoryStream);
					var contenido = memoryStream.ToArray();
					var extension = Path.GetExtension(actorCreacionDTO.Foto.FileName);
					actorDB.Foto = await almacenadorArchivos.EditarArhivo(contenido, extension, contenedor, actorDB.Foto, actorCreacionDTO.Foto.ContentType);
				}
			}
		
			await context.SaveChangesAsync();

			return NoContent();
		}

		[HttpPatch("{id}")]
		public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<ActorPatchDTO> patchDocument)
		{
			if (patchDocument == null)
			{
				return BadRequest();
			}

			var entidadDB = await context.Actores.FirstOrDefaultAsync(x => x.Id == id);

			if (entidadDB == null)
			{
				return NotFound();
			}

			var entidadDTO = mapper.Map<ActorPatchDTO>(entidadDB);

			patchDocument.ApplyTo(entidadDTO, ModelState);

			var esValido = TryValidateModel(ModelState);

			if (!esValido)
			{
				return BadRequest(ModelState);
			}

			mapper.Map(entidadDTO, entidadDB);

			await context.SaveChangesAsync();

			return NoContent();
		}


		[HttpDelete("{id:int}")]
		public async Task<ActionResult> Delete(int id)
		{
			var existe = await context.Actores.AnyAsync(x => x.Id == id);

			if (!existe)
			{
				return NotFound();
			}

			context.Remove(new Actor() { Id = id });

			await context.SaveChangesAsync();

			return NoContent();
		}

	}
}
