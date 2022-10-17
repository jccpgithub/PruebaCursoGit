using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasAPI.Helpers
{
	public static class HttpContextExtensions
	{
		public async static Task InsertarParametrosPaginacion<T>(this HttpContext httpContext, IQueryable<T> queryable, int cantidadRegistrosPorPagina)
		{
			//hacemos el conteo
			double cantidad = await queryable.CountAsync();

			// Vemos el número de páginas que vamos a necesitar

			double cantidadPaginas = Math.Ceiling(cantidad / cantidadRegistrosPorPagina);

			httpContext.Response.Headers.Add("cantidadPaginas", cantidadPaginas.ToString());
		}
	}
}
