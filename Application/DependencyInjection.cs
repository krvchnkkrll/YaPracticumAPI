using Application.Interfaces.Services;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application;

public static class DependencyInjection
{
    public static void AddApplication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IEventService, EventService>();
        builder.Services.AddScoped<IBookingService, BookingService>();
        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddHostedService<BookingProcessingBackgroundService>();
    }
}