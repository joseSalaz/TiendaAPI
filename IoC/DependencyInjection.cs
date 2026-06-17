using Bussines;
using IBussines;
using IRepository;
using Microsoft.Extensions.DependencyInjection;
using Repository;
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

            return services;
        }
    }
}