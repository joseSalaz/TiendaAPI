using Models.ApisPeru;
using Models.RequestResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;
using UtilPaginados.RequestResponse;

namespace IBussines
{
    public interface IVentaBussines
    {
        Task<RegistrarVentaResponse> RegistrarVentaAsync(RegistrarVentaRequest request);
        Task<ArchivoVentaResponse> DescargarPdfAsync(int ventaId);
        Task<ArchivoVentaResponse> DescargarXmlAsync(int ventaId);
        Task<EstadoSunatConsultaResponse> ConsultarEstadoSunatAsync(int ventaId);
        Task<RegistrarVentaResponse> SincronizarEstadoSunatAsync(int ventaId);
        Task<RegistrarVentaResponse> ReintentarEmisionAsync(int ventaId);
        Task<EmitirNotaCreditoResponse> EmitirNotaCreditoAsync(EmitirNotaCreditoRequest request);
        Task<AnularVentaResponse> AnularVentaAsync(AnularVentaRequest request);
        Task<PaginacionResponse<VentaListadoResponse>> ListarAsync(VentaFiltroRequest filtro);
        Task<VentaDetalleCompletoResponse> ObtenerDetalleAsync(int ventaId);
        Task<ArchivoVentaResponse> DescargarPdfPropioA4Async(int ventaId);
        Task<ArchivoVentaResponse> DescargarPdfPropioTicketAsync(int ventaId);
    }
}
