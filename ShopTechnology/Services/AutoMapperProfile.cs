using AutoMapper;
using ShopTechnology.DTOs;
using ShopTechnology.Models;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDTO>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : string.Empty))
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.ProductImages.Select(pi => pi.ImageUrl)))
            .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src =>
                src.ProductImages.Where(pi => pi.IsMain).Select(pi => pi.ImageUrl).FirstOrDefault() ??
                src.ProductImages.Select(pi => pi.ImageUrl).FirstOrDefault() ?? string.Empty));

        CreateMap<CreateProductDTO, Product>();
        CreateMap<UpdateProductDTO, Product>();

        // ProductDTO to ProductViewModel mapping
        CreateMap<ProductDTO, ProductViewModel>();

        // Order mappings
        CreateMap<Order, OrderDTO>()
            .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.Payment != null ? src.Payment.Method : string.Empty))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.Payment != null ? src.Payment.Status : string.Empty));

        CreateMap<CreateOrderDTO, Order>();
        CreateMap<UpdateOrderStatusDTO, Order>();

        // OrderDTO to OrderViewModel mapping
        CreateMap<OrderDTO, OrderViewModel>();

        // OrderDetail mappings
        CreateMap<OrderDetail, OrderDetailDTO>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : string.Empty))
            .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src =>
                src.Product != null ?
                (src.Product.ProductImages.Where(pi => pi.IsMain).Select(pi => pi.ImageUrl).FirstOrDefault() ??
                 src.Product.ProductImages.Select(pi => pi.ImageUrl).FirstOrDefault() ?? string.Empty) : string.Empty));

        CreateMap<CreateOrderDetailDTO, OrderDetail>();

        // User mappings
        CreateMap<User, UserDTO>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : string.Empty));

        CreateMap<CreateUserDTO, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password)); // Will be hashed in service

        CreateMap<UpdateUserDTO, User>();

        // Category mappings
        CreateMap<Category, CategoryDTO>();
        CreateMap<CreateCategoryDTO, Category>();
        CreateMap<UpdateCategoryDTO, Category>();

        // Cart mappings
        CreateMap<Cart, CartDTO>()
            .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty));

        CreateMap<CartItem, CartItemDTO>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : string.Empty))
            .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src =>
                src.Product != null ?
                (src.Product.ProductImages.Where(pi => pi.IsMain).Select(pi => pi.ImageUrl).FirstOrDefault() ??
                 src.Product.ProductImages.Select(pi => pi.ImageUrl).FirstOrDefault() ?? string.Empty) : string.Empty))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product != null ? src.Product.Price : 0));

        // Wishlist mappings
        CreateMap<Wishlist, WishlistDTO>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : string.Empty))
            .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src =>
                src.Product != null ?
                (src.Product.ProductImages.Where(pi => pi.IsMain).Select(pi => pi.ImageUrl).FirstOrDefault() ??
                 src.Product.ProductImages.Select(pi => pi.ImageUrl).FirstOrDefault() ?? string.Empty) : string.Empty))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product != null ? src.Product.Price : 0))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Product != null && src.Product.Category != null ? src.Product.Category.CategoryName : string.Empty));
    }
}
