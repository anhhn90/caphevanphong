using CapheVanPhong.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CapheVanPhong.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IHotNewsService, HotNewsService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IBlogCategoryService, BlogCategoryService>();
        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<ICommercialServiceService, CommercialServiceService>();

        return services;
    }
}
