using Bookings.Application.Interfaces.Events;
using Bookings.Application.Interfaces.Repositories;
using Bookings.Infrastructure.Events;
using Bookings.Infrastructure.Interfaces;
using Bookings.Infrastructure.Models;
using Bookings.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bookings.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("Postgres");

        builder.Services.Configure<KafkaOptions>(builder.Configuration.GetRequiredSection("Kafka"));

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
        
        builder.Services.AddScoped<IBookingRepository, BookingRepository>();

        builder.Services.AddSingleton<IBookingEventPublisher, KafkaBookingEventPublisher>();
    }
}