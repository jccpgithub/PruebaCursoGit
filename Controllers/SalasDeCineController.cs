using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using PeliculasAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasAPI.Controllers
{
	[Route("api/SalasDeCine")]
	[ApiController]
	public class SalasDeCineController
	{
		private readonly ApplicationDbContext context;
		private readonly IMapper mapper;
		private readonly GeometryFactory geometryFactory;

		public SalasDeCineController(ApplicationDbContext context,
			IMapper mapper,
			GeometryFactory geometryFactory)
		{
			this.context = context;
			this.mapper = mapper;
			this.geometryFactory = geometryFactory;
		}

		[HttpGet("Cercanos")]
		public async Task<ActionResult<List<SalaDeCineCercanoDTO>>> Cercanos(
			[FromQuery] SalaDeCineCercanoFiltroDTO filtro)
		{
			var ubicacionUsuario = geometryFactory.CreatePoint(new Coordinate(filtro.Longitud, filtro.Latitud));

			var salasDeCine = await context.SalasDeCine
				.OrderBy(x => x.Ubicacion.Distance(ubicacionUsuario))
				.Where(x => x.Ubicacion.IsWithinDistance(ubicacionUsuario, filtro.DistanciaEnKms * 1000))
				.Select(x => new SalaDeCineCercanoDTO
				{
					Id = x.Id,
					Nombre = x.Nombre,
					Latitud = x.Ubicacion.Y,
					Longitud = x.Ubicacion.X,
					DistanciaEnMetros = Math.Round(x.Ubicacion.Distance(ubicacionUsuario))
				}).ToListAsync();

			return salasDeCine;
		}
	}
}
