using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Concurrent;
using System.Globalization;
using UtilPDF.ComprobantesPdf.RequestResponse;
using Document = QuestPDF.Fluent.Document;

namespace UtilPDF.ComprobantesPdf
{
    public class ComprobantePdfService : IComprobantePdfService
    {
        private static readonly ConcurrentDictionary<string, byte[]> _imageCache = new();

        public async Task<byte[]> GenerarA4Async(ComprobantePdfData data)
        {
            AsegurarQr(data);

            if (data.LogoBytes == null || data.LogoBytes.Length == 0)
                data.LogoBytes = await DescargarImagenAsync(data.LogoUrl);

            return Document.Create(container =>
            {
                CrearDocumentoA4(container, data);
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerarTicket80mmAsync(ComprobantePdfData data)
        {
            AsegurarQr(data);

            if (data.LogoBytes == null || data.LogoBytes.Length == 0)
                data.LogoBytes = await DescargarImagenAsync(data.LogoUrl);

            return Document.Create(container =>
            {
                CrearDocumentoTicket(container, data);
            }).GeneratePdf();
        }

        public byte[] GenerarTicket80mm(ComprobantePdfData data)
        {
            AsegurarQr(data);

            return Document.Create(container =>
            {
                CrearDocumentoTicket(container, data);
            }).GeneratePdf();
        }

        private static async Task<byte[]> DescargarImagenAsync(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return Array.Empty<byte>();

            if (_imageCache.TryGetValue(url, out var cachedBytes) && cachedBytes.Length > 0)
                return cachedBytes;

            try
            {
                using var httpClient = new HttpClient();

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return Array.Empty<byte>();

                var contentType = response.Content.Headers.ContentType?.MediaType;

                if (!string.IsNullOrWhiteSpace(contentType) &&
                    !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    return Array.Empty<byte>();
                }

                var bytes = await response.Content.ReadAsByteArrayAsync();

                if (bytes.Length == 0)
                    return Array.Empty<byte>();

                _imageCache[url] = bytes;

                return bytes;
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }

        private static void AsegurarQr(ComprobantePdfData data)
        {
            if (data.QrPng != null && data.QrPng.Length > 0)
                return;

            if (string.IsNullOrWhiteSpace(data.TextoQr))
                return;

            data.QrPng = CrearQrPng(data.TextoQr);
        }

        private static byte[] CrearQrPng(string textoQr)
        {
            using var generator = new QRCodeGenerator();
            using var qrData = generator.CreateQrCode(textoQr, QRCodeGenerator.ECCLevel.Q);

            var qr = new PngByteQRCode(qrData);

            return qr.GetGraphic(18);
        }

        private static void CrearDocumentoA4(IDocumentContainer container, ComprobantePdfData data)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Spacing(8);

                    // HEADER
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(120)
                            .Height(70)
                            .AlignMiddle()
                            .AlignCenter()
                            .Element(c =>
                            {
                                if (data.LogoBytes != null && data.LogoBytes.Length > 0)
                                {
                                    c.Width(65).Image(data.LogoBytes).FitArea();
                                }
                                else
                                {
                                    c.Border(0.5f)
                                     .BorderColor(Colors.Grey.Lighten1)
                                     .AlignCenter()
                                     .AlignMiddle()
                                     .Text("LOGO")
                                     .FontSize(8);
                                }
                            });

                        row.RelativeItem()
                            .AlignCenter()
                            .Column(c =>
                            {
                                c.Item()
                                    .Text(data.RazonSocialEmpresa.ToUpperInvariant())
                                    .FontSize(13)
                                    .Bold()
                                    .AlignCenter();

                                if (!string.IsNullOrWhiteSpace(data.NombreComercialEmpresa))
                                {
                                    c.Item()
                                        .Text(data.NombreComercialEmpresa.ToUpperInvariant())
                                        .FontSize(8)
                                        .SemiBold()
                                        .FontColor(Colors.Grey.Darken1)
                                        .AlignCenter();
                                }

                                c.Item()
                                    .Text(data.DireccionEmpresa)
                                    .FontSize(7.5f)
                                    .AlignCenter();

                                c.Item()
                                    .Text($"{data.DistritoEmpresa} - {data.ProvinciaEmpresa} - {data.DepartamentoEmpresa}")
                                    .FontSize(7.5f)
                                    .AlignCenter();

                                c.Item()
                                    .Text($"Telf: {data.Telefono ?? "-"}")
                                    .FontSize(7.5f)
                                    .AlignCenter();

                                c.Item()
                                    .Text($"Email: {data.Email ?? "-"}")
                                    .FontSize(7.5f)
                                    .AlignCenter();
                            });

                        row.ConstantItem(180)
                            .Border(1)
                            .BorderColor(Colors.Grey.Darken2)
                            .Padding(8)
                            .Column(c =>
                            {
                                c.Spacing(4);

                                c.Item()
                                    .AlignCenter()
                                    .Text($"R.U.C. {data.RucEmpresa}")
                                    .FontSize(10)
                                    .Bold();

                                c.Item()
                                    .Background(Colors.Grey.Lighten4)
                                    .PaddingVertical(3)
                                    .AlignCenter()
                                    .Text(data.TipoDocumentoNombre.ToUpperInvariant())
                                    .FontSize(10)
                                    .Bold();

                                c.Item()
                                    .AlignCenter()
                                    .Text($"{data.Serie} - {data.Correlativo}")
                                    .FontSize(10)
                                    .Bold();
                            });
                    });

                    // CLIENTE
                    col.Item()
                        .Border(0.5f)
                        .BorderColor(Colors.Grey.Darken1)
                        .Padding(6)
                        .Column(c =>
                        {
                            c.Spacing(2);

                            FilaClienteA4(c, "Cliente", data.ClienteNombre);
                            FilaClienteA4(c, "Dirección", string.IsNullOrWhiteSpace(data.ClienteDireccion) ? "-" : data.ClienteDireccion);
                            FilaClienteA4(c, data.ClienteTipoDocumento, data.ClienteNumeroDocumento);
                        });

                    // DATOS ADICIONALES
                    col.Item().Element(c => DatosAdicionalesA4(c, data));

                    // DETALLE
                    col.Item().Element(c => TablaItemsA4(c, data));

                    // LEYENDA + TOTALES + QR
                    col.Item().Element(c => TotalesA4(c, data));
                });

