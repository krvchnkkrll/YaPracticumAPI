using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Infrastructure.Identity;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void AddPersistence(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("Postgres");
        builder.Services.Configure<JwtOptions>(builder.Configuration.GetRequiredSection("Jwt"));

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

        builder.Services.AddSingleton<IDbContextFactory, DbContextFactory>();
        builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IBookingRepository, BookingRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
    }
}