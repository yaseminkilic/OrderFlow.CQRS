using System.Net.Http.Json;
using OrderFlow.CQRS.Application.Common.Models;
using OrderFlow.CQRS.Application.DTOs;
using OrderFlow.CQRS.Application.Features.Orders.Commands;
using OrderFlow.CQRS.Domain.Enums;

namespace OrderFlow.CQRS.Blazor.Services;

public class OrderService
{
    private readonly HttpClient _httpClient;

    public OrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<OrderDto>> GetOrdersAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<Result<List<OrderDto>>>("api/orders");
        return result?.Data ?? new List<OrderDto>();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
    {
        var result = await _httpClient.GetFromJsonAsync<Result<OrderDto>>($"api/orders/{id}");
        return result?.Data;
    }

    public async Task<OrderDto?> CreateOrderAsync(CreateOrderCommand command)
    {
        var response = await _httpClient.PostAsJsonAsync("api/orders", command);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Result<OrderDto>>();
        return result?.Data;
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(Guid id, OrderStatus status)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/orders/{id}/status", new { Status = status });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Result<OrderDto>>();
        return result?.Data;
    }

    public async Task DeleteOrderAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/orders/{id}");
        response.EnsureSuccessStatusCode();
    }
}
