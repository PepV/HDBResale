using Microsoft.Extensions.DependencyInjection;
using HDBResale.Application.Interfaces;
using HDBResale.Application.Services;

namespace HDBResale.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IHDBService, HDBService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<ICEASalespersonService, CEASalespersonService>();
        services.AddMemoryCache();
        return services;
    }
}