using Bogus;
using OrderFlow.CQRS.Application.Features.Orders.Commands;

namespace OrderFlow.CQRS.Application.Tests.TestData;

public static class CreateOrderCommandFaker
{
    public static CreateOrderItemDto Item(int quantity = 1, decimal unitPrice = 10m) =>
        new Faker<CreateOrderItemDto>()
            .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
            .RuleFor(i => i.ProductCode, f => f.Commerce.Ean8())
            .RuleFor(i => i.Quantity, quantity)
            .RuleFor(i => i.UnitPrice, unitPrice)
            .Generate();

    public static CreateOrderCommand Valid(params CreateOrderItemDto[] items)
    {
        var faker = new Faker();
        return new CreateOrderCommand(
            CustomerName: faker.Name.FullName(),
            CustomerEmail: faker.Internet.Email(),
            ShippingAddress: faker.Address.FullAddress(),
            Notes: null,
            Items: items.Length == 0
                ? new List<CreateOrderItemDto> { Item() }
                : items.ToList());
    }
}
