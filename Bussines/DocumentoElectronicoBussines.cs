using DBModel.DBModels;
using IBussines;
using IRepository;
using IService.FacturacionElectronica;
using Models.ApisPeru;
using Models.RequestResponse;
using Service.FacturacionElectronica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilPaginados.RequestResponse;

namespace Bussines
{
    public class DocumentoElectronicoBussines : IDocumentoElectronicoBussines
    {
        private readonly IDocumentoElectronicoRepository _documentoElectronicoRepository;
        private readonly IVentaRepository _ventaRepository;
        private readonly IVentaDetalleRepository _ventaDetalleRepository;
        private readonly IEmpresaRepository _empresaRepository;
        private readonly INotaCreditoRepository _notaCreditoRepository;
        private readonly IApisPeruFacturacionService _apisPeruFacturacionService;
        private readonly IApisPeruPayloadBuilder _apisPeruPayloadBuilder;

        public DocumentoElectronicoBussines(
            IDocumentoElectronicoRepository documentoElectronicoRepository,
            IVentaRepository ventaRepository,
            IVentaDetalleRepository ventaDetalleRepository,
            IEmpresaRepository empresaRepository,
            INotaCreditoRepository notaCreditoRepository,
            IApisPeruFacturacionService apisPeruFacturacionService,
            IApisPeruPayloadBuilder apisPeruPayloadBuilder)
        {
            _documentoElectronicoRepository = documentoElectronicoRepository;
            _ventaRepository = ventaRepository;
            _ventaDetalleRepository = ventaDetalleRepository;
            _empresaRepository = empresaRepository;
            _notaCreditoRepository = notaCreditoRepository;
            _apisPeruFacturacionService = apisPeruFacturacionService;
            _apisPeruPayloadBuilder = apisPeruPayloadBuilder;
        }

        public async Task<PaginacionResponse<DocumentoElectronicoResponse>> ListarAsync(
            DocumentoElectronicoFiltroRequest filtro)
        {
            var paginado = await _documentoElectronicoRepository.ListarFiltradoAsync(filtro);

            return new PaginacionResponse<DocumentoElectronicoResponse>
            {
                Items = paginado.Items
                    .Select(MapDocumento)
                    .ToList(),

                Total = paginado.Total,
                PaginaActual = paginado.PaginaActual,
                TotalPaginas = paginado.TotalPaginas,
                CantidadPorPagina = paginado.CantidadPorPagina
            };
        }

        public async Task<List<DocumentoElectronicoResponse>> ObtenerPorVentaAsync(int ventaId)
        {
            var documentos = await _documentoElectronicoRepository.ObtenerPorVentaAsync(ventaId);

            return documentos.Select(MapDocumento).ToList();
        }

        public async Task<DocumentoElectronicoResponse> ObtenerDocumentoActualVentaAsync(int ventaId)
        {
            var documento = await _documentoElectronicoRepository.ObtenerUltimoPorVentaAsync(ventaId);

            if (documento == null)
                throw new Exception("La venta no tiene documentos electrónicos.");

            return MapDocumento(documento);
        }

        public async Task<ArchivoVentaResponse> DescargarPdfActualVentaAsync(int ventaId)
        {
            var documento = await _documentoElectronicoRepository.ObtenerUltimoPorVentaAsync(ventaId);

            if (documento == null)
                throw new Exception("La venta no tiene documentos electrónicos.");

            return await DescargarPdfAsync(documento.Id);
        }

        public async Task<ArchivoVentaResponse> DescargarXmlActualVentaAsync(int ventaId)
        {
            var documento = await _documentoElectronicoRepository.ObtenerUltimoPorVentaAsync(ventaId);

            if (documento == null)
                throw new Exception("La venta no tiene documentos electrónicos.");

            return await DescargarXmlAsync(documento.Id);
        }

        public async Task<ArchivoVentaResponse> DescargarXmlAsync(int documentoElectronicoId)
        {
            var documento = await ObtenerDocumentoAsync(documentoElectronicoId);

            if (!string.IsNullOrWhiteSpace(documento.XmlEnviado))
            {
                return new ArchivoVentaResponse
                {
                    Bytes = Encoding.UTF8.GetBytes(documento.XmlEnviado),
                    ContentType = "application/xml",
                    NombreArchivo = ObtenerNombreArchivo(documento, "xml")
                };
            }

            var payload = await ConstruirPayloadDocumentoAsync(documento);

            ApisPeruArchivoResponse archivo;

            if (documento.TipoDocumento == "07")
            {
                archivo = await _apisPeruFacturacionService
                    .GenerarXmlNotaCreditoAsync(payload);
            }
            else
            {
                archivo = await _apisPeruFacturacionService
                    .GenerarXmlFacturaBoletaAsync(payload);
            }

            if (!archivo.Success || archivo.Bytes == null || archivo.Bytes.Length == 0)
            {
                throw new Exception(
                    $"No se pudo generar XML. Error APISPERU: {archivo.RawError}"
                );
            }

            return new ArchivoVentaResponse
            {
                Bytes = archivo.Bytes,
                ContentType = "application/xml",
                NombreArchivo = ObtenerNombreArchivo(documento, "xml")
            };
        }

