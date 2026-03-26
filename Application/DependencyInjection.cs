using Application.Contracts.Services;
using Application.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;


namespace Application;

public static class DependencyInjection
{
    public static void AddApplication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IEventService, EventService>();
    }
}