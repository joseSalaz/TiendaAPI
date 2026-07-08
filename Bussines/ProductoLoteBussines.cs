using DBModel.DBModels;
using IBussines;
using IRepository;
using Models.RequestResponse;
using UtilInterface;

namespace Bussines
{
    public class ProductoLoteBussines : IProductoLoteBussines
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IProductoPresentacionRepository _productoPresentacionRepository;
        private readonly IProductoStockRepository _productoStockRepository;
        private readonly IProductoLoteRepository _productoLoteRepository;
        private readonly IStockMovimientoRepository _stockMovimientoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductoLoteBussines(
            IProductoRepository productoRepository,
            IProductoPresentacionRepository productoPresentacionRepository,
            IProductoStockRepository productoStockRepository,
            IProductoLoteRepository productoLoteRepository,
            IStockMovimientoRepository stockMovimientoRepository,
            IUnitOfWork unitOfWork)
        {
            _productoRepository = productoRepository;
            _productoPresentacionRepository = productoPresentacionRepository;
            _productoStockRepository = productoStockRepository;
            _productoLoteRepository = productoLoteRepository;
            _stockMovimientoRepository = stockMovimientoRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<RegistrarEntradaStockResponse> RegistrarEntradaAsync(RegistrarEntradaStockRequest request)
        {
            ValidarRequest(request);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var producto = await _productoRepository.GetByIdAsync(request.ProductoId);

                if (producto == null)
                    throw new Exception("El producto no existe.");

                var presentacion = await _productoPresentacionRepository.GetByIdAndProductoAsync(
                    request.PresentacionId,
                    request.ProductoId
                );

                if (presentacion == null)
                    throw new Exception("La presentación no existe o no pertenece al producto.");

                if (!presentacion.PermiteCompra)
                    throw new Exception("La presentación seleccionada no permite compra.");

                if (producto.ManejaVencimiento == true && request.FechaVencimiento == null)
                    throw new Exception("La fecha de vencimiento es obligatoria para este producto.");

                var unidadesPorPresentacion = Convert.ToInt32(presentacion.CantidadUnidades);

                if (unidadesPorPresentacion <= 0)
                    throw new Exception("La presentación tiene una cantidad de unidades inválida.");

                var cantidadUnidades = request.CantidadPresentacion * unidadesPorPresentacion;

                var stock = await _productoStockRepository.GetByProductoSucursalAsync(
                    request.ProductoId,
                    request.SucursalId
                );

                if (stock == null)
                    throw new Exception("No existe registro de stock para este producto y sucursal.");

                var stockAnterior = Convert.ToInt32(stock.StockActual);

                var precioCompraEntrada = request.PrecioCompra
                    ?? presentacion.PrecioCompra
                    ?? producto.PrecioCompra
                    ?? throw new Exception("No existe precio de compra para esta presentación.");

                var costoUnitario = precioCompraEntrada / unidadesPorPresentacion;

                var lote = new ProductoLote
                {
                    ProductoId = request.ProductoId,
                    SucursalId = request.SucursalId,
                    CompraDetalleId = null,
                    CodigoLote = request.CodigoLote,
                    FechaVencimiento = request.FechaVencimiento,
                    StockInicial = cantidadUnidades,
                    StockActual = cantidadUnidades,
                    CostoUnitario = costoUnitario,
                    FechaIngreso = DateTime.UtcNow,
                    Estado = "ACTIVO"
                };

                await _productoLoteRepository.CreateAsync(lote);

                await _unitOfWork.SaveChangesAsync();

                stock.StockActual = stockAnterior + cantidadUnidades;

                var movimiento = new StockMovimiento
                {
                    ProductoId = request.ProductoId,
                    SucursalId = request.SucursalId,
                    Tipo = "ENTRADA",
                    Cantidad = cantidadUnidades,
                    StockAnterior = stockAnterior,
                    StockNuevo = Convert.ToInt32(stock.StockActual),
                    Motivo = request.Observacion,
                    ReferenciaTabla = "producto_lotes",
                    ReferenciaId = lote.Id,
                    UsuarioId = request.UsuarioId,
                    LoteId = lote.Id,
                    FechaCreacion = DateTime.UtcNow
                };

                await _stockMovimientoRepository.CreateAsync(movimiento);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return new RegistrarEntradaStockResponse
                {
                    ProductoId = request.ProductoId,
                    PresentacionId = request.PresentacionId,
                    SucursalId = request.SucursalId,
                    CantidadPresentacion = request.CantidadPresentacion,
                    CantidadUnidades = cantidadUnidades,
                    LoteId = lote.Id,
                    StockAnterior = stockAnterior,
                    StockNuevo = Convert.ToInt32(stock.StockActual)
                };
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private void ValidarRequest(RegistrarEntradaStockRequest request)
        {
            if (request.ProductoId <= 0)
                throw new Exception("El producto es obligatorio.");

            if (request.PresentacionId <= 0)
                throw new Exception("La presentación es obligatoria.");

            if (request.SucursalId <= 0)
                throw new Exception("La sucursal es obligatoria.");

            if (request.UsuarioId <= 0)
                throw new Exception("El usuario es obligatorio.");

            if (request.CantidadPresentacion <= 0)
                throw new Exception("La cantidad debe ser mayor a 0.");

            if (request.PrecioCompra.HasValue && request.PrecioCompra.Value < 0)
                throw new Exception("El precio de compra no puede ser negativo.");
        }
    }
}