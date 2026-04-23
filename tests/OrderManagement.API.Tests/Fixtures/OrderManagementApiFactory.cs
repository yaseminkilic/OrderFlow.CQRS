using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OrderFlow.CQRS.Application.Interfaces;
using OrderFlow.CQRS.Infrastructure.Data;
using Testcontainers.MsSql;

namespace OrderFlow.CQRS.API.Tests.Fixtures;

public sealed class OrderManagementApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sql = new MsSqlBuilder()
        .WithPassword("Str0ng!Passw0rd")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            RemoveDescriptor<DbContextOptions<ApplicationDbContext>>(services);
            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(_sql.GetConnectionString()));

            RemoveDescriptor<IMessagePublisher>(services);
            services.AddSingleton<IMessagePublisher, FakeMessagePublisher>();

            RemoveHostedService<Infrastructure.Messaging.OrderProcessingConsumer>(services);
        });
    }

    public async Task InitializeAsync()
    {
        await _sql.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public new Task DisposeAsync() => _sql.DisposeAsync().AsTask();

    public FakeMessagePublisher Publisher => (FakeMessagePublisher)Services.GetRequiredService<IMessagePublisher>();

    private static void RemoveDescriptor<TService>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TService));
        if (descriptor is not null) services.Remove(descriptor);
    }

    private static void RemoveHostedService<THostedService>(IServiceCollection services)
        where THostedService : IHostedService
    {
        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(IHostedService) &&
            d.ImplementationType == typeof(THostedService));
        if (descriptor is not null) services.Remove(descriptor);
    }
}
