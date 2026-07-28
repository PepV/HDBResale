using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HDBResale.Shared.Configuration;

namespace HDBResale.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register HttpClient
        services.AddHttpClient("DataGovApi", client =>
        {
            var settings = configuration.GetSection("DataGovApi").Get<DataGovApiSettings>();
            client.BaseAddress = new Uri(settings?.BaseUrl ?? "https://data.gov.sg/api/action");
            client.Timeout = TimeSpan.FromSeconds(settings?.TimeoutSeconds ?? 30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "HDBResaleAnalytics/1.0");
        });
        
        return services;
    }
}