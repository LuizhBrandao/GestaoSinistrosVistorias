using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sinistros.Application.Common;
using Sinistros.Domain.Repositories;
using Sinistros.Infrastructure.Messaging;
using Sinistros.Infrastructure.Persistence;
using Sinistros.Infrastructure.Persistence.Repositories;

namespace Sinistros.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=sinistros_db;Username=postgres;Password=postgres";

        services.AddDbContext<SinistrosDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "sinistros");
            });
        });

        // Repositórios e Unidade de Trabalho
        services.AddScoped<ISinistroRepository, SinistroRepository>();
        services.AddScoped<IVistoriaRepository, VistoriaRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Mensageria RabbitMQ
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddScoped<IMessageBus, RabbitMqMessageBus>();

        // Carga de dados de demonstração
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
