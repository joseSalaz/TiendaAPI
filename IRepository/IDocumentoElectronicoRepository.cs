using DBModel.DBModels;
using Models.RequestResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;
using UtilPaginados.RequestResponse;

namespace IRepository
{
    public interface IDocumentoElectronicoRepository : ICRUDRepositorio<DocumentosElectronico>
    {
        Task<DocumentosElectronico?> ObtenerPorVentaIdAsync(int ventaId);

        Task<DocumentosElectronico?> ObtenerPorComprobanteAsync(string tipoDocumento, string serie, int correlativo);
        Task<List<DocumentosElectronico>> ObtenerPorVentaAsync(int ventaId);

        Task<DocumentosElectronico?> ObtenerUltimoPorVentaAsync(int ventaId);

        Task<PaginacionResponse<DocumentosElectronico>> ListarFiltradoAsync(DocumentoElectronicoFiltroRequest filtro);
        Task<List<DocumentosElectronico>> ObtenerPorVentasAsync(List<int> ventaIds);
    }
}
