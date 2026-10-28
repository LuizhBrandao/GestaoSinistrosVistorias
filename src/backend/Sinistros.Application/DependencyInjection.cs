using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Sinistros.Domain.DomainServices;

namespace Sinistros.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // Registro de Domain Services puros
        services.AddScoped<AlcadaAprovacaoService>();
        services.AddScoped<MotorCalculoIndenizacaoService>();

        return services;
    }
}
