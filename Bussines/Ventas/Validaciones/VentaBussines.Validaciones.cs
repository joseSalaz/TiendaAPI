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
        private void ValidarRequest(RegistrarVentaRequest request)
        {
            if (request.SucursalId <= 0)
                throw new Exception("La sucursal es obligatoria.");

            if (request.CajaId <= 0)
                throw new Exception("La caja es obligatoria.");

            if (request.CajaSesionId == null || request.CajaSesionId <= 0)
                throw new Exception("La sesión de caja es obligatoria.");

            if (request.UsuarioId <= 0)
                throw new Exception("El usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.TipoDocumento))
                throw new Exception("El tipo de documento es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Serie))
                throw new Exception("La serie es obligatoria.");

            if (request.Detalles == null || !request.Detalles.Any())
                throw new Exception("La venta debe tener al menos un detalle.");

            foreach (var detalle in request.Detalles)
            {
                if (detalle.ProductoId <= 0)
                    throw new Exception("El producto es obligatorio.");

                if (detalle.PresentacionId <= 0)
                    throw new Exception("La presentación es obligatoria.");

                if (detalle.Cantidad <= 0)
                    throw new Exception("La cantidad debe ser mayor a 0.");

                if (detalle.Descuento < 0)
                    throw new Exception("El descuento no puede ser negativo.");
            }

            if (request.Pagos == null || !request.Pagos.Any())
                throw new Exception("Debe registrar al menos un pago.");
        }
        private async Task<int?> ResolverClienteVentaAsync(RegistrarVentaRequest request)
        {
            var clienteIdRequest = request.ClienteId.HasValue && request.ClienteId.Value > 0
                ? request.ClienteId.Value
                : request.Cliente?.Id;

            if (clienteIdRequest.HasValue && clienteIdRequest.Value > 0)
            {
                var existeCliente = await _clienteRepository.ExisteAsync(clienteIdRequest.Value);

                if (!existeCliente)
                    throw new Exception("El cliente seleccionado no existe en la base de datos.");

                return clienteIdRequest.Value;
            }

            var clienteRequest = request.Cliente;

            if (clienteRequest == null)
            {
                if (request.TipoDocumento == "01")
                    throw new Exception("Para emitir factura debes seleccionar un cliente con RUC.");

                return null;
            }

            var tipoDocumento = NormalizarTipoDocumento(clienteRequest.TipoDocumento);
            var numeroDocumento = clienteRequest.NumeroDocumento?.Trim();

            if (string.IsNullOrWhiteSpace(numeroDocumento))
            {
                if (request.TipoDocumento == "01")
                    throw new Exception("Para emitir factura debes ingresar el RUC del cliente.");

                return null;
            }

            if (request.TipoDocumento == "01" && tipoDocumento != "RUC")
                throw new Exception("Para emitir factura el cliente debe tener RUC.");

            if (request.TipoDocumento == "01" && numeroDocumento.Length != 11)
                throw new Exception("El RUC debe tener 11 dígitos.");

            var clienteExistente = await _clienteRepository.ObtenerPorDocumentoAsync(
                tipoDocumento,
                numeroDocumento
            );

            if (clienteExistente != null)
                return clienteExistente.Id;

            var nuevoCliente = new Cliente
            {
                TipoDocumento = tipoDocumento,
                NumeroDocumento = numeroDocumento,

                Nombres = clienteRequest.Nombres,
                Apellidos = clienteRequest.Apellidos,
                RazonSocial = clienteRequest.RazonSocial,

                Direccion = clienteRequest.Direccion,
                Telefono = clienteRequest.Telefono,
                Email = clienteRequest.Email,

                Estado = "ACTIVO",
                FechaCreacion = DateTime.UtcNow,
            };

            if (string.IsNullOrWhiteSpace(nuevoCliente.Nombres) &&
                string.IsNullOrWhiteSpace(nuevoCliente.RazonSocial) &&
                !string.IsNullOrWhiteSpace(clienteRequest.NombreCompleto))
            {
                nuevoCliente.Nombres = clienteRequest.NombreCompleto.Trim();
            }

            await _clienteRepository.CreateAsync(nuevoCliente);
            await _unitOfWork.SaveChangesAsync();

            return nuevoCliente.Id;
        }

        private static string NormalizarTipoDocumento(string? tipoDocumento)
        {
            var tipo = tipoDocumento?.Trim().ToUpper() ?? "";

            return tipo switch
            {
                "DNI" => "DNI",
                "1" => "DNI",
                "RUC" => "RUC",
                "6" => "RUC",
                _ => tipo
            };
        }

        private void ValidarReglasLocalesEmision(RegistrarVentaRequest request)
        {
            var esFacturaBoleta =
                request.TipoDocumento == "01" ||
                request.TipoDocumento == "03";

            if (!esFacturaBoleta)
                return;

            if (request.Descuento < 0)
                throw new Exception("El descuento global no puede ser negativo.");

            if (request.TipoDocumento == "01")
            {
                var tieneCliente =
                    (request.ClienteId.HasValue && request.ClienteId.Value > 0) ||
                    request.Cliente != null;

                if (!tieneCliente)
                    throw new Exception("Para emitir factura debes seleccionar un cliente con RUC.");
            }
        }
    }
}
