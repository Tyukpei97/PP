using System;

namespace WinFormsShop.Models;

public class CartItem
{
    public Product Product { get; set; } = new();
    public int Quantity { get; set; } = 1;
    public decimal Subtotal => Math.Round(Product.Price * Quantity, 2);
}
