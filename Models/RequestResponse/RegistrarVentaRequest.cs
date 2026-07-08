using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestResponse
{
    public class RegistrarVentaRequest
    {
        public int SucursalId { get; set; }
        public int CajaId { get; set; }
        public int CajaSesionId { get; set; }
        public int UsuarioId { get; set; }
        public int? ClienteId { get; set; }
        public ClienteVentaRequest? Cliente { get; set; }
        // 01 = Factura, 03 = Boleta
        public string TipoDocumento { get; set; } = "03";

        // B001 o F001
        public string Serie { get; set; } = "B001";

        public decimal Descuento { get; set; }

        public string? Observaciones { get; set; }

        public List<RegistrarVentaDetalleRequest> Detalles { get; set; } = new();
        public List<RegistrarVentaPagoRequest> Pagos { get; set; } = new();
    }
    public class ClienteVentaRequest
    {
        public int? Id { get; set; }

        public string? TipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }

        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public string? RazonSocial { get; set; }
        public string? NombreCompleto { get; set; }

        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
    }
    public class RegistrarVentaDetalleRequest
    {
        public int ProductoId { get; set; }
        public int PresentacionId { get; set; }
        public int Cantidad { get; set; }
        public decimal Descuento { get; set; }
    }

    public class RegistrarVentaPagoRequest
    {
        public int MedioPagoId { get; set; }
        public decimal Monto { get; set; }
        public string? Referencia { get; set; }
    }
}
