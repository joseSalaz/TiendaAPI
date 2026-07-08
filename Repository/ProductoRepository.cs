using DBModel.DBModels;
using IRepository;
using Microsoft.EntityFrameworkCore;
using Models.RequestResponse;
using UtilPaginados.RequestResponse;

namespace Repository
{
    public class ProductoRepository : GenericRepository<Producto>, IProductoRepository
    {
        public ProductoRepository(
            _TiendaDbContext context)
            : base(context)
        {
        }
        public IQueryable<Producto> GetQueryable()
        {
            return _context.Productos;
        }

        public async Task<List<Producto>>
            GetAutoCompleteAsync(
                string query)
        {
            return await _dbSet
                .Where(x =>
                    x.Nombre.Contains(query))
                .ToListAsync();
        }
<<<<<<< HEAD
        public async Task<Producto?> GetByCodigoBarras(string codigoBarras)
        {
            return await _context.Productos
                .FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras);
        }
=======
        public async Task<Producto?> GetByIdConPresentacionesAsync(int id)
        {
            return await _context.Productos
                .Include(x => x.ProductoPresentaciones)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<PaginacionResponse<ProductoVentaResponse>> BuscarParaVentaAsync(ProductoVentaFiltroRequest filtro)
        {
            var texto = filtro.Texto?.Trim();

            filtro.Pagina = Math.Max(1, filtro.Pagina);
            filtro.Cantidad = Math.Clamp(filtro.Cantidad, 1, 50);

            if (string.IsNullOrWhiteSpace(texto) || texto.Length < 2)
            {
                return new PaginacionResponse<ProductoVentaResponse>
                {
                    Items = new List<ProductoVentaResponse>(),
                    Total = 0,
                    PaginaActual = filtro.Pagina,
                    TotalPaginas = 0,
                    CantidadPorPagina = filtro.Cantidad
                };
            }

            var textoLike = $"%{texto}%";

            var query = _context.Productos
                .AsNoTracking()
                .Where(p => p.Estado == "ACTIVO")
                .Where(p =>
                    EF.Functions.ILike(p.Nombre, textoLike) ||
                    EF.Functions.ILike(p.CodigoInterno ?? "", textoLike) ||
                    EF.Functions.ILike(p.CodigoBarras ?? "", textoLike) ||
                    p.ProductoPresentaciones.Any(pp =>
                        pp.Estado == "ACTIVO" &&
                        pp.PermiteVenta == true &&
                        EF.Functions.ILike(pp.CodigoBarras ?? "", textoLike)
                    )
                )
                .OrderByDescending(p =>
                    p.CodigoInterno == texto ||
                    p.CodigoBarras == texto ||
                    p.ProductoPresentaciones.Any(pp => pp.CodigoBarras == texto)
                )
                .ThenBy(p => p.Nombre);

            var total = await query.CountAsync();

            var items = await query
                .Skip((filtro.Pagina - 1) * filtro.Cantidad)
                .Take(filtro.Cantidad)
                .Select(p => new ProductoVentaResponse
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    CodigoInterno = p.CodigoInterno,
                    CodigoBarras = p.CodigoBarras,
                    UnidadMedida = p.UnidadMedida,
                    PrecioVenta = p.PrecioVenta,
                    ManejaVencimiento = p.ManejaVencimiento || false,

                    Presentaciones = p.ProductoPresentaciones
                        .Where(pp =>
                            pp.Estado == "ACTIVO" &&
                            pp.PermiteVenta == true
                        )
                        .OrderByDescending(pp => pp.EsUnidadBase)
                        .ThenBy(pp => pp.CantidadUnidades)
                        .Select(pp => new ProductoVentaPresentacionResponse
                        {
                            Id = pp.Id,
                            Nombre = pp.Nombre,
                            CodigoBarras = pp.CodigoBarras,
                            CantidadUnidades = pp.CantidadUnidades,
                            PrecioVenta = pp.PrecioVenta,
                            PrecioMayoreo = pp.PrecioMayoreo,
                            EsUnidadBase = pp.EsUnidadBase
                        })
                        .ToList()
                })
                .ToListAsync();

            return new PaginacionResponse<ProductoVentaResponse>
            {
                Items = items,
                Total = total,
                PaginaActual = filtro.Pagina,
                TotalPaginas = (int)Math.Ceiling(total / (double)filtro.Cantidad),
                CantidadPorPagina = filtro.Cantidad
            };
        }

>>>>>>> 787cd51adb08540b2ce86f2737763df37a392c8d
    }
}