using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Users.Application.Interfaces.Services;
using Users.Application.Services;

namespace Users.Application;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserService, UserService>();
        
        return builder;
    }
}