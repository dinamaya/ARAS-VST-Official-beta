using ARAS.Main.Oracle.Api.App_Code.Globals.Extensions;
using ARAS.Main.Oracle.Api.Context;
using ARAS.Main.Oracle.Api.Repositories.Implementations;
using ARAS.Main.Oracle.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

var builder = WebApplication.CreateBuilder(args);
string mainDbConnection = builder.Configuration.GetConnectionString("MainDbContext") ?? throw new Exception("Main Oracle Database Context not found");

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<MainDbContext>(options =>
	options.UseOracle(mainDbConnection));

builder.Services.AddTransient<Func<Task<OracleConnection>>>(sp => async () =>
{
	var conn = new OracleConnection(mainDbConnection);
	await conn.OpenWithPolicyContextAsync();
	return conn;
});

builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
