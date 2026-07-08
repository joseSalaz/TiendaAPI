using AutoMapper;
using DBModel.DBModels;
using IBussines;
using IRepository;
using Microsoft.EntityFrameworkCore;
using Models.RequestResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;
using UtilPaginados.RequestResponse;

namespace Bussines
{
    public class ProductoPresentacionBussines : IProductoPresentacionBussines
    {
        private readonly IProductoPresentacionRepository _ProductoPresentacionRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public ProductoPresentacionBussines(
            IProductoPresentacionRepository ProductoPresentacionRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _ProductoPresentacionRepository = ProductoPresentacionRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ProductoPresentacionResponse>> GetAllAsync()
        {
            var lista = await _ProductoPresentacionRepository.GetAllAsync();

            return _mapper.Map<List<ProductoPresentacionResponse>>(lista);
        }

        public async Task<PaginacionResponse<ProductoPresentacionResponse>> GetAllProductosPresentacionPaged(int pagina, int cantidad)
        {
            var query = _ProductoPresentacionRepository
                .GetQueryable()
                .AsNoTracking();

            var resultado = await query.PaginarAsync(
                pagina,
                cantidad);

            return new PaginacionResponse<ProductoPresentacionResponse>
            {
                Items = _mapper.Map<List<ProductoPresentacionResponse>>(resultado.Items),
                Total = resultado.Total,
                PaginaActual = resultado.PaginaActual,
                TotalPaginas = resultado.TotalPaginas,
                CantidadPorPagina = resultado.CantidadPorPagina
            };
        }

        public async Task<ProductoPresentacionResponse?> GetByIdAsync(object id)
        {
            var entidad = await _ProductoPresentacionRepository.GetByIdAsync(id);

            if (entidad == null)
                return null;

            return _mapper.Map<ProductoPresentacionResponse>(entidad);
        }

        public async Task<ProductoPresentacionResponse> CreateAsync(
            ProductoPresentacionRequest entity)
        {
            var ProductoPresentacion = _mapper.Map<ProductoPresentacione>(entity);

            await _ProductoPresentacionRepository.CreateAsync(ProductoPresentacion);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductoPresentacionResponse>(ProductoPresentacion);
        }

        public async Task<ProductoPresentacionResponse> UpdateAsync(
            ProductoPresentacionRequest entity)
        {
            var ProductoPresentacion = _mapper.Map<ProductoPresentacione>(entity);

            await _ProductoPresentacionRepository.UpdateAsync(ProductoPresentacion);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductoPresentacionResponse>(ProductoPresentacion);
        }

        public async Task DeleteAsync(object id)
        {
            await _ProductoPresentacionRepository.DeleteAsync(id);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<ProductoPresentacionResponse>>
            CreateMultipleAsync(
                List<ProductoPresentacionRequest> request)
        {
            var ProductoPresentacions =
                _mapper.Map<List<ProductoPresentacione>>(request);

            await _ProductoPresentacionRepository
                .InsertMultipleAsync(ProductoPresentacions);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<List<ProductoPresentacionResponse>>(ProductoPresentacions);
        }

        public async Task<List<ProductoPresentacionResponse>>
            UpdateMultipleAsync(
                List<ProductoPresentacionRequest> request)
        {
            var ProductoPresentacions =
                _mapper.Map<List<ProductoPresentacione>>(request);

            await _ProductoPresentacionRepository
                .UpdateMultipleAsync(ProductoPresentacions);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<List<ProductoPresentacionResponse>>(ProductoPresentacions);
        }

        public async Task DeleteMultipleItemsAsync(
            List<ProductoPresentacionRequest> request)
        {
            var ProductoPresentacions =
                _mapper.Map<List<ProductoPresentacione>>(request);

            await _ProductoPresentacionRepository
                .DeleteMultipleItemsAsync(ProductoPresentacions);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<ProductoPresentacionResponse>>
            GetAutoCompleteAsync(string query)
        {
            var lista =
                await _ProductoPresentacionRepository
                    .GetAutoCompleteAsync(query);

            return _mapper.Map<List<ProductoPresentacionResponse>>(lista);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
