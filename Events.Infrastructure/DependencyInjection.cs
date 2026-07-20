using Events.Application.Interfaces.Repositories;
using Events.Infrastructure.Interfaces;
using Events.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Events.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("Postgres");

        builder.Services.AddDbContextPool<AppDbContext>(options =>
        {
            options
                .UseSnakeCaseNamingConvention()
                .UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName);
                });
        });

        builder.Services.AddPooledDbContextFactory<AppDbContext>(options =>
        {
            options
                .UseSnakeCaseNamingConvention()
                .UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName);
                });
        });

        builder.Services.AddScoped<IDbContext>(
            provider => provider.GetRequiredService<AppDbContext>());
        
        builder.Services.AddScoped<IEventRepository, EventRepository>();
    }
}