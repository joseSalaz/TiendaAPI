using AutoMapper;
using DBModel.DBModels;
using IBussines;
using IRepository;
using Microsoft.EntityFrameworkCore;
using Models.RequestResponse;
using UtilInterface;
using UtilPaginados.RequestResponse;

namespace Bussines
{
    public class ProductoBussines : IProductoBussines
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductoPresentacionRepository _productoPresentacionRepository;
        private readonly ISucursalRepository _sucursalRepository;
        private readonly IProductoStockRepository _productoStockRepository;

        public ProductoBussines(
            IProductoRepository productoRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IProductoPresentacionRepository productoPresentacionRepository, ISucursalRepository sucursalRepository
            , IProductoStockRepository productoStockRepository)
        {
            _productoRepository = productoRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _productoPresentacionRepository = productoPresentacionRepository;
            _sucursalRepository = sucursalRepository;
            _productoStockRepository = productoStockRepository;
        }

        public async Task<List<ProductoResponse>> GetAllAsync()
        {
            var lista = await _productoRepository.GetAllAsync();

            return _mapper.Map<List<ProductoResponse>>(lista);
        }

        public async Task<PaginacionResponse<ProductoResponse>> GetAllProductsPaged(int pagina, int cantidad)
        {
            var query = _productoRepository
                .GetQueryable()
                .AsNoTracking();

            var resultado = await query.PaginarAsync(
                pagina,
                cantidad);

            return new PaginacionResponse<ProductoResponse>
            {
                Items = _mapper.Map<List<ProductoResponse>>(resultado.Items),
                Total = resultado.Total,
                PaginaActual = resultado.PaginaActual,
                TotalPaginas = resultado.TotalPaginas,
                CantidadPorPagina = resultado.CantidadPorPagina
            };
        }

        public async Task<ProductoResponse?> GetByIdAsync(object id)
        {
            var entidad = await _productoRepository.GetByIdAsync(id);

            if (entidad == null)
                return null;

            return _mapper.Map<ProductoResponse>(entidad);
        }

        public async Task<ProductoResponse> CreateAsync(
            ProductoRequest entity)
        {
            var producto = _mapper.Map<Producto>(entity);

            await _productoRepository.CreateAsync(producto);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductoResponse>(producto);
        }

        public async Task<ProductoResponse> UpdateAsync(
            ProductoRequest entity)
        {
            var producto = _mapper.Map<Producto>(entity);

            await _productoRepository.UpdateAsync(producto);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductoResponse>(producto);
        }

