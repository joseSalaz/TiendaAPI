using Bussines;
using IBussines;
using IRepository;
using IService;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Service;
using UtilInterface;

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
                <IBarcodeService, 
                BarcodeService>();
            return services;
        }
    }
}