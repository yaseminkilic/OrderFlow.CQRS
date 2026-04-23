using Bogus;
using OrderFlow.CQRS.Domain.Entities;
using OrderFlow.CQRS.Domain.Enums;

namespace OrderFlow.CQRS.Domain.Tests.TestData;

public static class OrderFaker
{
    public static Faker<OrderItem> Item(int? quantity = null, decimal? unitPrice = null) =>
        new Faker<OrderItem>()
            .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
            .RuleFor(i => i.ProductCode, f => f.Commerce.Ean8())
            .RuleFor(i => i.Quantity, f => quantity ?? f.Random.Int(1, 5))
            .RuleFor(i => i.UnitPrice, f => unitPrice ?? f.Random.Decimal(1m, 500m));

    public static Faker<Order> Order(int itemCount = 2) =>
        new Faker<Order>()
            .RuleFor(o => o.OrderNumber, f => $"ORD-{f.Random.AlphaNumeric(8).ToUpperInvariant()}")
            .RuleFor(o => o.CustomerName, f => f.Name.FullName())
            .RuleFor(o => o.CustomerEmail, f => f.Internet.Email())
            .RuleFor(o => o.ShippingAddress, f => f.Address.FullAddress())
            .RuleFor(o => o.Status, OrderStatus.Pending)
            .RuleFor(o => o.PaymentStatus, PaymentStatus.Pending)
            .RuleFor(o => o.Items, _ => Item().Generate(itemCount));
}
