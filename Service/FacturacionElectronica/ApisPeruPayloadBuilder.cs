using DBModel.DBModels;
using IService.FacturacionElectronica;
using Models.ApisPeru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.FacturacionElectronica
{
    public class ApisPeruPayloadBuilder : IApisPeruPayloadBuilder
    {
        public object ConstruirFacturaBoletaPayload(
           Venta venta,
           List<DetalleSunatTemporal> detallesSunat,
           Empresa empresa)
        {
            ValidarEmpresa(empresa);

            if (venta.Descuento.HasValue && venta.Descuento.Value > 0)
                throw new Exception("Para emisión electrónica usa descuento por detalle, no descuento global.");

            var rucEmpresa = long.Parse(empresa.Ruc);

            var detallesPayload = ConstruirDetallesPayload(detallesSunat);

            var mtoOperGravadas = Math.Round(detallesPayload.Sum(x => x.mtoValorVenta), 2);
            var mtoIgv = Math.Round(detallesPayload.Sum(x => x.igv), 2);
            var total = Math.Round(mtoOperGravadas + mtoIgv, 2);

            var fechaPeru = DateTimeOffset
                .UtcNow
                .ToOffset(TimeSpan.FromHours(-5))
                .ToString("yyyy-MM-ddTHH:mm:sszzz");

            var client = ConstruirClienteSunat(venta, empresa);

            return new
            {
                ublVersion = "2.1",
                tipoOperacion = "0101",
                tipoDoc = venta.TipoDocumento,
                serie = venta.Serie,
                correlativo = venta.Correlativo.ToString(),
                fechaEmision = fechaPeru,

                formaPago = new
                {
                    moneda = "PEN",
                    tipo = "Contado"
                },

                tipoMoneda = "PEN",

                client = client,

                company = new
                {
                    ruc = rucEmpresa,
                    razonSocial = empresa.RazonSocial,
                    nombreComercial = empresa.NombreComercial ?? empresa.RazonSocial,
                    address = new
                    {
                        direccion = empresa.DireccionFiscal,
                        provincia = empresa.Provincia,
                        departamento = empresa.Departamento,
                        distrito = empresa.Distrito,
                        ubigueo = empresa.Ubigeo
                    }
                },

                mtoOperGravadas = mtoOperGravadas,
                mtoIGV = mtoIgv,
                valorVenta = mtoOperGravadas,
                totalImpuestos = mtoIgv,
                subTotal = total,
                mtoImpVenta = total,

                details = detallesPayload,

                legends = new[]
                {
                    new
                    {
                        code = "1000",
                        value = ObtenerLeyendaMonto(total)
                    }
                }
            };
        }

        public object ConstruirNotasCreditoPayload(
            NotasCredito NotasCredito,
            Venta venta,
            List<DetalleSunatTemporal> detallesSunat,
            Empresa empresa)
        {
            ValidarEmpresa(empresa);

            var rucEmpresa = long.Parse(empresa.Ruc);

            var detallesPayload = ConstruirDetallesPayload(detallesSunat);

            var mtoOperGravadas = Math.Round(detallesPayload.Sum(x => x.mtoValorVenta), 2);
            var mtoIgv = Math.Round(detallesPayload.Sum(x => x.igv), 2);
            var total = Math.Round(mtoOperGravadas + mtoIgv, 2);

            var fechaPeru = DateTimeOffset
                .UtcNow
                .ToOffset(TimeSpan.FromHours(-5))
                .ToString("yyyy-MM-ddTHH:mm:sszzz");

            var client = ConstruirClienteSunat(venta, empresa);

            return new
            {
                ublVersion = "2.1",
                tipoDoc = NotasCredito.TipoDocumento,
                serie = NotasCredito.Serie,
                correlativo = NotasCredito.Correlativo.ToString(),
                fechaEmision = fechaPeru,

                tipDocAfectado = venta.TipoDocumento,
                numDocfectado = $"{venta.Serie}-{venta.Correlativo}",

                codMotivo = NotasCredito.CodigoMotivo,
                desMotivo = NotasCredito.DescripcionMotivo,

                tipoMoneda = "PEN",

                client = client,

                company = new
                {
                    ruc = rucEmpresa,
                    razonSocial = empresa.RazonSocial,
                    nombreComercial = empresa.NombreComercial ?? empresa.RazonSocial,
                    address = new
                    {
                        direccion = empresa.DireccionFiscal,
                        provincia = empresa.Provincia,
                        departamento = empresa.Departamento,
                        distrito = empresa.Distrito,
                        ubigueo = empresa.Ubigeo
                    }
                },

                mtoOperGravadas = mtoOperGravadas,
                mtoIGV = mtoIgv,
                valorVenta = mtoOperGravadas,
                totalImpuestos = mtoIgv,
                subTotal = total,
                mtoImpVenta = total,

                details = detallesPayload,

                legends = new[]
                {
                    new
                    {
                        code = "1000",
                        value = ObtenerLeyendaMonto(total)
                    }
                }
            };
        }
        public object ConstruirPayloadApisPeru(Venta venta, List<DetalleSunatTemporal> detallesSunat, Empresa empresa)
        {
            if (empresa == null)
                throw new Exception("La empresa emisora es obligatoria.");

            if (string.IsNullOrWhiteSpace(empresa.Ruc))
                throw new Exception("La empresa no tiene RUC configurado.");

            if (string.IsNullOrWhiteSpace(empresa.RazonSocial))
                throw new Exception("La empresa no tiene razón social configurada.");

            if (string.IsNullOrWhiteSpace(empresa.DireccionFiscal))
                throw new Exception("La empresa no tiene dirección fiscal configurada.");

            if (venta.Descuento.HasValue && venta.Descuento.Value > 0)
                throw new Exception("Para emisión electrónica usa descuento por detalle, no descuento global.");

            var rucEmpresa = long.Parse(empresa.Ruc);

            var detallesPayload = detallesSunat.Select(detalle =>
            {
                var valorUnitario = Math.Round(detalle.PrecioUnitarioConIgv / 1.18m, 6);
                var valorVenta = Math.Round(detalle.TotalConIgv / 1.18m, 2);
                var igv = Math.Round(detalle.TotalConIgv - valorVenta, 2);

                var tipoAfectacionIgv = int.TryParse(detalle.TipoAfectacionIgv, out var tipoAfe)
                    ? tipoAfe
                    : 10;

                return new
                {
                    codProducto = detalle.CodProducto,
                    unidad = detalle.UnidadSunat,
                    descripcion = detalle.Descripcion,
                    cantidad = detalle.Cantidad,

                    mtoValorUnitario = valorUnitario,
                    mtoValorVenta = valorVenta,

                    mtoBaseIgv = valorVenta,
                    porcentajeIgv = 18,
                    igv = igv,
                    tipAfeIgv = tipoAfectacionIgv,
                    totalImpuestos = igv,

                    mtoPrecioUnitario = detalle.PrecioUnitarioConIgv
                };
            }).ToList();

            var mtoOperGravadas = Math.Round(detallesPayload.Sum(x => x.mtoValorVenta), 2);
            var mtoIgv = Math.Round(detallesPayload.Sum(x => x.igv), 2);
            var total = Math.Round(mtoOperGravadas + mtoIgv, 2);

            var fechaPeru = DateTimeOffset
                .UtcNow
                .ToOffset(TimeSpan.FromHours(-5))
                .ToString("yyyy-MM-ddTHH:mm:sszzz");

            var client = ConstruirClienteSunat(venta, empresa);

            return new
            {
                ublVersion = "2.1",
                tipoOperacion = "0101",
                tipoDoc = venta.TipoDocumento,
                serie = venta.Serie,
                correlativo = venta.Correlativo.ToString(),
                fechaEmision = fechaPeru,

                formaPago = new
                {
                    moneda = "PEN",
                    tipo = "Contado"
                },

                tipoMoneda = "PEN",

                client = client,

                company = new
                {
                    ruc = rucEmpresa,
                    razonSocial = empresa.RazonSocial,
                    nombreComercial = empresa.NombreComercial ?? empresa.RazonSocial,
                    address = new
                    {
                        direccion = empresa.DireccionFiscal,
                        provincia = empresa.Provincia,
                        departamento = empresa.Departamento,
                        distrito = empresa.Distrito,
                        ubigueo = empresa.Ubigeo
                    }
                },

                mtoOperGravadas = mtoOperGravadas,
                mtoIGV = mtoIgv,
                valorVenta = mtoOperGravadas,
                totalImpuestos = mtoIgv,
                subTotal = total,
                mtoImpVenta = total,

                details = detallesPayload,

                legends = new[]
                {
            new
            {
                code = "1000",
                value = ObtenerLeyendaMonto(total)
            }
        }
            };
        }
        private static List<DetallePayloadApisPeru> ConstruirDetallesPayload(
            List<DetalleSunatTemporal> detallesSunat)
        {
            return detallesSunat.Select(detalle =>
            {
                var valorUnitario = Math.Round(detalle.PrecioUnitarioConIgv / 1.18m, 6);
                var valorVenta = Math.Round(detalle.TotalConIgv / 1.18m, 2);
                var igv = Math.Round(detalle.TotalConIgv - valorVenta, 2);

                var tipoAfectacionIgv = int.TryParse(detalle.TipoAfectacionIgv, out var tipoAfe)
                    ? tipoAfe
                    : 10;

                return new DetallePayloadApisPeru
                {
                    codProducto = detalle.CodProducto,
                    unidad = detalle.UnidadSunat,
                    descripcion = detalle.Descripcion,
                    cantidad = detalle.Cantidad,
                    mtoValorUnitario = valorUnitario,
                    mtoValorVenta = valorVenta,
                    mtoBaseIgv = valorVenta,
                    porcentajeIgv = 18,
                    igv = igv,
                    tipAfeIgv = tipoAfectacionIgv,
                    totalImpuestos = igv,
                    mtoPrecioUnitario = detalle.PrecioUnitarioConIgv
                };
            }).ToList();
        }

        private static object ConstruirClienteSunat(Venta venta, Empresa empresa)
        {
            var tipoDocumento = venta.ClienteTipoDoc?.Trim().ToUpper();
            var numeroDocumento = venta.ClienteNumeroDoc?.Trim();
            var nombreCliente = venta.ClienteNombre?.Trim();

            if (string.IsNullOrWhiteSpace(tipoDocumento) ||
                string.IsNullOrWhiteSpace(numeroDocumento) ||
                string.IsNullOrWhiteSpace(nombreCliente))
            {
                if (venta.TipoDocumento == "01")
                    throw new Exception("Para factura el cliente con RUC es obligatorio.");

                return new
                {
                    tipoDoc = "6",
                    numDoc = long.Parse(empresa.Ruc),
                    rznSocial = "Cliente",
                    address = new
                    {
                        direccion = "Direccion cliente",
                        provincia = empresa.Provincia,
                        departamento = empresa.Departamento,
                        distrito = empresa.Distrito,
                        ubigueo = empresa.Ubigeo
                    }
                };
            }

            var tipoSunat = tipoDocumento switch
            {
                "DNI" => "1",
                "1" => "1",
                "RUC" => "6",
                "6" => "6",
                "CE" => "4",
                "4" => "4",
                "PASAPORTE" => "7",
                "7" => "7",
                "0" => "0",
                "SIN_DOC" => "0",
                _ => "0"
            };

            if (venta.TipoDocumento == "01" && tipoSunat != "6")
                throw new Exception("Para factura el cliente debe tener RUC.");

            object numDoc = tipoSunat == "6" || tipoSunat == "1"
                ? long.Parse(numeroDocumento)
                : numeroDocumento;

            return new
            {
                tipoDoc = tipoSunat,
                numDoc = numDoc,
                rznSocial = nombreCliente,
                address = new
                {
                    direccion = venta.ClienteDireccion ?? "-",
                    provincia = empresa.Provincia,
                    departamento = empresa.Departamento,
                    distrito = empresa.Distrito,
                    ubigueo = empresa.Ubigeo
                }
            };
        }

        private static void ValidarEmpresa(Empresa empresa)
        {
            if (empresa == null)
                throw new Exception("La empresa emisora es obligatoria.");

            if (string.IsNullOrWhiteSpace(empresa.Ruc))
                throw new Exception("La empresa no tiene RUC configurado.");

            if (string.IsNullOrWhiteSpace(empresa.RazonSocial))
                throw new Exception("La empresa no tiene razón social configurada.");

            if (string.IsNullOrWhiteSpace(empresa.DireccionFiscal))
                throw new Exception("La empresa no tiene dirección fiscal configurada.");
        }

        private static string ObtenerLeyendaMonto(decimal total)
        {
            total = Math.Round(total, 2);

            var entero = (int)Math.Floor(total);
            var centimos = (int)Math.Round((total - entero) * 100);

            if (centimos == 100)
            {
                entero += 1;
                centimos = 0;
            }

            return $"SON {NumeroEnteroALetras(entero)} CON {centimos:00}/100 SOLES";
        }

        private static string NumeroEnteroALetras(int numero)
        {
            if (numero == 0) return "CERO";

            if (numero < 0)
                return "MENOS " + NumeroEnteroALetras(Math.Abs(numero));

            string[] unidades =
            {
                "",
                "UNO",
                "DOS",
                "TRES",
                "CUATRO",
                "CINCO",
                "SEIS",
                "SIETE",
                "OCHO",
                "NUEVE",
                "DIEZ",
                "ONCE",
                "DOCE",
                "TRECE",
                "CATORCE",
                "QUINCE",
                "DIECISÉIS",
                "DIECISIETE",
                "DIECIOCHO",
                "DIECINUEVE",
                "VEINTE",
                "VEINTIUNO",
                "VEINTIDÓS",
                "VEINTITRÉS",
                "VEINTICUATRO",
                "VEINTICINCO",
                "VEINTISÉIS",
                "VEINTISIETE",
                "VEINTIOCHO",
                "VEINTINUEVE"
            };

            string[] decenas =
            {
                "",
                "",
                "VEINTE",
                "TREINTA",
                "CUARENTA",
                "CINCUENTA",
                "SESENTA",
                "SETENTA",
                "OCHENTA",
                "NOVENTA"
            };

            string[] centenas =
            {
                "",
                "CIENTO",
                "DOSCIENTOS",
                "TRESCIENTOS",
                "CUATROCIENTOS",
                "QUINIENTOS",
                "SEISCIENTOS",
                "SETECIENTOS",
                "OCHOCIENTOS",
                "NOVECIENTOS"
            };

            if (numero < 30)
                return unidades[numero];

            if (numero < 100)
            {
                var decena = numero / 10;
                var unidad = numero % 10;

                return unidad == 0
                    ? decenas[decena]
                    : $"{decenas[decena]} Y {unidades[unidad]}";
            }

            if (numero == 100)
                return "CIEN";

            if (numero < 1000)
            {
                var centena = numero / 100;
                var resto = numero % 100;

                return resto == 0
                    ? centenas[centena]
                    : $"{centenas[centena]} {NumeroEnteroALetras(resto)}";
            }

            if (numero < 2000)
            {
                var resto = numero % 1000;

                return resto == 0
                    ? "MIL"
                    : $"MIL {NumeroEnteroALetras(resto)}";
            }

            if (numero < 1000000)
            {
                var miles = numero / 1000;
                var resto = numero % 1000;

                return resto == 0
                    ? $"{NumeroEnteroALetras(miles)} MIL"
                    : $"{NumeroEnteroALetras(miles)} MIL {NumeroEnteroALetras(resto)}";
            }

            if (numero < 2000000)
            {
                var resto = numero % 1000000;

                return resto == 0
                    ? "UN MILLÓN"
                    : $"UN MILLÓN {NumeroEnteroALetras(resto)}";
            }

            var millones = numero / 1000000;
            var restoMillones = numero % 1000000;

            return restoMillones == 0
                ? $"{NumeroEnteroALetras(millones)} MILLONES"
                : $"{NumeroEnteroALetras(millones)} MILLONES {NumeroEnteroALetras(restoMillones)}";
        }

        private class DetallePayloadApisPeru
        {
            public string codProducto { get; set; } = string.Empty;
            public string unidad { get; set; } = "NIU";
            public string descripcion { get; set; } = string.Empty;
            public decimal cantidad { get; set; }
            public decimal mtoValorUnitario { get; set; }
            public decimal mtoValorVenta { get; set; }
            public decimal mtoBaseIgv { get; set; }
            public decimal porcentajeIgv { get; set; }
            public decimal igv { get; set; }
            public int tipAfeIgv { get; set; }
            public decimal totalImpuestos { get; set; }
            public decimal mtoPrecioUnitario { get; set; }
        }
    }
}
