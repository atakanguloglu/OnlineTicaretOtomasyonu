using AutoMapper;
using OnlineTicaretOtomasyonu.Models;
using OnlineTicaretOtomasyonu.ViewModels;

namespace OnlineTicaretOtomasyonu.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Tenant mappings
            CreateMap<Tenant, TenantViewModel>()
                .ReverseMap();

            // Product mappings
            CreateMap<Product, ProductViewModel>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ReverseMap();

            // Category mappings
            CreateMap<Category, CategoryViewModel>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
                .ReverseMap();

            // Customer mappings
            CreateMap<Customer, CustomerViewModel>()
                .ReverseMap();

            // Order mappings
            CreateMap<Order, OrderViewModel>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName))
                .ReverseMap();

            CreateMap<OrderItem, OrderItemViewModel>()
                .ReverseMap();

            // User mappings
            CreateMap<AppUser, UserViewModel>()
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant != null ? src.Tenant.Name : null))
                .ReverseMap();
        }
    }
} 