                page.Footer()
                    .PaddingTop(6)
                    .Column(footer =>
                    {
                        footer.Item()
                            .LineHorizontal(0.5f);

                        footer.Item()
                            .PaddingTop(6)
                            .Row(row =>
                            {
                                row.RelativeItem()
                                    .AlignCenter()
                                    .Text($"Representación impresa de la {data.TipoDocumentoNombre.ToUpperInvariant()} ELECTRÓNICA, Autorizado por Resolución SUNAT.")
                                    .FontSize(6.5f);
                            });
                    });
            });
        }

        private static void FilaClienteA4(ColumnDescriptor col, string etiqueta, string valor)
        {
            col.Item().Row(row =>
            {
                row.ConstantItem(80)
                    .Text(etiqueta)
                    .FontSize(8)
                    .Bold();

                row.ConstantItem(8)
                    .Text(":")
                    .FontSize(8)
                    .Bold();

                row.RelativeItem()
                    .Text(valor ?? "-")
                    .FontSize(8);
            });
        }

        private static void FilaEtiquetaA4(ColumnDescriptor col, string etiqueta, string valor)
        {
            col.Item().Row(row =>
            {
                row.ConstantItem(70)
                    .Text(etiqueta)
                    .Bold();

                row.RelativeItem()
                    .Text($": {valor ?? "---"}");
            });
        }

        private static void DatosAdicionalesA4(IContainer container, ComprobantePdfData data)
        {
            container.Column(col =>
            {
                col.Spacing(2);

                col.Item()
                    .Text("Observaciones")
                    .Bold()
                    .FontSize(7);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    HeaderCellA4(table, "FECHA EMISIÓN", 6.5f);
                    HeaderCellA4(table, "FEC. VENCIMIENTO", 6.5f);
                    HeaderCellA4(table, "ORDEN COMPRA / PEDIDO", 6.5f);
                    HeaderCellA4(table, "GUÍA", 6.5f);
                    HeaderCellA4(table, "COND. DE PAGO", 6.5f);

                    BodyCellA4(table, data.FechaEmision.ToString("dd/MM/yyyy"), true, 7);
                    BodyCellA4(table, "---", true, 7);
                    BodyCellA4(table, "---", true, 7);
                    BodyCellA4(table, "---", true, 7);
                    BodyCellA4(table, ObtenerMediosPagoTexto(data), true, 7);
                });
            });
        }

        private static void TablaItemsA4(IContainer container, ComprobantePdfData data)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(55);
                    columns.ConstantColumn(45);
                    columns.RelativeColumn();
                    columns.ConstantColumn(75);
                    columns.ConstantColumn(85);
                });

                HeaderCellA4(table, "CANTIDAD.", 7);
                HeaderCellA4(table, "U.M.", 7);
                HeaderCellA4(table, "DESCRIPCIÓN", 7);
                HeaderCellA4(table, "PRECIO UNIT.", 7);
                HeaderCellA4(table, "IMPORTE (Inc. IGV)", 7);

                foreach (var item in data.Detalles)
                {
                    var importe = (item.Cantidad * item.PrecioUnitario) - item.Descuento;

                    BodyCellDetalleA4(table, $"{item.Cantidad:0.##}", true);
                    BodyCellDetalleA4(table, item.Unidad ?? "NIU", true);
                    BodyCellDetalleA4(table, item.Descripcion, false);
                    BodyCellDetalleA4(table, item.PrecioUnitario.ToString("0.00"), true);
                    BodyCellDetalleA4(table, importe.ToString("0.00"), true);
                }

                var minFilas = 9;
                var filasFaltantes = minFilas - data.Detalles.Count;

                if (filasFaltantes > 0)
                {
                    for (var i = 0; i < filasFaltantes; i++)
                    {
                        BodyCellDetalleA4(table, "", true);
                        BodyCellDetalleA4(table, "", true);
                        BodyCellDetalleA4(table, "", false);
                        BodyCellDetalleA4(table, "", true);
                        BodyCellDetalleA4(table, "", true);
                    }
                }

                // Borde inferior final
                table.Cell().BorderTop(0.5f).Text("");
                table.Cell().BorderTop(0.5f).Text("");
                table.Cell().BorderTop(0.5f).Text("");
                table.Cell().BorderTop(0.5f).Text("");
                table.Cell().BorderTop(0.5f).Text("");
            });
        }

        private static void TotalesA4(IContainer container, ComprobantePdfData data)
        {
            container.Column(col =>
            {
                col.Spacing(8);

                col.Item()
                    .PaddingTop(4)
                    .Text(ObtenerLeyendaSimple(data.Total))
                    .Bold()
                    .FontSize(7.5f);

                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        if (data.QrPng != null && data.QrPng.Length > 0)
                        {
                            left.Item()
                                .Width(78)
                                .Image(data.QrPng)
                                .FitWidth();
                        }

                        left.Item()
                            .PaddingTop(4)
                            .Text($"FORMA DE PAGO: {ObtenerMediosPagoTexto(data)}")
                            .FontSize(7);

                        left.Item()
                            .Text("COND. VENTA: CONTADO")
                            .FontSize(7);
                    });

                    row.ConstantItem(230)
                        .Border(0.5f)
                        .BorderColor(Colors.Grey.Darken1)
                        .Padding(6)
                        .Column(right =>
                        {
                            var totalDescuentos = data.DescuentoGlobal + data.Detalles.Sum(x => x.Descuento);

                            FilaTotalA4(right, "OP. GRAVADA (S/)", Money(data.Subtotal).Replace("S/ ", ""), false);

                            if (totalDescuentos > 0)
                                FilaTotalA4(right, "DESCUENTOS (S/)", Money(totalDescuentos).Replace("S/ ", ""), false);

                            FilaTotalA4(right, "TOTAL IGV (S/)", Money(data.Igv).Replace("S/ ", ""), false);

                            right.Item()
                                .PaddingVertical(3)
                                .LineHorizontal(0.5f);

                            FilaTotalA4(right, "IMPORTE TOTAL (S/)", Money(data.Total).Replace("S/ ", ""), true);
                        });
                });
            });
        }
        private static void BodyCellDetalleA4(TableDescriptor table, string text, bool center)
        {
            var cell = table.Cell()
                .BorderLeft(0.5f)
                .BorderRight(0.5f)
                .MinHeight(18)
                .PaddingHorizontal(4)
                .PaddingVertical(3)
                .Text(text ?? "")
                .FontSize(7.5f);

            if (center)
                cell.AlignCenter();
        }
        private static void FilaTotalA4(ColumnDescriptor col, string etiqueta, string valor, bool destacado)
        {
            col.Item().Row(r =>
            {
                r.RelativeItem()
                    .Text(etiqueta)
                    .FontSize(destacado ? 8.5f : 7.5f)
                    .Bold()
                    .AlignRight();

                r.ConstantItem(70)
                    .Text(valor)
                    .FontSize(destacado ? 9 : 7.5f)
                    .Bold()
                    .AlignRight();
            });
        }

        private static void CrearDocumentoTicket(IDocumentContainer container, ComprobantePdfData data)
        {
            var altoMm = 180 + (data.Detalles.Count * 14) + ((data.Pagos?.Count ?? 0) * 6);

            if (altoMm < 200)
                altoMm = 200;

            container.Page(page =>
            {
                page.Size(new PageSize(80, altoMm, Unit.Millimetre));
                page.Margin(4, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontSize(8.5f).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Spacing(3);

                    col.Item()
                        .AlignCenter()
                        .Height(40)
                        .Element(c =>
                        {
                            if (data.LogoBytes != null && data.LogoBytes.Length > 0)
                            {
                                c.Image(data.LogoBytes).FitArea();
                            }
                            else
                            {
                                c.AlignCenter()
                                 .AlignMiddle()
                                 .Text("LOGO")
                                 .FontSize(8);
                            }
                        });

                    col.Item()
                        .AlignCenter()
                        .Text(data.RazonSocialEmpresa)
                        .Bold()
                        .FontSize(10);

                    col.Item()
                        .AlignCenter()
                        .Text(data.NombreComercialEmpresa)
                        .FontSize(8)
                        .Bold();

                    col.Item()
                        .AlignCenter()
                        .Text($"RUC: {data.RucEmpresa}")
                        .Bold();

                    col.Item()
                        .AlignCenter()
                        .Text(data.DireccionEmpresa)
                        .FontSize(8);

                    col.Item()
                        .AlignCenter()
                        .Text($"Telf: {data.Telefono ?? "-"}")
                        .FontSize(8);

                    col.Item()
                        .AlignCenter()
                        .Text($"Correo: {data.Email ?? "-"}")
                        .FontSize(8);

                    col.Item()
                        .AlignCenter()
                        .Text($"{data.DistritoEmpresa} - {data.ProvinciaEmpresa} - {data.DepartamentoEmpresa}")
                        .FontSize(8);

                    col.Item()
                        .PaddingTop(3)
                        .LineHorizontal(0.5f);

                    col.Item()
                        .AlignCenter()
                        .Text($"{data.TipoDocumentoNombre.ToUpperInvariant()} ELECTRÓNICA")
                        .Bold()
                        .FontSize(9);

                    col.Item()
                        .AlignCenter()
                        .Text($"{data.Serie} - {data.Correlativo}")
                        .Bold()
                        .FontSize(10);

                    col.Item().LineHorizontal(0.5f);

                    col.Item()
                        .AlignCenter()
                        .Text(data.ClienteNombre);

                    col.Item()
                        .AlignCenter()
                        .Text(string.IsNullOrWhiteSpace(data.ClienteDireccion) ? "---" : data.ClienteDireccion);

                    col.Item()
                        .AlignCenter()
                        .Text($"{data.ClienteTipoDocumento} {data.ClienteNumeroDocumento}")
                        .Bold();

                    col.Item()
                        .Text($"FECHA: {data.FechaEmision:dd/MM/yyyy}   HORA: {data.FechaEmision:hh:mm tt}")
                        .FontSize(8);

                    col.Item().LineHorizontal(0.5f);

                    col.Item().Row(row =>
                    {
                        row.ConstantItem(25).Text("Cant").Bold();
                        row.ConstantItem(30).Text("U.M").Bold();
                        row.RelativeItem().Text("COD").Bold();
                        row.ConstantItem(45).Text("PRECIO").Bold().AlignRight();
                        row.ConstantItem(45).Text("TOTAL").Bold().AlignRight();
                    });

                    col.Item()
                        .Text("DESCRIPCION")
                        .Bold()
                        .FontSize(8);

                    col.Item().LineHorizontal(0.5f);

                    foreach (var item in data.Detalles)
                    {
                        col.Item().Row(row =>
                        {
                            row.ConstantItem(25).Text($"{item.Cantidad:0.##}");
                            row.ConstantItem(30).Text(item.Unidad ?? "UNID");
                            row.RelativeItem().Text(string.IsNullOrWhiteSpace(item.Codigo) ? "-" : item.Codigo);
                            row.ConstantItem(45).Text(item.PrecioUnitario.ToString("0.00")).AlignRight();
                            row.ConstantItem(45).Text(item.Subtotal.ToString("0.00")).AlignRight();
                        });

                        col.Item()
                            .Text(item.Descripcion)
                            .FontSize(8);
                    }

                    col.Item().LineHorizontal(0.5f);

                    var totalDescuentos = data.DescuentoGlobal + data.Detalles.Sum(x => x.Descuento);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("TOTAL GRAVADO").Bold();
                        row.ConstantItem(35).Text("(S/)").AlignRight();
                        row.ConstantItem(50).Text(data.Subtotal.ToString("0.00")).Bold().AlignRight();
                    });

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("I.G.V").Bold();
                        row.ConstantItem(35).Text("(S/)").AlignRight();
                        row.ConstantItem(50).Text(data.Igv.ToString("0.00")).Bold().AlignRight();
                    });

                    if (totalDescuentos > 0)
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("DESCUENTOS").Bold();
                            row.ConstantItem(35).Text("(S/)").AlignRight();
                            row.ConstantItem(50).Text(totalDescuentos.ToString("0.00")).Bold().AlignRight();
                        });
                    }

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("TOTAL").FontSize(11).Bold();
                        row.ConstantItem(35).Text("(S/)").FontSize(11).Bold().AlignRight();
                        row.ConstantItem(55).Text(data.Total.ToString("0.00")).FontSize(12).Bold().AlignRight();
                    });

                    col.Item()
                        .PaddingTop(2)
                        .Text(ObtenerLeyendaSimple(data.Total))
                        .FontSize(8);

                    col.Item()
                        .Text($"FORMA DE PAGO: {ObtenerMediosPagoTexto(data)}")
                        .FontSize(8);

                    col.Item()
                        .Text("COND. VENTA: CONTADO")
                        .FontSize(8);

                    col.Item()
                        .Text("Observaciones:")
                        .FontSize(8)
                        .Bold();

                    if (data.QrPng != null && data.QrPng.Length > 0)
                    {
                        col.Item()
                            .PaddingTop(5)
                            .AlignCenter()
                            .Width(75)
                            .Image(data.QrPng)
                            .FitWidth();
                    }

                    col.Item()
                        .Text($"Resumen: {data.Serie}-{data.Correlativo}")
                        .FontSize(7);

                    col.Item()
                        .PaddingTop(2)
                        .AlignCenter()
                        .Text($"Representación Impresa de la {data.TipoDocumentoNombre.ToUpperInvariant()} ELECTRÓNICA")
                        .FontSize(7.5f)
                        .AlignCenter();

                    col.Item()
                        .AlignCenter()
                        .Text("Autorizado mediante Resolución SUNAT")
                        .FontSize(7.5f);
                });
            });
        }

        private static void HeaderCellA4(TableDescriptor table, string text, float fontSize)
        {
            table.Cell()
                .Border(0.5f)
                .BorderColor(Colors.Grey.Darken1)
                .Background(Colors.Grey.Lighten4)
                .MinHeight(16)
                .Padding(3)
                .AlignMiddle()
                .Text(text)
                .Bold()
                .FontSize(fontSize)
                .AlignCenter();
        }

        private static void BodyCellA4(TableDescriptor table, string text, bool center, float fontSize)
        {
            var descriptor = table.Cell()
                .BorderLeft(0.5f)
                .BorderRight(0.5f)
                .MinHeight(15)
                .Padding(3)
                .AlignMiddle()
                .Text(text ?? "")
                .FontSize(fontSize);

            if (center)
                descriptor.AlignCenter();
        }

        private static string ObtenerMediosPagoTexto(ComprobantePdfData data)
        {
            var mediosPago = data.Pagos?
                .Where(p => !string.IsNullOrWhiteSpace(p.MedioPago))
                .Select(p => p.MedioPago.Trim().ToUpperInvariant())
                .Distinct()
                .ToList() ?? new List<string>();

            return mediosPago.Any()
                ? string.Join(" / ", mediosPago)
                : "CONTADO";
        }

        private static string Money(decimal value)
        {
            return $"S/ {Math.Round(value, 2).ToString("0.00", CultureInfo.InvariantCulture)}";
        }

        private static string ObtenerLeyendaSimple(decimal total)
        {
            total = Math.Round(total, 2);

            var entero = (int)Math.Floor(total);
            var centimos = (int)Math.Round((total - entero) * 100);

            if (centimos == 100)
            {
                entero += 1;
                centimos = 0;
            }

            return $"SON: {NumeroEnteroALetras(entero)} CON {centimos:00}/100 SOLES";
        }

        private static string NumeroEnteroALetras(int numero)
        {
            if (numero == 0)
                return "CERO";

            if (numero < 0)
                return "MENOS " + NumeroEnteroALetras(Math.Abs(numero));

            string[] unidades =
            {
                "", "UNO", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE",
                "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISÉIS", "DIECISIETE",
                "DIECIOCHO", "DIECINUEVE", "VEINTE", "VEINTIUNO", "VEINTIDÓS", "VEINTITRÉS",
                "VEINTICUATRO", "VEINTICINCO", "VEINTISÉIS", "VEINTISIETE", "VEINTIOCHO", "VEINTINUEVE"
            };

            string[] decenas =
            {
                "", "", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA",
                "SESENTA", "SETENTA", "OCHENTA", "NOVENTA"
            };

            string[] centenas =
            {
                "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS",
                "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS"
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
    }
}