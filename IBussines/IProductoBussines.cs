using Models.Comon;
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
    public interface IProductoBussines : ICRUDBussnies<ProductoRequest, ProductoResponse>
    {
        Task<PaginacionResponse<ProductoResponse>> GetAllProductsPaged(
           int pagina,
           int cantidad);

        Task<ProductoScannedDto> ScanByBarcodeAsync(string barcode);

        Task<ProductoResponse> CreateProductosStock(ProductoRequest request);
        Task<ProductoResponse?> GetByIdAsync(int id);
        Task<ProductoResponse> UpdateProductoConPresentaciones(ProductoRequest request);
        Task<PaginacionResponse<ProductoVentaResponse>> BuscarParaVentaAsync(ProductoVentaFiltroRequest filtro);

    }
}
