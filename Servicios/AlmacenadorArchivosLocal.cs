using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasAPI.Servicios
{
	public class AlmacenadorArchivosLocal : IAlmacenadorArchivos
	{
		private readonly IWebHostEnvironment env; // para obtener la ruta del wwwroot donde colocar los archivos
		private readonly IHttpContextAccessor httpContextAccessor;

		public AlmacenadorArchivosLocal(IWebHostEnvironment env, IHttpContextAccessor httpContextAccesor)
		{
			this.env = env;
			this.httpContextAccessor = httpContextAccesor;
		}

		public Task BorrarArhivo(string ruta, string contenedor)
		{
			if (ruta != null)
			{
				var nombreArchivo = Path.GetFileName(ruta);
				string directorioArchivo = Path.Combine(env.WebRootPath, contenedor, nombreArchivo);

				if (File.Exists(directorioArchivo))
				{
					File.Delete(directorioArchivo);
				}
			}

			return Task.FromResult(0);
		}

		public async Task<string> EditarArhivo(byte[] contenido, string extension, string contenedor, string ruta, string contentType)
		{
			await BorrarArhivo(ruta, contenedor);
			return await GuardarArhivo(contenido, extension, contenedor, contentType);
		}

		public async Task<string> GuardarArhivo(byte[] contenido, string extension, string contenedor, string contentType)
		{
			var nombreArchivo = $"{ Guid.NewGuid()}{ extension}";
			string folder = Path.Combine(env.WebRootPath, contenedor);

			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			string ruta = Path.Combine(folder, nombreArchivo);
			await File.WriteAllBytesAsync(ruta, contenido);

			var urlActual = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";
			var urlParaBD = Path.Combine(urlActual, contenedor, nombreArchivo).Replace("\\", "/");
			return urlParaBD;
		}
	}
}
