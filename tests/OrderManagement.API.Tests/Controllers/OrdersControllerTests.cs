using System.Net;
using System.Net.Http.Json;
using OrderFlow.CQRS.API.Tests.Fixtures;
using OrderFlow.CQRS.Application.Common.Models;
using OrderFlow.CQRS.Application.DTOs;
using OrderFlow.CQRS.Application.Features.Orders.Commands;
using OrderFlow.CQRS.Application.Tests.TestData;

namespace OrderFlow.CQRS.API.Tests.Controllers;

public class OrdersControllerTests : IClassFixture<OrderManagementApiFactory>
{
    private readonly OrderManagementApiFactory _factory;
    private readonly HttpClient _client;

    public OrdersControllerTests(OrderManagementApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_WithValidCommand_Returns201AndPersistsOrder()
    {
        var command = CreateOrderCommandFaker.Valid(
            CreateOrderCommandFaker.Item(quantity: 2, unitPrice: 50m));

        var response = await _client.PostAsJsonAsync("/api/orders", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<Result<OrderDto>>();
        body!.IsSuccess.Should().BeTrue();
        body.Data!.TotalAmount.Should().Be(100m);
        _factory.Publisher.Published.Should().ContainSingle(p => p.Queue == "order-events");
    }

    [Fact]
    public async Task Get_AfterCreatingOrder_ReturnsSameOrder()
    {
        var command = CreateOrderCommandFaker.Valid();

        var createResponse = await _client.PostAsJsonAsync("/api/orders", command);
        var created = (await createResponse.Content.ReadFromJsonAsync<Result<OrderDto>>())!.Data!;

        var getResponse = await _client.GetAsync($"/api/orders/{created.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await getResponse.Content.ReadFromJsonAsync<Result<OrderDto>>();
        body!.Data!.OrderNumber.Should().Be(created.OrderNumber);
    }
}
