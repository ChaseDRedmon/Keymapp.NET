using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Keymapp.NET;

public static class KeymappServiceExtensions
{
    public static IServiceCollection AddKeymappServices(this IServiceCollection services, GrpcChannel channel)
    {
        services.AddScoped<IKeymappApi, KeymappApi>(_ => new KeymappApi(channel));
        return services;
    }
}