using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasAPI.Servicios
{
	public class AlmacenadorArchivosAzure : IAlmacenadorArchivos
	{
		private readonly string connectionString;

		public AlmacenadorArchivosAzure(IConfiguration configuration)
		{
			connectionString = configuration.GetConnectionString("AzureStorage");
		}
		public async Task BorrarArhivo(string ruta, string contenedor)
		{
			if (string.IsNullOrEmpty(ruta)) //si no hay una imagen ya existente
			{
				return;
			}

			var cliente = new BlobContainerClient(connectionString, contenedor);
			await cliente.CreateIfNotExistsAsync(); // crea el contenedor si no existe

			var archivo = Path.GetFileName(ruta);
			var blob = cliente.GetBlobClient(archivo);
			await blob.DeleteIfExistsAsync();

		}

		public async Task<string> EditarArhivo(byte[] contenido, string extension, string contenedor, string ruta, string contentType)
		{
			await BorrarArhivo(ruta, contenedor);
			return await GuardarArhivo(contenido, extension, contenedor, contentType);
		}

		public async Task<string> GuardarArhivo(byte[] contenido, string extension, string contenedor, string contentType)
		{
			//Hay que coger el array de bytes contenido y subirlo hacia azurestorage

			//contenedor -> la carpeta
			var cliente = new BlobContainerClient(connectionString, contenedor);
			await cliente.CreateIfNotExistsAsync(); // crea el contenedor si no existe
			cliente.SetAccessPolicy(PublicAccessType.Blob);

			var archivoNombre = $"{Guid.NewGuid()}{extension}"; //nombre aleatorio para que no haya colisiones
			var blob = cliente.GetBlobClient(archivoNombre);

			//Pasamos ahora el content-type para que se sepa que es una imagen con lo que vamos a trabajar

			var blobUploadOptions = new BlobUploadOptions();
			var blobHttpHeader = new BlobHttpHeaders();
			blobHttpHeader.ContentType = contentType;

			blobUploadOptions.HttpHeaders = blobHttpHeader;

			//enviamos el archivo hacia azure
			await blob.UploadAsync(new BinaryData(contenido), blobUploadOptions);

			//devolvemos la url para poder guardarla en la base de datos

			return blob.Uri.ToString();

		}
	}
}