        public async Task DeleteAsync(object id)
        {
            await _productoRepository.DeleteAsync(id);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<ProductoResponse>>
            CreateMultipleAsync(
                List<ProductoRequest> request)
        {
            var productos =
                _mapper.Map<List<Producto>>(request);

            await _productoRepository
                .InsertMultipleAsync(productos);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<List<ProductoResponse>>(productos);
        }

        public async Task<List<ProductoResponse>>
            UpdateMultipleAsync(
                List<ProductoRequest> request)
        {
            var productos =
                _mapper.Map<List<Producto>>(request);

            await _productoRepository
                .UpdateMultipleAsync(productos);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<List<ProductoResponse>>(productos);
        }

        public async Task DeleteMultipleItemsAsync(
            List<ProductoRequest> request)
        {
            var productos =
                _mapper.Map<List<Producto>>(request);

            await _productoRepository
                .DeleteMultipleItemsAsync(productos);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<ProductoResponse>>
            GetAutoCompleteAsync(string query)
        {
            var lista =
                await _productoRepository
                    .GetAutoCompleteAsync(query);

            return _mapper.Map<List<ProductoResponse>>(lista);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #region MetodosPropios
        public async Task<ProductoResponse> CreateProductosStock(ProductoRequest request)
        {
            ValidarProducto(request);

            await ValidarCodigosBarrasAsync(request);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var producto = _mapper.Map<Producto>(request);

                await _productoRepository.CreateAsync(producto);

                await _unitOfWork.SaveChangesAsync();

                var presentaciones = CrearPresentaciones(
                    producto.Id,
                    request.Presentaciones
                );

                await _productoPresentacionRepository.CreateRangeAsync(presentaciones);

                var stocks = await CrearStocksInicialesAsync(producto.Id);

                await _productoStockRepository.CreateRangeAsync(stocks);

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();

                return _mapper.Map<ProductoResponse>(producto);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        private void ValidarProducto(ProductoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new Exception("El nombre del producto es obligatorio.");

            if (request.Presentaciones == null || !request.Presentaciones.Any())
                throw new Exception("El producto debe tener al menos una presentación.");

            var cantidadBase = request.Presentaciones.Count(p => p.EsUnidadBase);

            if (cantidadBase != 1)
                throw new Exception("El producto debe tener exactamente una presentación base.");

            var unidadBase = request.Presentaciones.First(p => p.EsUnidadBase);

            if (unidadBase.CantidadUnidades != 1)
                throw new Exception("La presentación base debe tener cantidad de unidades igual a 1.");

            foreach (var presentacion in request.Presentaciones)
            {
                if (string.IsNullOrWhiteSpace(presentacion.Nombre))
                    throw new Exception("El nombre de la presentación es obligatorio.");

                if (presentacion.CantidadUnidades <= 0)
                    throw new Exception("La cantidad de unidades debe ser mayor a 0.");

                if (presentacion.PrecioVenta <= 0)
                    throw new Exception("El precio de venta debe ser mayor a 0.");
            }

            var codigosDuplicados = request.Presentaciones
                .Where(p => !string.IsNullOrWhiteSpace(p.CodigoBarras))
                .GroupBy(p => p.CodigoBarras)
                .Any(g => g.Count() > 1);

            if (codigosDuplicados)
                throw new Exception("Hay códigos de barras repetidos en las presentaciones.");
        }

        private async Task ValidarCodigosBarrasAsync(ProductoRequest request)
        {
            foreach (var presentacion in request.Presentaciones)
            {
                if (string.IsNullOrWhiteSpace(presentacion.CodigoBarras))
                    continue;

                var existe = await _productoPresentacionRepository
                    .ExistsCodigoBarrasAsync(presentacion.CodigoBarras);

                if (existe)
                    throw new Exception($"El código de barras {presentacion.CodigoBarras} ya existe.");
            }
        }

        private List<ProductoPresentacione> CrearPresentaciones(
            int productoId,
            List<ProductoPresentacionRequest> requests)
        {
            return requests.Select(p => new ProductoPresentacione
            {
                ProductoId = productoId,
                Nombre = p.Nombre,
                CodigoBarras = p.CodigoBarras,
                CantidadUnidades = p.CantidadUnidades,
                PrecioCompra = p.PrecioCompra,
                PrecioVenta = p.PrecioVenta,
                PrecioMayoreo = p.PrecioMayoreo,
                EsUnidadBase = p.EsUnidadBase,
                PermiteVenta = p.PermiteVenta,
                PermiteCompra = p.PermiteCompra,
                Estado = string.IsNullOrWhiteSpace(p.Estado) ? "ACTIVO" : p.Estado
            }).ToList();
        }

        private async Task<List<ProductoStock>> CrearStocksInicialesAsync(int productoId)
        {
            var sucursales = await _sucursalRepository.GetActivasAsync();

            return sucursales.Select(s => new ProductoStock
            {
                ProductoId = productoId,
                SucursalId = s.Id,
                StockActual = 0,
                StockMinimo = 0
            }).ToList();
        }
        public async Task<ProductoResponse?> GetByIdAsync(int id)
        {
            var producto = await _productoRepository.GetByIdConPresentacionesAsync(id);

            if (producto == null)
                return null;

            return _mapper.Map<ProductoResponse>(producto);
        }


        public async Task<ProductoResponse> UpdateProductoConPresentaciones(ProductoRequest request)
        {
            ValidarProductoConPresentaciones(request);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var producto = await _productoRepository.GetByIdConPresentacionesAsync(request.Id);

                if (producto == null)
                    throw new Exception("Producto no encontrado.");

                producto.CategoriaId = request.CategoriaId;
                producto.CodigoBarras = request.CodigoBarras;
                producto.CodigoInterno = request.CodigoInterno;
                producto.Nombre = request.Nombre;
                producto.Descripcion = request.Descripcion;
                producto.UnidadMedida = request.UnidadMedida;
                producto.PrecioCompra = request.PrecioCompra;
                producto.PrecioVenta = request.PrecioVenta;
                producto.PrecioMayoreo = request.PrecioMayoreo;
                producto.PermiteStockNegativo = request.PermiteStockNegativo;
                producto.Estado = request.Estado;
                producto.ImagenUrl = request.ImagenUrl;
                producto.ManejaVencimiento = request.ManejaVencimiento;
                producto.FechaActualizacion = DateTime.UtcNow;

                var presentacionesDb = producto.ProductoPresentaciones.ToList();

                var idsRequest = request.Presentaciones
                    .Where(x => x.Id.HasValue && x.Id.Value > 0)
                    .Select(x => x.Id!.Value)
                    .ToList();

                foreach (var presentacionRequest in request.Presentaciones)
                {
                    if (presentacionRequest.Id.HasValue && presentacionRequest.Id.Value > 0)
                    {
                        var presentacionDb = presentacionesDb
                            .FirstOrDefault(x => x.Id == presentacionRequest.Id.Value);

                        if (presentacionDb == null)
                            throw new Exception($"La presentación con id {presentacionRequest.Id.Value} no existe.");

                        presentacionDb.Nombre = presentacionRequest.Nombre;
                        presentacionDb.CodigoBarras = presentacionRequest.CodigoBarras;
                        presentacionDb.CantidadUnidades = presentacionRequest.CantidadUnidades;
                        presentacionDb.PrecioCompra = presentacionRequest.PrecioCompra;
                        presentacionDb.PrecioVenta = presentacionRequest.PrecioVenta;
                        presentacionDb.PrecioMayoreo = presentacionRequest.PrecioMayoreo;
                        presentacionDb.EsUnidadBase = presentacionRequest.EsUnidadBase;
                        presentacionDb.PermiteVenta = presentacionRequest.PermiteVenta;
                        presentacionDb.PermiteCompra = presentacionRequest.PermiteCompra;
                        presentacionDb.Estado = presentacionRequest.Estado;
                    }
                    else
                    {
                        producto.ProductoPresentaciones.Add(new ProductoPresentacione
                        {
                            ProductoId = producto.Id,
                            Nombre = presentacionRequest.Nombre,
                            CodigoBarras = presentacionRequest.CodigoBarras,
                            CantidadUnidades = presentacionRequest.CantidadUnidades,
                            PrecioCompra = presentacionRequest.PrecioCompra,
                            PrecioVenta = presentacionRequest.PrecioVenta,
                            PrecioMayoreo = presentacionRequest.PrecioMayoreo,
                            EsUnidadBase = presentacionRequest.EsUnidadBase,
                            PermiteVenta = presentacionRequest.PermiteVenta,
                            PermiteCompra = presentacionRequest.PermiteCompra,
                            Estado = presentacionRequest.Estado
                        });
                    }
                }

                var presentacionesQuitadas = presentacionesDb
                    .Where(x => !idsRequest.Contains(x.Id))
                    .ToList();

                foreach (var presentacionQuitada in presentacionesQuitadas)
                {
                    presentacionQuitada.Estado = "INACTIVO";
                    presentacionQuitada.PermiteVenta = false;
                    presentacionQuitada.PermiteCompra = false;
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return _mapper.Map<ProductoResponse>(producto);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        private void ValidarProductoConPresentaciones(ProductoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new Exception("El nombre del producto es obligatorio.");

            if (request.Presentaciones == null || !request.Presentaciones.Any())
                throw new Exception("El producto debe tener al menos una presentación.");

            var bases = request.Presentaciones.Where(x => x.EsUnidadBase).ToList();

            if (bases.Count != 1)
                throw new Exception("Debe existir exactamente una presentación base.");

            if (bases[0].CantidadUnidades != 1)
                throw new Exception("La presentación base debe tener cantidad de unidades igual a 1.");

            foreach (var presentacion in request.Presentaciones)
            {
                if (string.IsNullOrWhiteSpace(presentacion.Nombre))
                    throw new Exception("El nombre de la presentación es obligatorio.");

                if (presentacion.CantidadUnidades <= 0)
                    throw new Exception("La cantidad de unidades debe ser mayor a 0.");

                if (presentacion.PrecioVenta <= 0)
                    throw new Exception("El precio de venta de la presentación debe ser mayor a 0.");
            }
        }

        public async Task<PaginacionResponse<ProductoVentaResponse>> BuscarParaVentaAsync(ProductoVentaFiltroRequest filtro)
        {
            if (filtro.SucursalId <= 0)
                throw new Exception("La sucursal es obligatoria.");

            if (string.IsNullOrWhiteSpace(filtro.Texto) || filtro.Texto.Trim().Length < 2)
            {
                return new PaginacionResponse<ProductoVentaResponse>
                {
                    Items = new List<ProductoVentaResponse>(),
                    Total = 0,
                    PaginaActual = Math.Max(1, filtro.Pagina),
                    TotalPaginas = 0,
                    CantidadPorPagina = Math.Clamp(filtro.Cantidad, 1, 50)
                };
            }

            return await _productoRepository.BuscarParaVentaAsync(filtro);
        }
        #endregion 
    }
}