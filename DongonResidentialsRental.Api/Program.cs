using DongonResidentialsRental.Application;
using DongonResidentialsRental.Infrastracture;
using DongonResidentialsRental.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var config = builder.Configuration;
builder.Services.AddApplication();
builder.Services.AddPersistence(config);
builder.Services.AddInfrastructure();

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

app.Run();
