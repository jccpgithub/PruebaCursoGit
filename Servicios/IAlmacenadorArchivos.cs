using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasAPI.Servicios
{
	public interface IAlmacenadorArchivos
	{
		Task<string> GuardarArhivo(byte[] contenido, string extension, string contenedor, string contentType);
		Task<string> EditarArhivo(byte[] contenido, string extension, string contenedor, string ruta, string contentType);

		Task BorrarArhivo(string ruta, string contenedor);
	}
}
