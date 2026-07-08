using Models.ApisPeru;
namespace IService.FacturacionElectronica
{
    public interface IApisPeruFacturacionService
    {
        Task<ApisPeruFacturaResponse> EnviarFacturaBoletaAsync(object payload);
        Task<ApisPeruArchivoResponse> GenerarPdfFacturaBoletaAsync(object payload);
        Task<ApisPeruArchivoResponse> GenerarXmlFacturaBoletaAsync(object payload);
        Task<ApisPeruEstadoResponse> ConsultarEstadoFacturaBoletaAsync(string tipoDocumento, string serie, int correlativo, string ruc);
        Task<ApisPeruFacturaResponse> EnviarNotaCreditoAsync(object payload);
        Task<ApisPeruArchivoResponse> GenerarPdfNotaCreditoAsync(object payload);
        Task<ApisPeruArchivoResponse> GenerarXmlNotaCreditoAsync(object payload);
    }
}
