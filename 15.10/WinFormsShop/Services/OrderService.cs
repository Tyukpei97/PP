using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using WinFormsShop.Models;

namespace WinFormsShop.Services;

public class OrderService
{
    public async Task<Order> CreateOrderAsync(User user, Cart cart, string address, string paymentMethod)
    {
        if (cart.Items.Count == 0)
        {
            throw new InvalidOperationException("Корзина пустая.");
        }

        await using var connection = DatabaseService.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync();

        var orderDetails = BuildOrderDetails(cart, address, paymentMethod);
        var total = cart.Total();
        var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

        await using var orderCmd = connection.CreateCommand();
        orderCmd.Transaction = transaction;
        orderCmd.CommandText = @"INSERT INTO orders(user_id, order_details, total_price, delivery_address,
                                                     payment_method, status, created_at)
                                 VALUES(@user_id, @details, @total, @address, @payment, @status, @created);
                                 SELECT last_insert_rowid();";
        orderCmd.Parameters.AddWithValue("@user_id", user.Id);
        orderCmd.Parameters.AddWithValue("@details", orderDetails);
        orderCmd.Parameters.AddWithValue("@total", total);
        orderCmd.Parameters.AddWithValue("@address", address);
        orderCmd.Parameters.AddWithValue("@payment", paymentMethod);
        orderCmd.Parameters.AddWithValue("@status", "В обработке");
        orderCmd.Parameters.AddWithValue("@created", now);
        var orderId = (long)(await orderCmd.ExecuteScalarAsync() ?? 0);

        foreach (var item in cart.Items)
        {
            await using var itemCmd = connection.CreateCommand();
            itemCmd.Transaction = transaction;
            itemCmd.CommandText = @"INSERT INTO order_items(order_id, product_id, product_name_snapshot,
                                                          unit_price_snapshot, quantity)
                                    VALUES(@order_id, @product_id, @name, @price, @qty);";
            itemCmd.Parameters.AddWithValue("@order_id", orderId);
            itemCmd.Parameters.AddWithValue("@product_id", item.Product.Id);
            itemCmd.Parameters.AddWithValue("@name", item.Product.Name);
            itemCmd.Parameters.AddWithValue("@price", item.Product.Price);
            itemCmd.Parameters.AddWithValue("@qty", item.Quantity);
            await itemCmd.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();

        return new Order
        {
            Id = (int)orderId,
            UserId = user.Id,
            OrderDetails = orderDetails,
            TotalPrice = total,
            DeliveryAddress = address,
            PaymentMethod = paymentMethod,
            Status = "В обработке",
            CreatedAt = DateTime.Parse(now, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            Items = cart.Items.Select(i => new OrderItem
            {
                OrderId = (int)orderId,
                ProductId = i.Product.Id,
                ProductNameSnapshot = i.Product.Name,
                UnitPriceSnapshot = i.Product.Price,
                Quantity = i.Quantity
            }).ToList()
        };
    }

    public async Task<List<(Order Order, string UserLogin, string UserEmail)>> GetOrdersWithUsersAsync()
    {
        var list = new List<(Order, string, string)>();
        await using var connection = DatabaseService.CreateConnection();
        await connection.OpenAsync();
        const string sql = @"SELECT o.id, o.user_id, o.order_details, o.total_price, o.delivery_address,
                                    o.payment_method, o.status, o.created_at,
                                    u.login, u.email
                             FROM orders o
                             JOIN users u ON u.id = o.user_id
                             ORDER BY o.created_at DESC;";
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var order = new Order
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                OrderDetails = reader.GetString(2),
                TotalPrice = (decimal)reader.GetDouble(3),
                DeliveryAddress = reader.GetString(4),
                PaymentMethod = reader.GetString(5),
                Status = reader.GetString(6),
                CreatedAt = DateTime.Parse(reader.GetString(7), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            };
            list.Add((order, reader.GetString(8), reader.GetString(9)));
        }

        return list;
    }

    public async Task UpdateStatusAsync(int orderId, string status)
    {
        await using var connection = DatabaseService.CreateConnection();
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE orders SET status=@status WHERE id=@id;";
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@id", orderId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static string BuildOrderDetails(Cart cart, string address, string paymentMethod)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Детали заказа:");
        foreach (var item in cart.Items)
        {
            sb.AppendLine($"{item.Product.Name} x{item.Quantity} — {item.Subtotal:0.00} ₽");
        }

        sb.AppendLine($"Итого: {cart.Total():0.00} ₽");
        sb.AppendLine($"Адрес: {address}");
        sb.AppendLine($"Оплата: {paymentMethod}");
        return sb.ToString();
    }
}
