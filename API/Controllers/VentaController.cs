using IBussines;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.ApisPeru;
using Models.RequestResponse;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentaController : ControllerBase
    {
        private readonly IVentaBussines _ventaBussines;

        public VentaController(IVentaBussines ventaBussines)
        {
            _ventaBussines = ventaBussines;
        }
        #region VENTA ELECTRONICA
        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarVenta([FromBody] RegistrarVentaRequest request)
        {
            var response = await _ventaBussines.RegistrarVentaAsync(request);
            return Ok(response);
        }

        [HttpGet("descargar-pdf/{ventaId}")]
        public async Task<IActionResult> DescargarPdf(int ventaId)
        {
            var archivo = await _ventaBussines.DescargarPdfAsync(ventaId);

            return File(
                archivo.Bytes,
                archivo.ContentType,
                archivo.NombreArchivo
            );
        }

        [HttpGet("descargar-xml/{ventaId}")]
        public async Task<IActionResult> DescargarXml(int ventaId)
        {
            var archivo = await _ventaBussines.DescargarXmlAsync(ventaId);

            return File(
                archivo.Bytes,
                archivo.ContentType,
                archivo.NombreArchivo
            );
        }

        [HttpGet("consultar-estado-sunat/{ventaId}")]
        public async Task<IActionResult> ConsultarEstadoSunat(int ventaId)
        {
            var response = await _ventaBussines.ConsultarEstadoSunatAsync(ventaId);
            return Ok(response);
        }

        [HttpPost("sincronizar-estado-sunat/{ventaId}")]
        public async Task<IActionResult> SincronizarEstadoSunat(int ventaId)
        {
            var response = await _ventaBussines.SincronizarEstadoSunatAsync(ventaId);
            return Ok(response);
        }

        [HttpPost("reintentar-emision/{ventaId}")]
        public async Task<IActionResult> ReintentarEmision(int ventaId)
        {
            var response = await _ventaBussines.ReintentarEmisionAsync(ventaId);
            return Ok(response);
        }

        [HttpPost("emitir-nota-credito")]
        public async Task<IActionResult> EmitirNotaCredito(EmitirNotaCreditoRequest request)
        {
            var response = await _ventaBussines.EmitirNotaCreditoAsync(request);
            return Ok(response);
        }

        [HttpPost("anular-venta")]
        public async Task<IActionResult> AnularVenta(AnularVentaRequest request)
        {
            var response = await _ventaBussines.AnularVentaAsync(request);
            return Ok(response);
        }
        #endregion

        #region LISTAR VENTAS

        [HttpGet("listar/detalles")]
        public async Task<IActionResult> Listar([FromQuery] VentaFiltroRequest filtro)
        {
            var response = await _ventaBussines.ListarAsync(filtro);
            return Ok(response);
        }

        [HttpGet("{ventaId}/detalle")]
        public async Task<IActionResult> ObtenerDetalle(int ventaId)
        {
            var response = await _ventaBussines.ObtenerDetalleAsync(ventaId);
            return Ok(response);
        }
        #endregion
    }
}
