using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence.Contracts.Repositories;
using Persistence.Contracts.Storages;
using Persistence.Repositories;
using Persistence.Storages;

namespace Persistence;

public static class DependencyInjection
{
    public static void AddPersistence(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IBookingRepository, BookingRepository>();
        
        // Storages
        builder.Services.AddSingleton<IEventStorage, EventStorage>();
        builder.Services.AddSingleton<IBookingStorage, BookingStorage>();
    }
}