        public async Task<ArchivoVentaResponse> DescargarPdfAsync(int documentoElectronicoId)
        {
            var documento = await ObtenerDocumentoAsync(documentoElectronicoId);

            var payload = await ConstruirPayloadDocumentoAsync(documento);

            ApisPeruArchivoResponse archivo;

            if (documento.TipoDocumento == "07")
            {
                archivo = await _apisPeruFacturacionService
                    .GenerarPdfNotaCreditoAsync(payload);
            }
            else
            {
                archivo = await _apisPeruFacturacionService
                    .GenerarPdfFacturaBoletaAsync(payload);
            }

            if (!archivo.Success || archivo.Bytes == null || archivo.Bytes.Length == 0)
            {
                throw new Exception(
                    $"No se pudo generar PDF. Error APISPERU: {archivo.RawError}"
                );
            }

            return new ArchivoVentaResponse
            {
                Bytes = archivo.Bytes,
                ContentType = "application/pdf",
                NombreArchivo = ObtenerNombreArchivo(documento, "pdf")
            };
        }

        private async Task<DocumentosElectronico> ObtenerDocumentoAsync(int id)
        {
            var documento = await _documentoElectronicoRepository.GetByIdAsync(id);

            if (documento == null)
                throw new Exception($"No se encontró el documento electrónico con id {id}.");

            return documento;
        }

        private async Task<object> ConstruirPayloadDocumentoAsync(DocumentosElectronico documento)
        {
            var venta = await _ventaRepository.GetByIdAsync(documento.VentaId);

            if (venta == null)
                throw new Exception($"No se encontró la venta {documento.VentaId}.");

            var empresa = await _empresaRepository.ObtenerEmpresaPorSucursalAsync(venta.SucursalId);

            if (empresa == null)
                throw new Exception("No se encontró la empresa emisora.");

            var detallesSunat = await ConstruirDetallesSunatDesdeVentaAsync(venta.Id);

            if (documento.TipoDocumento == "07")
            {
                var nota = await _notaCreditoRepository.ObtenerPorComprobanteAsync(
                    documento.TipoDocumento,
                    documento.Serie,
                    documento.Correlativo
                );

                if (nota == null)
                    throw new Exception($"No se encontró la nota de crédito {documento.Serie}-{documento.Correlativo}.");

                return _apisPeruPayloadBuilder.ConstruirNotasCreditoPayload(
                    nota,
                    venta,
                    detallesSunat,
                    empresa
                );
            }

            return _apisPeruPayloadBuilder.ConstruirPayloadApisPeru(
                venta,
                detallesSunat,
                empresa
            );
        }

        private async Task<List<DetalleSunatTemporal>> ConstruirDetallesSunatDesdeVentaAsync(int ventaId)
        {
            var detalles = await _ventaDetalleRepository.ObtenerPorVentaIdAsync(ventaId);

            if (detalles == null || !detalles.Any())
                throw new Exception("La venta no tiene detalles.");

            return detalles.Select(detalle => new DetalleSunatTemporal
            {
                ProductoId = detalle.ProductoId,
                PresentacionId = detalle.PresentacionId ?? 0,

                CodProducto = detalle.ProductoCodigoInterno
                 ?? detalle.CodigoBarras
                 ?? detalle.ProductoId.ToString(),

                        Descripcion = string.IsNullOrWhiteSpace(detalle.PresentacionNombre)
                 ? detalle.NombreProducto
                 : $"{detalle.NombreProducto} - {detalle.PresentacionNombre}",

                        UnidadSunat = string.IsNullOrWhiteSpace(detalle.UnidadSunat)
                 ? "NIU"
                 : detalle.UnidadSunat,

                        TipoAfectacionIgv = string.IsNullOrWhiteSpace(detalle.TipoAfectacionIgv)
                 ? "10"
                 : detalle.TipoAfectacionIgv,

                Cantidad = detalle.Cantidad,
                PrecioUnitarioConIgv = detalle.PrecioUnitario,
                DescuentoConIgv = detalle.Descuento ?? 0,
                TotalConIgv = detalle.Subtotal
            }).ToList();
        }

        private static DocumentoElectronicoResponse MapDocumento(DocumentosElectronico documento)
        {
            return new DocumentoElectronicoResponse
            {
                Id = documento.Id,
                VentaId = documento.VentaId,
                TipoDocumento = documento.TipoDocumento,
                TipoDocumentoNombre = ObtenerNombreTipoDocumento(documento.TipoDocumento),
                Serie = documento.Serie,
                Correlativo = documento.Correlativo,
                Comprobante = $"{documento.Serie}-{documento.Correlativo}",
                Estado = documento.Estado,
                CodigoRespuesta = documento.CodigoRespuesta,
                DescripcionRespuesta = documento.DescripcionRespuesta,
                CodigoHash = documento.CodigoHash,
                FechaEnvio = documento.FechaEnvio,
                FechaRespuesta = documento.FechaRespuesta,
                FechaCreacion = documento.FechaCreacion
            };
        }

        private static string ObtenerNombreTipoDocumento(string tipo)
        {
            return tipo switch
            {
                "01" => "FACTURA",
                "03" => "BOLETA",
                "07" => "NOTA DE CRÉDITO",
                "08" => "NOTA DE DÉBITO",
                _ => "DOCUMENTO"
            };
        }

        private static string ObtenerNombreArchivo(DocumentosElectronico documento, string extension)
        {
            return $"{documento.TipoDocumento}-{documento.Serie}-{documento.Correlativo}.{extension}";
        }

    }
}