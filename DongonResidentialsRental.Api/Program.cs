using DongonResidentialsRental.Api.Common.Extensions;
using DongonResidentialsRental.Api.Common.Middleware;
using DongonResidentialsRental.Application;
using DongonResidentialsRental.Infrastracture;
using DongonResidentialsRental.Persistence;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter(
                JsonNamingPolicy.CamelCase, 
                allowIntegerValues: false));
});

var config = builder.Configuration;
builder.Services
    .AddApplication()
    .AddPersistence(config)
    .AddInfrastructure();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(
        options =>
        {
            options
            .WithTitle("Dongon Residentials Rental API")
            .WithTheme(ScalarTheme.Moon)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            
        });
}


app.UseHttpsRedirection();

app.UseExceptionHandler();

app.MapApiEndpoints();

app.Run();
