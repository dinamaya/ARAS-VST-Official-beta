using ARAS.OracleSync.Worker;
using ARAS.OracleSync.Worker.Implementations;
using ARAS.OracleSync.Worker.Interfaces;
using System.Net.Http.Headers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IBaseService, BaseService>();
builder.Services.AddSingleton<IConfigService, ConfigService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
