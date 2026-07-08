using AutoMapper;
using DBModel.DBModels;
using IBussines;
using IRepository;
using Models.Comon;
using Models.RequestResponse;
using UtilInterface;

namespace Bussines
{
    public class CajaSesionBussines : ICajaSesionBussines
    {
        private readonly ICajaRepository _cajaRepository;
        private readonly ICajaSesionRepository _cajaSesionRepository;
        private readonly ICajaMovimientoRepository _cajaMovimientoRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        private static readonly decimal[] DenominacionesSoles =
        {
            200m, 100m, 50m, 20m, 10m,
            5m, 2m, 1m,
            0.50m, 0.20m, 0.10m
        };

        public CajaSesionBussines(
            ICajaRepository cajaRepository,
            ICajaSesionRepository cajaSesionRepository,
            ICajaMovimientoRepository cajaMovimientoRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _cajaRepository = cajaRepository;
            _cajaSesionRepository = cajaSesionRepository;
            _cajaMovimientoRepository = cajaMovimientoRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<CajaSesioneResponse> AbrirCajaAsync(AperturaCajaRequest request)
        {
            ValidarAperturaCaja(request);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var caja = await _cajaRepository.GetByIdAsync(request.CajaId);

                if (caja is null)
                    throw new CustomException($"No se encontró la caja con Id: {request.CajaId}.", 404);

                if (caja.Estado != "ACTIVO")
                    throw new CustomException("La caja no está activa.", 400);

                var sesionActiva = await _cajaSesionRepository
                    .ObtenerSesionAbiertaPorCajaAsync(request.CajaId);

                if (sesionActiva is not null)
                    throw new CustomException(
                        $"La caja {request.CajaId} ya tiene una sesión abierta (Id: {sesionActiva.Id}). " +
                        "Debe cerrarla antes de abrir una nueva.", 400);

                var nuevaSesion = new CajaSesione
                {
                    CajaId = request.CajaId,
                    UsuarioAperturaId = request.UsuarioAperturaId,
                    FechaApertura = DateTime.UtcNow,
                    MontoInicial = request.MontoInicial,
                    FechaCierre = null,
                    UsuarioCierreId = null,
                    MontoCierre = null,
                    Diferencia = null,
                    Observaciones = request.Observaciones,
                    Estado = "ABIERTA"
                };

                await _cajaSesionRepository.CreateAsync(nuevaSesion);
                await _unitOfWork.SaveChangesAsync();

                var movimientoApertura = new CajaMovimiento
                {
                    CajaSesionId = nuevaSesion.Id,
                    CajaId = caja.Id,
                    SucursalId = caja.SucursalId,
                    UsuarioId = request.UsuarioAperturaId,

                    TipoMovimiento = "INGRESO",
                    Concepto = "APERTURA",

                    MedioPagoId = null,
                    Monto = request.MontoInicial,
                    AfectaEfectivo = true,

                    Descripcion = "Monto inicial de apertura de caja",
                    ReferenciaTabla = "caja_sesiones",
                    ReferenciaId = nuevaSesion.Id,

                    Estado = "ACTIVO",
                    FechaCreacion = DateTime.UtcNow
                };

                await _cajaMovimientoRepository.CreateAsync(movimientoApertura);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return MapToResponse(nuevaSesion);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<CajaSesioneResponse> CerrarCajaAsync(CierreCajaRequest request)
        {
            ValidarCierreCaja(request);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var sesion = await _cajaSesionRepository.ObtenerPorIdAsync(request.CajaSesionId)
                    ?? throw new CustomException(
                        $"No se encontró la sesión de caja con Id: {request.CajaSesionId}.", 404);

                if (sesion.Estado == "CERRADA")
                    throw new CustomException(
                        $"La sesión {request.CajaSesionId} ya fue cerrada el {sesion.FechaCierre:dd/MM/yyyy HH:mm}.", 400);

                if (sesion.Estado != "ABIERTA")
                    throw new CustomException("La sesión de caja no está abierta.", 400);

                var caja = await _cajaRepository.GetByIdAsync(sesion.CajaId);

                if (caja is null)
                    throw new CustomException($"No se encontró la caja con Id: {sesion.CajaId}.", 404);

                var efectivoEsperado = await CalcularEfectivoEsperadoAsync(sesion.Id);
                var diferencia = Math.Round(request.MontoCierre - efectivoEsperado, 2);

                sesion.UsuarioCierreId = request.UsuarioCierreId;
                sesion.FechaCierre = DateTime.UtcNow;
                sesion.MontoCierre = request.MontoCierre;
                sesion.Diferencia = diferencia;
                sesion.Observaciones = request.Observaciones ?? sesion.Observaciones;
                sesion.Estado = "CERRADA";

                var movimientoCierre = new CajaMovimiento
                {
                    CajaSesionId = sesion.Id,
                    CajaId = caja.Id,
                    SucursalId = caja.SucursalId,
                    UsuarioId = request.UsuarioCierreId,

                    TipoMovimiento = "AJUSTE",
                    Concepto = "CIERRE",

                    MedioPagoId = null,
                    Monto = request.MontoCierre,
                    AfectaEfectivo = false,

                    Descripcion = $"Cierre de caja. Esperado: S/ {efectivoEsperado:F2}, contado: S/ {request.MontoCierre:F2}, diferencia: S/ {diferencia:F2}",
                    ReferenciaTabla = "caja_sesiones",
                    ReferenciaId = sesion.Id,

                    Estado = "ACTIVO",
                    FechaCreacion = DateTime.UtcNow
                };

                await _cajaMovimientoRepository.CreateAsync(movimientoCierre);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return MapToResponse(sesion);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
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
                    $"al total (S/ {request.Total:F2}).", 400);

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

        private async Task<decimal> CalcularEfectivoEsperadoAsync(int cajaSesionId)
        {
            var movimientos = await _cajaMovimientoRepository
                .ObtenerMovimientosActivosPorSesionAsync(cajaSesionId);

            decimal efectivoEsperado = 0;

            foreach (var movimiento in movimientos)
            {
                if (!movimiento.AfectaEfectivo)
                    continue;

                if (movimiento.TipoMovimiento == "INGRESO")
                    efectivoEsperado += movimiento.Monto;

                if (movimiento.TipoMovimiento == "EGRESO")
                    efectivoEsperado -= movimiento.Monto;
            }

            return Math.Round(efectivoEsperado, 2);
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

                restante = Math.Round(restante - (denominacion * cantidad), 2);
            }

            return resultado;
        }

        private static void ValidarAperturaCaja(AperturaCajaRequest request)
        {
            if (request.CajaId <= 0)
                throw new CustomException("La caja es obligatoria.", 400);

            if (request.UsuarioAperturaId <= 0)
                throw new CustomException("El usuario de apertura es obligatorio.", 400);

            if (request.MontoInicial < 0)
                throw new CustomException("El monto inicial no puede ser negativo.", 400);
        }

        private static void ValidarCierreCaja(CierreCajaRequest request)
        {
            if (request.CajaSesionId <= 0)
                throw new CustomException("La sesión de caja es obligatoria.", 400);

            if (request.UsuarioCierreId <= 0)
                throw new CustomException("El usuario de cierre es obligatorio.", 400);

            if (request.MontoCierre < 0)
                throw new CustomException("El monto de cierre no puede ser negativo.", 400);
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
    }
}