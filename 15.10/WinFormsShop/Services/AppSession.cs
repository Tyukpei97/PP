using System;
using System.Collections.Generic;
using System.Linq;
using WinFormsShop.Models;

namespace WinFormsShop.Services;

public static class AppSession
{
    public static User? CurrentUser { get; private set; }
    public static Cart? CurrentCart { get; private set; }

    public static void SetUser(User user)
    {
        CurrentUser = user;
        CurrentCart = new Cart();
    }

    public static void Logout()
    {
        CurrentUser = null;
        CurrentCart = null;
    }
}

public class Cart
{
    private readonly List<CartItem> _items = new();

    public IReadOnlyList<CartItem> Items => _items;

    public void AddItem(Product product, int quantity)
    {
        if (quantity <= 0)
        {
            return;
        }

        var existing = _items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existing == null)
        {
            _items.Add(new CartItem { Product = product, Quantity = quantity });
        }
        else
        {
            existing.Quantity += quantity;
        }
    }

    public void UpdateQuantity(int productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.Product.Id == productId);
        if (item == null)
        {
            return;
        }

        if (quantity <= 0)
        {
            _items.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }
    }

    public void Remove(int productId)
    {
        var item = _items.FirstOrDefault(i => i.Product.Id == productId);
        if (item != null)
        {
            _items.Remove(item);
        }
    }

    public decimal Total() => Math.Round(_items.Sum(i => i.Subtotal), 2);

    public void Clear() => _items.Clear();
}
