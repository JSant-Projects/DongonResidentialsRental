using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;

namespace DongonResidentialsRental.Api.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("dongon_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly FakeDateTimeProvider _fakeDateTimeProvider = new();

    public DateOnly Today
    {
        get => _fakeDateTimeProvider.Today;
        set => _fakeDateTimeProvider.Today = value;
    }

    public string ConnectionString => _postgresContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the app's existing DbContext registration.
            var dbContextOptionsDescriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextOptionsDescriptor is not null)
            {
                services.Remove(dbContextOptionsDescriptor);
            }

            // If you registered ApplicationDbContext directly, remove that too.
            var dbContextDescriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));

            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            var dateTimeProviderDescriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(IDateTimeProvider));

            if (dateTimeProviderDescriptor is not null)
            {
                services.Remove(dateTimeProviderDescriptor);
            }

            services.AddSingleton<IDateTimeProvider>(_fakeDateTimeProvider);


            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(ConnectionString);
            });

            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            dbContext.Database.Migrate();
        });
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}
