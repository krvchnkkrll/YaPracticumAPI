using Application;
using Persistence;
using Persistence.Contracts;
using Web;
using Web.Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplication();
builder.AddPersistence();
builder.AddWeb();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();