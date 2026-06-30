using AutoMapper;
using DBModel.DBModels;
using IBussines;
using IRepository;
using IService.ConsultaDNI_RUC;
using Microsoft.EntityFrameworkCore;
using Models.ApisPeru;
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
    public class ClienteBussines : IClienteBussines
    {
        private readonly IClienteRepository _ClienteRepository;
        private readonly IMapper _mapper;
        private readonly IApisPeruConsultaDocumentoService _ApisPeruDocumentoService;
        private readonly IUnitOfWork _unitOfWork;

        public ClienteBussines(
            IClienteRepository ClienteRepository,
            IMapper mapper,
            IApisPeruConsultaDocumentoService apisPeruConsultaDocumentoService,
            IUnitOfWork unitOfWork)
        {
            _ClienteRepository = ClienteRepository;
            _mapper = mapper;
            _ApisPeruDocumentoService = apisPeruConsultaDocumentoService;
            _unitOfWork = unitOfWork;
        }
        #region CRUD
        public async Task<List<ClienteResponse>> GetAllAsync()
        {
            var lista = await _ClienteRepository.GetAllAsync();

            return _mapper.Map<List<ClienteResponse>>(lista);
        }

        public async Task<PaginacionResponse<ClienteResponse>> GetAllClientesPaged(int pagina, int cantidad)
        {
            var query = _ClienteRepository
                .GetQueryable()
                .AsNoTracking();

            var resultado = await query.PaginarAsync(
                pagina,
                cantidad);

            return new PaginacionResponse<ClienteResponse>
            {
                Items = _mapper.Map<List<ClienteResponse>>(resultado.Items),
                Total = resultado.Total,
                PaginaActual = resultado.PaginaActual,
                TotalPaginas = resultado.TotalPaginas,
                CantidadPorPagina = resultado.CantidadPorPagina
            };
        }

        public async Task<ClienteResponse?> GetByIdAsync(object id)
        {
            var entidad = await _ClienteRepository.GetByIdAsync(id);

            if (entidad == null)
                return null;

            return _mapper.Map<ClienteResponse>(entidad);
        }

        public async Task<ClienteResponse> CreateAsync(
            ClienteRequest entity)
        {
            var Cliente = _mapper.Map<Cliente>(entity);

            await _ClienteRepository.CreateAsync(Cliente);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ClienteResponse>(Cliente);
        }

        public async Task<ClienteResponse> UpdateAsync(
            ClienteRequest entity)
        {
            var Cliente = _mapper.Map<Cliente>(entity);

            await _ClienteRepository.UpdateAsync(Cliente);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ClienteResponse>(Cliente);
        }

        public async Task DeleteAsync(object id)
        {
            await _ClienteRepository.DeleteAsync(id);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<ClienteResponse>>
            CreateMultipleAsync(
                List<ClienteRequest> request)
        {
            var Clientes =
                _mapper.Map<List<Cliente>>(request);

            await _ClienteRepository
                .InsertMultipleAsync(Clientes);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<List<ClienteResponse>>(Clientes);
        }

        public async Task<List<ClienteResponse>>
            UpdateMultipleAsync(
                List<ClienteRequest> request)
        {
            var Clientes =
                _mapper.Map<List<Cliente>>(request);

            await _ClienteRepository
                .UpdateMultipleAsync(Clientes);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<List<ClienteResponse>>(Clientes);
        }

        public async Task DeleteMultipleItemsAsync(
            List<ClienteRequest> request)
        {
            var Clientes =
                _mapper.Map<List<Cliente>>(request);

            await _ClienteRepository
                .DeleteMultipleItemsAsync(Clientes);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<ClienteResponse>>
            GetAutoCompleteAsync(string query)
        {
            var lista =
                await _ClienteRepository
                    .GetAutoCompleteAsync(query);

            return _mapper.Map<List<ClienteResponse>>(lista);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion

        #region APIsPeru
        public async Task<Cliente> GetByTipoNroDocumento(string TipoDocumento, string NumeroDocumento)
        {
            Cliente vPersona = _ClienteRepository.GetByTipoNroDocumento(TipoDocumento, NumeroDocumento);

            if (vPersona == null || vPersona.Id == 0)
            {
                if (TipoDocumento.ToLower() == "dni")
                {
                    ApisPeruPersonaResponse pres = _ApisPeruDocumentoService.PersonaPorDNI(NumeroDocumento);
                    if (pres.success)
                    {
                        vPersona = new Cliente();
                        vPersona.NumeroDocumento = pres.dni;
                        vPersona.Apellidos = pres.apellidoPaterno +" "+ pres.apellidoMaterno ;
                        vPersona.Nombres = pres.nombres;
                    }
                }
                else if (TipoDocumento.ToLower() == "ruc")
                {
                    ApisPeruEmpresaResponse eres = _ApisPeruDocumentoService.EmpresaPorRUC(NumeroDocumento);
                    if (!string.IsNullOrEmpty(eres.ruc))
                    {
                        vPersona = new Cliente();
                        vPersona.NumeroDocumento = eres.ruc;
                        vPersona.Nombres = eres.razonSocial;
                        vPersona.RazonSocial = eres.razonSocial;
                        // Asignar otros datos de la empresa según sea necesario
                    }
                }
            }
            return vPersona;
        }
        #endregion
    }
}
