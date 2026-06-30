using DBModel.DBModels;
using Models.ApisPeru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IService.FacturacionElectronica
{
    public interface IApisPeruPayloadBuilder
    {
        object ConstruirFacturaBoletaPayload(
           Venta venta,
           List<DetalleSunatTemporal> detallesSunat,
           Empresa empresa
       );

        object ConstruirNotasCreditoPayload(
            NotasCredito notaCredito,
            Venta venta,
            List<DetalleSunatTemporal> detallesSunat,
            Empresa empresa
        );
        object ConstruirPayloadApisPeru(Venta venta, List<DetalleSunatTemporal> detallesSunat, Empresa empresa);
    }
}
