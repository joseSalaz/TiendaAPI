using DBModel.DBModels;
using IBussines;
using IRepository;
using IService.FacturacionElectronica;
using Models.ApisPeru;
using Models.RequestResponse;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UtilInterface;
using UtilPaginados.RequestResponse;

namespace Bussines
{
    public partial class VentaBussines
    {
        public async Task<AnularVentaResponse> AnularVentaAsync(AnularVentaRequest request)
        {
            if (request.VentaId <= 0)
                throw new Exception("El id de venta es obligatorio.");

            if (request.UsuarioId <= 0)
                throw new Exception("El usuario es obligatorio.");

            var venta = await _ventaRepository.GetByIdAsync(request.VentaId);

            if (venta == null)
                throw new Exception($"No se encontró la venta con id {request.VentaId}.");

            if (venta.EstadoSunat == "ANULADO")
                throw new Exception("La venta ya está anulada.");

            if (venta.EstadoSunat != "ACEPTADO" && venta.SunatCodigo != "0")
                throw new Exception("Solo se puede anular una venta aceptada por SUNAT.");

            var notaExistente = await _notaCreditoRepository.ObtenerPorVentaIdAsync(venta.Id);

            if (notaExistente != null && notaExistente.EstadoSunat == "ACEPTADO")
                throw new Exception($"La venta ya tiene nota de crédito aceptada: {notaExistente.Serie}-{notaExistente.Correlativo}");

            var empresa = await _empresaRepository.ObtenerEmpresaPorSucursalAsync(venta.SucursalId);

            if (empresa == null)
                throw new Exception("No se encontró la empresa emisora para la sucursal de la venta.");

            var detallesSunat = await ConstruirDetallesSunatDesdeVentaAsync(venta.Id);

            var serieNota = venta.TipoDocumento == "03" ? "BC01" : "FC01";

            var serie = await _comprobanteSerieRepository.GetSerieActivaAsync(
                venta.SucursalId,
                "07",
                serieNota
            );

            if (serie == null)
                throw new Exception($"No existe serie activa para nota de crédito {serieNota}.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                serie.CorrelativoActual += 1;
                var correlativoNota = serie.CorrelativoActual;

                var notaCredito = new NotasCredito
                {
                    VentaId = venta.Id,

                    TipoDocumento = "07",
                    Serie = serieNota,
                    Correlativo = correlativoNota,

                    TipoDocumentoAfectado = venta.TipoDocumento,
                    SerieAfectada = venta.Serie,
                    CorrelativoAfectado = venta.Correlativo,

                    CodigoMotivo = request.CodigoMotivo,
                    DescripcionMotivo = request.DescripcionMotivo,

                    Subtotal = venta.Subtotal,
                    Impuesto = venta.Impuesto,
                    Total = venta.Total,

                    EstadoSunat = "PENDIENTE_ENVIO",
                    FechaEmision = DateTime.UtcNow,
                    Estado = "ACTIVO"
                };

                await _notaCreditoRepository.CreateAsync(notaCredito);
                await _unitOfWork.SaveChangesAsync();

                var documentoNota = await ObtenerOCrearDocumentoElectronicoNotaCreditoAsync(notaCredito);

                var payload = _apisPeruPayloadBuilder.ConstruirNotasCreditoPayload(
                    notaCredito,
                    venta,
                    detallesSunat,
                    empresa
                );

                var respuesta = await _apisPeruFacturacionService.EnviarNotaCreditoAsync(payload);

                var codigoCdr = respuesta.CdrResponse?.Code;
                var aceptado = respuesta.Success &&
                               ((respuesta.CdrResponse?.Accepted ?? false) || codigoCdr == "0");

                var estadoSunat = aceptado
                    ? "ACEPTADO"
                    : respuesta.Success && respuesta.CdrResponse != null
                        ? "RECHAZADO"
                        : "ERROR_ENVIO";

                var codigoRespuesta = respuesta.CdrResponse?.Code
                    ?? respuesta.Error?.Code;

                var descripcionRespuesta = respuesta.CdrResponse?.Description
                    ?? respuesta.Error?.Message
                    ?? "Respuesta recibida de APISPERU.";

                notaCredito.EstadoSunat = estadoSunat;
                notaCredito.SunatCodigo = codigoRespuesta;
                notaCredito.SunatMensaje = descripcionRespuesta;
                notaCredito.SunatHash = respuesta.Hash;
                notaCredito.RespuestaSunatJson = respuesta.RawJson ?? "{}";
                notaCredito.FechaEnvioSunat = DateTime.UtcNow;

                documentoNota.FechaRespuesta = DateTime.UtcNow;
                documentoNota.CodigoRespuesta = Limitar(codigoRespuesta, 10);
                documentoNota.DescripcionRespuesta = descripcionRespuesta;
                documentoNota.CdrNumero = Limitar(respuesta.CdrResponse?.Id, 20);
                documentoNota.CdrDescripcion = respuesta.CdrResponse?.Description;
                documentoNota.XmlEnviado = respuesta.Xml;
                documentoNota.XmlRespuesta = respuesta.SunatResponse?.CdrZip ?? respuesta.RawJson;
                documentoNota.CodigoHash = respuesta.Hash;
                documentoNota.SunatXmlId = Limitar(
                    $"{empresa.Ruc}-{notaCredito.TipoDocumento}-{notaCredito.Serie}-{notaCredito.Correlativo}",
                    100
                );
                documentoNota.Estado = estadoSunat;

                if (!aceptado)
                {
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();

                    return new AnularVentaResponse
                    {
                        VentaId = venta.Id,
                        NotaCreditoId = notaCredito.Id,
                        VentaOriginal = $"{venta.Serie}-{venta.Correlativo}",
                        NotaCredito = $"{notaCredito.Serie}-{notaCredito.Correlativo}",
                        EstadoVenta = venta.EstadoSunat,
                        EstadoNotaCredito = notaCredito.EstadoSunat,
                        TotalDevuelto = 0,
                        SunatCodigo = notaCredito.SunatCodigo,
                        SunatMensaje = notaCredito.SunatMensaje,
                        Mensaje = "La nota de crédito no fue aceptada. No se devolvió stock ni caja."
                    };
                }

                await DevolverStockVentaAsync(
                    venta,
                    request.UsuarioId,
                    notaCredito.Id
                );

                if (request.DevolverDineroCaja)
                {
                    await RegistrarDevolucionCajaAsync(
                        venta,
                        request.UsuarioId,
                        notaCredito.Id
                    );
                }

                venta.EstadoSunat = "ANULADO";
                venta.SunatMensaje = $"Venta anulada con nota de crédito {notaCredito.Serie}-{notaCredito.Correlativo}";

                if (!string.IsNullOrWhiteSpace(request.Observacion))
                {
                    venta.Observaciones = $"{venta.Observaciones} | ANULACIÓN: {request.Observacion}";
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return new AnularVentaResponse
                {
                    VentaId = venta.Id,
                    NotaCreditoId = notaCredito.Id,
                    VentaOriginal = $"{venta.Serie}-{venta.Correlativo}",
                    NotaCredito = $"{notaCredito.Serie}-{notaCredito.Correlativo}",
                    EstadoVenta = venta.EstadoSunat,
                    EstadoNotaCredito = notaCredito.EstadoSunat,
                    TotalDevuelto = request.DevolverDineroCaja ? venta.Total : 0,
                    SunatCodigo = notaCredito.SunatCodigo,
                    SunatMensaje = notaCredito.SunatMensaje,
                    Mensaje = "Venta anulada correctamente. Stock y caja actualizados."
                };
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private async Task DevolverStockVentaAsync(Venta venta, int usuarioId, int notaCreditoId)
        {
            var detalles = await _ventaDetalleRepository.ObtenerPorVentaIdAsync(venta.Id);

            if (detalles == null || !detalles.Any())
                throw new Exception("La venta no tiene detalles para devolver stock.");

            foreach (var detalle in detalles)
            {
                var lotesVendidos = await _ventaDetalleLoteRepository
                    .ObtenerPorVentaDetalleIdAsync(detalle.Id);

                if (lotesVendidos == null || !lotesVendidos.Any())
                    throw new Exception($"No se encontró detalle de lotes para el producto {detalle.ProductoId}.");

                var totalUnidadesDevueltas = lotesVendidos.Sum(x => x.Cantidad);

                var stock = await _productoStockRepository.GetByProductoSucursalAsync(
                    detalle.ProductoId,
                    venta.SucursalId
                );

                if (stock == null)
                    throw new Exception($"No existe stock del producto {detalle.ProductoId} en la sucursal {venta.SucursalId}.");

                var stockAnterior = stock.StockActual;
                stock.StockActual += totalUnidadesDevueltas;

                foreach (var loteVendido in lotesVendidos)
                {
                    var lote = await _productoLoteRepository.GetByIdAsync(loteVendido.LoteId);

                    if (lote == null)
                        throw new Exception($"No se encontró el lote {loteVendido.LoteId}.");

                    lote.StockActual += loteVendido.Cantidad;

                    var movimiento = new StockMovimiento
                    {
                        ProductoId = detalle.ProductoId,
                        SucursalId = venta.SucursalId,
                        LoteId = lote.Id,

                        Tipo = "DEVOLUCION",
                        Cantidad = loteVendido.Cantidad,

                        StockAnterior = stockAnterior,
                        StockNuevo = stock.StockActual,

                        Motivo = $"Anulación de venta {venta.Serie}-{venta.Correlativo}",
                        ReferenciaTabla = "notas_credito",
                        ReferenciaId = notaCreditoId,

                        UsuarioId = usuarioId,
                        FechaCreacion = DateTime.UtcNow
                    };

                    await _stockMovimientoRepository.CreateAsync(movimiento);
                }
            }
        }

        private async Task RegistrarDevolucionCajaAsync(Venta venta, int usuarioId, int notaCreditoId)
        {
            if (venta.CajaId == null || venta.CajaSesionId == null)
                return;

            var pagos = await _ventaPagoRepository.ObtenerPorVentaIdAsync(venta.Id);

            if (pagos == null || !pagos.Any())
                return;

            // Por ahora asumimos que medio_pago_id = 1 es EFECTIVO.
            // Si tu tabla tiene otro id para efectivo, cambia esto.
            var montoEfectivo = pagos
                .Where(x => x.MedioPagoId == 1)
                .Sum(x => x.Monto);

            if (montoEfectivo <= 0)
                return;

            var movimientoCaja = new CajaMovimiento
            {
                CajaSesionId = venta.CajaSesionId.Value,
                CajaId = venta.CajaId,
                SucursalId = venta.SucursalId,
                UsuarioId = usuarioId,

                TipoMovimiento = "EGRESO",
                Concepto = "DEVOLUCION_VENTA",
                MedioPagoId = 1,

                Monto = montoEfectivo,
                AfectaEfectivo = true,

                Descripcion = $"Devolución por anulación de venta {venta.Serie}-{venta.Correlativo}",
                ReferenciaTabla = "notas_credito",
                ReferenciaId = notaCreditoId,

                Estado = "ACTIVO",
                FechaCreacion = DateTime.UtcNow
            };

            await _cajaMovimientoRepository.CreateAsync(movimientoCaja);
        }
    }
}
