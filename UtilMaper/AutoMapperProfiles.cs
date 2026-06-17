using AutoMapper;
using DBModel.Models;
using Models.RequestResponse;

namespace UtilMaper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles() {

            CreateMap<Producto, ProductoResponse>()
        .ReverseMap();

            CreateMap<ProductoRequest, Producto>()
                .ReverseMap();
        }
    }
}
