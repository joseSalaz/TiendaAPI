using IBussines;
using Microsoft.AspNetCore.Mvc;
using Models.RequestResponse;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentoElectronicoController : ControllerBase
    {
        private readonly IDocumentoElectronicoBussines _documentoElectronicoBussines;

        public DocumentoElectronicoController(
            IDocumentoElectronicoBussines documentoElectronicoBussines)
        {
            _documentoElectronicoBussines = documentoElectronicoBussines;
        }

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] DocumentoElectronicoFiltroRequest filtro)
        {
            var response = await _documentoElectronicoBussines.ListarAsync(filtro);
            return Ok(response);
        }

        [HttpGet("venta/{ventaId}")]
        public async Task<IActionResult> ObtenerPorVenta(int ventaId)
        {
            var response = await _documentoElectronicoBussines.ObtenerPorVentaAsync(ventaId);
            return Ok(response);
        }

        [HttpGet("venta/{ventaId}/actual")]
        public async Task<IActionResult> ObtenerActualVenta(int ventaId)
        {
            var response = await _documentoElectronicoBussines.ObtenerDocumentoActualVentaAsync(ventaId);
            return Ok(response);
        }

        [HttpGet("{documentoElectronicoId}/pdf")]
        public async Task<IActionResult> DescargarPdf(int documentoElectronicoId)
        {
            var archivo = await _documentoElectronicoBussines.DescargarPdfAsync(documentoElectronicoId);

            return File(
                archivo.Bytes,
                archivo.ContentType,
                archivo.NombreArchivo
            );
        }

        [HttpGet("{documentoElectronicoId}/xml")]
        public async Task<IActionResult> DescargarXml(int documentoElectronicoId)
        {
            var archivo = await _documentoElectronicoBussines.DescargarXmlAsync(documentoElectronicoId);

            return File(
                archivo.Bytes,
                archivo.ContentType,
                archivo.NombreArchivo
            );
        }

        [HttpGet("venta/{ventaId}/actual/pdf")]
        public async Task<IActionResult> DescargarPdfActualVenta(int ventaId)
        {
            var archivo = await _documentoElectronicoBussines.DescargarPdfActualVentaAsync(ventaId);

            return File(
                archivo.Bytes,
                archivo.ContentType,
                archivo.NombreArchivo
            );
        }

        [HttpGet("venta/{ventaId}/actual/xml")]
        public async Task<IActionResult> DescargarXmlActualVenta(int ventaId)
        {
            var archivo = await _documentoElectronicoBussines.DescargarXmlActualVentaAsync(ventaId);

            return File(
                archivo.Bytes,
                archivo.ContentType,
                archivo.NombreArchivo
            );
        }
    }
}
