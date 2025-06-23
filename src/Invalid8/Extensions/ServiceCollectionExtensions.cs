using Invalid8.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Invalid8.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInvalid8(this IServiceCollection services)
    {
        services.AddSingleton<IQueryClient, QueryClient>();

        return services;
    }
}