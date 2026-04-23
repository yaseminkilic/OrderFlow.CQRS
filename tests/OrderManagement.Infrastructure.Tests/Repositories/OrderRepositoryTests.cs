using Microsoft.EntityFrameworkCore;
using OrderFlow.CQRS.Domain.Enums;
using OrderFlow.CQRS.Domain.Tests.TestData;
using OrderFlow.CQRS.Infrastructure.Repositories;
using OrderFlow.CQRS.Infrastructure.Tests.Fixtures;

namespace OrderFlow.CQRS.Infrastructure.Tests.Repositories;

[Collection(nameof(SqlServerCollection))]
public class OrderRepositoryTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;

    public OrderRepositoryTests(SqlServerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        await using var ctx = _fixture.CreateContext();
        ctx.Orders.RemoveRange(ctx.Orders);
        await ctx.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_PersistsOrderAndItems()
    {
        await using var ctx = _fixture.CreateContext();
        var repo = new OrderRepository(ctx);
        var order = OrderFaker.Order(itemCount: 3).Generate();

        await repo.AddAsync(order);

        await using var verify = _fixture.CreateContext();
        var stored = await verify.Orders.Include(o => o.Items).SingleAsync(o => o.Id == order.Id);
        stored.Items.Should().HaveCount(3);
        stored.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderExists_ReturnsOrderWithItems()
    {
        var order = OrderFaker.Order(itemCount: 2).Generate();
        await using (var seed = _fixture.CreateContext())
        {
            seed.Orders.Add(order);
            await seed.SaveChangesAsync();
        }

        await using var ctx = _fixture.CreateContext();
        var repo = new OrderRepository(ctx);

        var result = await repo.GetByIdAsync(order.Id);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderMissing_ReturnsNull()
    {
        await using var ctx = _fixture.CreateContext();
        var repo = new OrderRepository(ctx);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }
}
