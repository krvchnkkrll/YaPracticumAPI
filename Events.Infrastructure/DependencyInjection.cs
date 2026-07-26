using Events.Application.Interfaces.Cache;
using Events.Application.Interfaces.Repositories;
using Events.Infrastructure.Cache;
using Events.Infrastructure.Events;
using Events.Infrastructure.Interfaces;
using Events.Infrastructure.Models;
using Events.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Events.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("Postgres");
        
        builder.Services.Configure<KafkaOptions>(builder.Configuration.GetRequiredSection("Kafka"));

        builder.Services.Configure<RedisOptions>(builder.Configuration.GetRequiredSection("Redis"));
        var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
        
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
        
        var redisConfiguration = ConfigurationOptions.Parse(redisConnectionString!);
        redisConfiguration.AbortOnConnectFail = false;

        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfiguration));
        builder.Services.AddSingleton<IDatabase>(provider => provider.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
        builder.Services.AddScoped<IEventsCached, EventsCached>();

        builder.Services.AddScoped<IDbContext>(
            provider => provider.GetRequiredService<AppDbContext>());

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        
        builder.Services.AddHostedService<KafkaTopicInitializerHostedService>();
        builder.Services.AddHostedService<KafkaBookingConfirmedConsumerWorker>();
        builder.Services.AddHostedService<KafkaBookingCancelledConsumerWorker>();
    }
}