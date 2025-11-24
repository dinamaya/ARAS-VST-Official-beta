using Microsoft.Extensions.Configuration;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
bool isProduction = builder.Environment.IsProduction();

builder.Configuration.AddJsonFile(isProduction ? "ocelot.Production.json" : "ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

await app.UseOcelot();
await app.RunAsync();
