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
        private async Task DescontarStockFefoAsync(VentaDetalle ventaDetalle, int productoId, int sucursalId, int usuarioId, int cantidadUnidades)
        {
            var stock = await _productoStockRepository.GetByProductoSucursalAsync(productoId, sucursalId);

            if (stock == null)
                throw new Exception("No existe stock para este producto y sucursal.");

            var stockActual = Convert.ToInt32(stock.StockActual);

            if (stockActual < cantidadUnidades)
                throw new Exception($"Stock insuficiente. Stock actual: {stockActual}, requerido: {cantidadUnidades}.");

            var lotes = await _productoLoteRepository.GetLotesDisponiblesFefoAsync(productoId, sucursalId);

            var stockLotes = lotes.Sum(x => x.StockActual);

            if (stockLotes < cantidadUnidades)
                throw new Exception($"Stock insuficiente por lotes. Disponible en lotes: {stockLotes}, requerido: {cantidadUnidades}.");

            var restante = cantidadUnidades;
            var stockTemporal = stockActual;

            foreach (var lote in lotes)
            {
                if (restante <= 0)
                    break;

                var tomar = Math.Min(restante, lote.StockActual);

                lote.StockActual -= tomar;
                restante -= tomar;

                var detalleLote = new VentaDetalleLote
                {
                    VentaDetalleId = ventaDetalle.Id,
                    LoteId = lote.Id,
                    Cantidad = tomar,
                    FechaCreacion = DateTime.UtcNow
                };

                await _ventaDetalleLoteRepository.CreateAsync(detalleLote);

                var movimiento = new StockMovimiento
                {
                    ProductoId = productoId,
                    SucursalId = sucursalId,
                    Tipo = "VENTA",
                    Cantidad = tomar,
                    StockAnterior = stockTemporal,
                    StockNuevo = stockTemporal - tomar,
                    Motivo = $"Venta {ventaDetalle.VentaId}",
                    ReferenciaTabla = "venta_detalles",
                    ReferenciaId = ventaDetalle.Id,
                    UsuarioId = usuarioId,
                    FechaCreacion = DateTime.UtcNow,
                    LoteId = lote.Id
                };

                await _stockMovimientoRepository.CreateAsync(movimiento);

                stockTemporal -= tomar;
            }

            stock.StockActual = stockActual - cantidadUnidades;
        }

    }
}
