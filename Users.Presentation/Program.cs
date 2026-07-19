using Microsoft.EntityFrameworkCore;
using Users.Application;
using Users.Infrastructure;
using Users.Infrastructure.Interfaces;
using Users.Presentation;
using Users.Presentation.Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplication();
builder.AddPersistence();
builder.AddPresentation();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IDbContext>();
    await db.Database.MigrateAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();