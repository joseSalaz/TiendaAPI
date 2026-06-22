using AutoMapper;
using DBModel.Models;
using IBussines;
using IRepository;
using Microsoft.EntityFrameworkCore;
using Models.Comon;
using Models.RequestResponse;
using Repository;
using UtilInterface;
using UtilPaginados.RequestResponse;

namespace Bussines
{
    public class CajaSesionBussines : ICajaSesionBussines
    {
        private readonly ICajaSesionRepository _cajaSesionRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public CajaSesionBussines(
            ICajaSesionRepository cajasesionRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _cajaSesionRepository = cajasesionRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        private static readonly decimal[] DenominacionesSoles =
        {
            200m, 100m, 50m, 20m, 10m,   // billetes
            5m, 2m, 1m,                    // monedas
            0.50m, 0.20m, 0.10m            // centimos
        };

        public CajaSesionBussines(
            IUnitOfWork unitOfWork,
            ICajaSesionRepository cajaSesionRepository)
        {
            _unitOfWork = unitOfWork;
            _cajaSesionRepository = cajaSesionRepository;
        }

       
        public async Task<CajaSesioneResponse> AbrirCajaAsync(AperturaCajaRequest request)
        {
            // Validar que no haya caja abierta para ese cajaId
            var sesionActiva = await _cajaSesionRepository
                .ObtenerSesionAbiertaPorCajaAsync(request.CajaId);

            if (sesionActiva is not null)
                throw new CustomException(
                    $"La caja {request.CajaId} ya tiene una sesión abierta (Id: {sesionActiva.Id}). " +
                    "Debe cerrarla antes de abrir una nueva.",404);

            var nuevaSesion = new CajaSesione
            {
                CajaId = request.CajaId,
                UsuarioAperturaId = request.UsuarioAperturaId,
                MontoInicial = request.MontoInicial,
                Observaciones = request.Observaciones,
                Estado = "ABIERTA"
            };

            var creada = await _cajaSesionRepository.AbrirCajaAsync(nuevaSesion);
            return MapToResponse(creada);
        }

        public async Task<CajaSesioneResponse> CerrarCajaAsync(CierreCajaRequest request)
        {
            var sesion = await _cajaSesionRepository.ObtenerPorIdAsync(request.CajaSesionId)
                ?? throw new CustomException(
                    $"No se encontró la sesión de caja con Id: {request.CajaSesionId}.", 404);

            if (sesion.Estado == "CERRADA")
                throw new CustomException(
                    $"La sesión {request.CajaSesionId} ya fue cerrada el {sesion.FechaCierre:dd/MM/yyyy HH:mm}.", 404);

            sesion.UsuarioCierreId = request.UsuarioCierreId;
            sesion.MontoCierre = request.MontoCierre;
            sesion.Observaciones = request.Observaciones ?? sesion.Observaciones;

            var cerrada = await _cajaSesionRepository.CerrarCajaAsync(sesion);
            return MapToResponse(cerrada);
        }

       
        public async Task<CajaSesioneResponse?> ObtenerSesionActivaAsync(int cajaId)
        {
            var sesion = await _cajaSesionRepository
                .ObtenerSesionAbiertaPorCajaAsync(cajaId);

            return sesion is null ? null : MapToResponse(sesion);
        }

       
        public VueltoResponse CalcularVuelto(VueltoRequest request)
        {
            if (request.MontoRecibido < request.Total)
                throw new CustomException(
                    $"El monto recibido (S/ {request.MontoRecibido:F2}) es menor " +
                    $"al total (S/ {request.Total:F2}).", 404);

            var vuelto = request.MontoRecibido - request.Total;
            var desglose = DesgloseVuelto(vuelto);

            return new VueltoResponse
            {
                Total = request.Total,
                MontoRecibido = request.MontoRecibido,
                Vuelto = Math.Round(vuelto, 2),
                Desglose = desglose
            };
        }

        private static List<DesgloseBillete> DesgloseVuelto(decimal vuelto)
        {
            var resultado = new List<DesgloseBillete>();
            var restante = Math.Round(vuelto, 2);

            foreach (var denominacion in DenominacionesSoles)
            {
                if (restante <= 0) break;
                var centimosRestante = (int)Math.Round(restante * 100);
                var centimosDenominacion = (int)Math.Round(denominacion * 100);

                var cantidad = centimosRestante / centimosDenominacion;
                if (cantidad <= 0) continue;

                resultado.Add(new DesgloseBillete
                {
                    Denominacion = denominacion,
                    Cantidad = cantidad,
                    Subtotal = Math.Round(denominacion * cantidad, 2)
                });

                restante = Math.Round(
                    restante - (denominacion * cantidad), 2);
            }

            return resultado;
        }

        private static CajaSesioneResponse MapToResponse(CajaSesione cs) => new()
        {
            Id = cs.Id,
            CajaId = cs.CajaId,
            UsuarioAperturaId = cs.UsuarioAperturaId,
            FechaApertura = cs.FechaApertura,
            MontoInicial = cs.MontoInicial,
            FechaCierre = cs.FechaCierre,
            UsuarioCierreId = cs.UsuarioCierreId,
            MontoCierre = cs.MontoCierre,
            Diferencia = cs.Diferencia,
            Observaciones = cs.Observaciones,
            Estado = cs.Estado
        };


        #region 
        public Task<List<CajaSesioneRequest>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CajaSesioneRequest?> GetByIdAsync(object id)
        {
            throw new NotImplementedException();
        }

        public Task<CajaSesioneRequest> CreateAsync(CajaSesioneResponse entity)
        {
            throw new NotImplementedException();
        }

        public Task<CajaSesioneRequest> UpdateAsync(CajaSesioneResponse entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(object id)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMultipleItemsAsync(List<CajaSesioneResponse> request)
        {
            throw new NotImplementedException();
        }

        public Task<List<CajaSesioneRequest>> CreateMultipleAsync(List<CajaSesioneResponse> request)
        {
            throw new NotImplementedException();
        }

        public Task<List<CajaSesioneRequest>> UpdateMultipleAsync(List<CajaSesioneResponse> request)
        {
            throw new NotImplementedException();
        }

        public Task<List<CajaSesioneRequest>> GetAutoCompleteAsync(string query)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}