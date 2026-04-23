using OrderFlow.CQRS.Domain.Enums;
using OrderFlow.CQRS.Domain.Tests.TestData;

namespace OrderFlow.CQRS.Domain.Tests.Entities;

public class OrderTests
{
    [Fact]
    public void NewOrder_ByDefault_StartsInPendingState()
    {
        var order = OrderFaker.Order().Generate();

        order.Status.Should().Be(OrderStatus.Pending);
        order.PaymentStatus.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void NewOrder_ByDefault_HasGeneratedIdAndTimestamp()
    {
        var before = DateTime.UtcNow;

        var order = OrderFaker.Order().Generate();

        order.Id.Should().NotBe(Guid.Empty);
        order.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Order_WhenItemsAdded_AggregatesExpectedTotalViaItems()
    {
        var order = OrderFaker.Order(itemCount: 0).Generate();
        order.Items.Add(OrderFaker.Item(quantity: 2, unitPrice: 50m).Generate());
        order.Items.Add(OrderFaker.Item(quantity: 1, unitPrice: 25m).Generate());

        // Order entity's TotalAmount property
        order.TotalAmount.Should().Be(125m);
    }
}
