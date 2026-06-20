using AutoMapper;
using DBModel.Models;
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

        public ProductoBussines(
            IProductoRepository productoRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _productoRepository = productoRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
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
    }
}