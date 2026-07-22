using Bookings.Application.Interfaces.Services;
using Bookings.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bookings.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IBookingService, BookingService>();
        builder.Services.AddScoped<IAccessService, AccessService>();

        builder.Services.AddHostedService<BookingProcessingBackgroundService>();
    }
}