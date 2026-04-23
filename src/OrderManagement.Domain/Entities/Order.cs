using OrderFlow.CQRS.Domain.Common;
using OrderFlow.CQRS.Domain.Enums;

namespace OrderFlow.CQRS.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public List<OrderItem> Items { get; private set; } = new();

    /// <summary>
    /// Total amount of the order. 
    /// </summary>
    public decimal TotalAmount { get; set; }

    public Order()
    {
        Status = OrderStatus.Pending;
        PaymentStatus = PaymentStatus.Pending;
    }

    public void CalculateTotalAmount() => 
        TotalAmount = Items.Sum(i => i.TotalPrice);

    // Logic for updating status could be added here to enforce domain rules
}