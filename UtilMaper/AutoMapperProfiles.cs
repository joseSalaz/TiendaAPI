using AutoMapper;
using DBModel.DBModels;
using Models.RequestResponse;

namespace UtilMaper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {

            CreateMap<Producto, ProductoResponse>().ForMember(dest => dest.Presentaciones,
        opt => opt.MapFrom(src => src.ProductoPresentaciones))
        .ReverseMap();

            CreateMap<ProductoRequest, Producto>()
                .ReverseMap();
            CreateMap<Cliente, ClienteResponse>().ReverseMap();
            CreateMap<ClienteRequest, Cliente>().ReverseMap();
            CreateMap<ProductoPresentacionRequest, ProductoPresentacione>().ReverseMap();
            CreateMap<ProductoPresentacione, ProductoPresentacionResponse>().ReverseMap();
        }
    }
}
