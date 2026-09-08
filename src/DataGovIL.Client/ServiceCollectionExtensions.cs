using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGovIL.Client;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ICkanApiClient"/> backed by a named, pooled HttpClient.
    /// Binds <see cref="CkanClientOptions"/> from the given configuration section
    /// (defaults to "DataGovIL").
    /// </summary>
    public static IServiceCollection AddDataGovIlClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName = "DataGovIL")
    {
        services.Configure<CkanClientOptions>(configuration.GetSection(configSectionName));

        services.AddHttpClient<ICkanApiClient, CkanApiClient>();

        return services;
    }

    /// <summary>Registers the client with options configured in code instead of from IConfiguration.</summary>
    public static IServiceCollection AddDataGovIlClient(
        this IServiceCollection services,
        Action<CkanClientOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddHttpClient<ICkanApiClient, CkanApiClient>();
        return services;
    }
}
