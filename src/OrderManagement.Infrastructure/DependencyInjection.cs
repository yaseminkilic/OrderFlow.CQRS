using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.CQRS.Application.Interfaces;
using OrderFlow.CQRS.Domain.Interfaces;
using OrderFlow.CQRS.Infrastructure.Data;
using OrderFlow.CQRS.Infrastructure.Messaging;
using OrderFlow.CQRS.Infrastructure.Repositories;

namespace OrderFlow.CQRS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Entity Framework Core
        var dbConfig = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(dbConfig, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        // RabbitMQ
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
        services.AddHostedService<OrderProcessingConsumer>();

        return services;
    }
}
