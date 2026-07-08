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
        private async Task CrearMovimientoCajaPorPagoAsync(Venta venta, VentaPago pago, int medioPagoId, int usuarioId)
        {
            if (venta.CajaSesionId == null)
                throw new Exception("La venta no tiene una sesión de caja asociada.");

            var medioPago = await _medioPagoRepository.GetByIdAsync(medioPagoId);

            if (medioPago == null)
                throw new Exception("El medio de pago no existe.");

            var codigo = medioPago.Codigo?.Trim().ToUpper();
            var tipo = medioPago.Tipo?.Trim().ToUpper();

            var esEfectivo = codigo == "EFECTIVO" || tipo == "EFECTIVO";

            if (!esEfectivo)
                return;

            var movimiento = new CajaMovimiento
            {
                CajaSesionId = venta.CajaSesionId.Value,
                CajaId = venta.CajaId,
                SucursalId = venta.SucursalId,
                UsuarioId = usuarioId,

                TipoMovimiento = "INGRESO",
                Concepto = "VENTA_EFECTIVO",

                MedioPagoId = medioPagoId,
                Monto = pago.Monto,

                AfectaEfectivo = true,

                Descripcion = $"Ingreso por venta {venta.Serie}-{venta.Correlativo}",
                ReferenciaTabla = "venta_pagos",
                ReferenciaId = pago.Id,

                Estado = "ACTIVO",
                FechaCreacion = DateTime.UtcNow
            };

            await _cajaMovimientoRepository.CreateAsync(movimiento);
        }

    }
}
