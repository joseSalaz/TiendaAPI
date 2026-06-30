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
    public interface IDocumentoElectronicoBussines
    {
        Task<PaginacionResponse<DocumentoElectronicoResponse>> ListarAsync(
            DocumentoElectronicoFiltroRequest filtro
        );

        Task<List<DocumentoElectronicoResponse>> ObtenerPorVentaAsync(int ventaId);

        Task<DocumentoElectronicoResponse> ObtenerDocumentoActualVentaAsync(int ventaId);

        Task<ArchivoVentaResponse> DescargarPdfAsync(int documentoElectronicoId);

        Task<ArchivoVentaResponse> DescargarXmlAsync(int documentoElectronicoId);

        Task<ArchivoVentaResponse> DescargarPdfActualVentaAsync(int ventaId);

        Task<ArchivoVentaResponse> DescargarXmlActualVentaAsync(int ventaId);
    }
}
