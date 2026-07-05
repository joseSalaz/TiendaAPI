using Bussines;
using IBussines;
using IRepository;
using IService.ConsultaDNI_RUC;
using IService.FacturacionElectronica;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Service.ConsultaDNI_RUC;
using Service.FacturacionElectronica;
using UtilInterface;
using UtilPDF.ComprobantesPdf;

namespace IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection
            AddDependencyInjection(
                this IServiceCollection services)
        {
            services.AddScoped
                <IUnitOfWork,
                UnitOfWork>();

            services.AddScoped
                <IProductoRepository,
                ProductoRepository>();

            services.AddScoped
                <IProductoBussines,
                ProductoBussines>();

            services.AddScoped
                <ICajaSesionRepository,
                CajaSesionRepository>();

            services.AddScoped
                <ICajaSesionBussines,
                CajaSesionBussines>();

            services.AddScoped
                <IClienteRepository, ClienteRepository>();

            services.AddScoped
                <IClienteBussines, ClienteBussines>();

            services.AddScoped
                <IProductoStockRepository,
                ProductoStockRepository>();

            services.AddScoped
                <IProductoPresentacionBussines,
                ProductoPresentacionBussines>();

            services.AddScoped
                <IProductoPresentacionRepository,
                ProductoPresentacionRepository>();

            services.AddScoped
                <ISucursalRepository,
                SucursalRepository>();
            services.AddScoped
                <IProductoLoteBussines, ProductoLoteBussines>();
            services.AddScoped
                <IProductoLoteRepository, ProductoLoteRepository>();
            services.AddScoped
                <IStockMovimientoRepository, StockMovimientoRepository>();
            services.AddScoped<IVentaBussines, VentaBussines>();

            services.AddScoped<IVentaRepository, VentaRepository>();
            services.AddScoped<IVentaDetalleRepository, VentaDetalleRepository>();
            services.AddScoped<IVentaPagoRepository, VentaPagoRepository>();
            services.AddScoped<IVentaDetalleLoteRepository, VentaDetalleLoteRepository>();

            services.AddScoped<IComprobanteSerieRepository, ComprobanteSerieRepository>();

            services.AddScoped<ICajaMovimientoRepository, CajaMovimientoRepository>();
            services.AddScoped<IMedioPagoRepository, MedioPagoRepository>();
            services.AddScoped<ICajaSesionBussines, CajaSesionBussines>();

            services.AddScoped<ICajaSesionRepository, CajaSesionRepository>();
            services.AddScoped<ICajaMovimientoRepository, CajaMovimientoRepository>();
            services.AddScoped<ICajaRepository, CajaRepository>();
            services.AddScoped<IEmpresaRepository, EmpresaRepository>();
            services.AddHttpClient<IApisPeruFacturacionService, ApisPeruFacturacionService>();
            services.AddScoped<IDocumentoElectronicoRepository, DocumentoElectronicoRepository>();
            services.AddScoped<IApisPeruConsultaDocumentoService, ApisPeruConsultaDocumentoService>();
            services.AddScoped<INotaCreditoRepository, NotaCreditoRepository>();
            services.AddScoped<IDocumentoElectronicoBussines, DocumentoElectronicoBussines>();
            services.AddScoped<IApisPeruPayloadBuilder, ApisPeruPayloadBuilder>();
            services.AddScoped<IComprobantePdfService, ComprobantePdfService>();
            return services;
        }
    }
}