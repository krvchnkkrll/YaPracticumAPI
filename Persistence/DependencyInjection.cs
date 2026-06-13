using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence.Contracts;
using Persistence.Contracts.Repositories;
using Persistence.Contracts.Storages;
using Persistence.Repositories;
using Persistence.Storages;

namespace Persistence;

public static class DependencyInjection
{
    public static void AddPersistence(this IHostApplicationBuilder builder)
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

        builder.Services.AddScoped<IDbContextFactory, DbContextFactory>();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IBookingRepository, BookingRepository>();
        
        builder.Services.AddSingleton<IEventStorage, EventStorage>();
        builder.Services.AddSingleton<IBookingStorage, BookingStorage>();
    }
}