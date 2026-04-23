using OrderFlow.CQRS.Domain.Entities;

namespace OrderFlow.CQRS.Domain.Tests.Entities;

public class OrderItemTests
{
    [Theory]
    [InlineData(1, 100.00, 100.00)]
    [InlineData(3, 49.99, 149.97)]
    [InlineData(10, 0.01, 0.10)]
    public void TotalPrice_WhenQuantityAndUnitPriceSet_ReturnsProduct(
        int quantity, decimal unitPrice, decimal expected)
    {
        var item = new OrderItem { Quantity = quantity, UnitPrice = unitPrice };

        item.TotalPrice.Should().Be(expected);
    }

    [Fact]
    public void TotalPrice_WhenQuantityIsZero_ReturnsZero()
    {
        var item = new OrderItem { Quantity = 0, UnitPrice = 100m };

        item.TotalPrice.Should().Be(0m);
    }
}
