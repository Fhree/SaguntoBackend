using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Features.Orders;
using Sagunto.Application.Interfaces;
using Sagunto.Infrastructure.Data;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SaguntoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<ISaguntoDbContext>(provider =>
    provider.GetRequiredService<SaguntoDbContext>());

builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(CreateNewOrderCommandHandler).Assembly);
});
builder.Services.AddWolverineHttp();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapWolverineEndpoints();
app.Run();