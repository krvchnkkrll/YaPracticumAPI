using Application;
using Persistence;
using Web;
using Web.Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplication();
builder.AddPersistence();
builder.AddWeb();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();