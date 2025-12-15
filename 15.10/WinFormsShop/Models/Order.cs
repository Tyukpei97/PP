using System;
using System.Collections.Generic;

namespace WinFormsShop.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string OrderDetails { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = "В обработке";
    public DateTime CreatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}
