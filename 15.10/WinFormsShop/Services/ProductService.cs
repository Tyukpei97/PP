using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using WinFormsShop.Models;

namespace WinFormsShop.Services;

public class ProductService
{
    public async Task<List<Product>> GetProductsAsync(string? category = null, decimal? minPrice = null, decimal? maxPrice = null, string? search = null, bool includeInactive = false)
    {
        var products = new List<Product>();
        await using var connection = DatabaseService.CreateConnection();
        await connection.OpenAsync();

        var sql =
            @"SELECT id, name, description, price, category, image_path, is_active
              FROM products WHERE 1=1";

        var parameters = new List<(string Name, object? Value)>();
        if (!includeInactive)
        {
            sql += " AND is_active = 1";
        }
        if (!string.IsNullOrWhiteSpace(category))
        {
            sql += " AND category = @category";
            parameters.Add(("@category", category));
        }
        if (minPrice.HasValue)
        {
            sql += " AND price >= @minPrice";
            parameters.Add(("@minPrice", minPrice.Value));
        }
        if (maxPrice.HasValue)
        {
            sql += " AND price <= @maxPrice";
            parameters.Add(("@maxPrice", maxPrice.Value));
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " AND (LOWER(name) LIKE LOWER(@search) OR LOWER(description) LIKE LOWER(@search))";
            parameters.Add(("@search", $"%{search}%"));
        }

        sql += " ORDER BY name;";

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Price = (decimal)reader.GetDouble(3),
                Category = reader.GetString(4),
                ImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                IsActive = reader.GetInt32(6) == 1
            });
        }

        return products;
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        var categories = new List<string>();
        await using var connection = DatabaseService.CreateConnection();
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT DISTINCT category FROM products ORDER BY category;";
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            categories.Add(reader.GetString(0));
        }

        return categories;
    }

    public async Task<int> AddAsync(Product product)
    {
        await using var connection = DatabaseService.CreateConnection();
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"INSERT INTO products(name, description, price, category, image_path, is_active)
                            VALUES(@name, @description, @price, @category, @image, @active);
                            SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("@name", product.Name);
        cmd.Parameters.AddWithValue("@description", product.Description);
        cmd.Parameters.AddWithValue("@price", product.Price);
        cmd.Parameters.AddWithValue("@category", product.Category);
        cmd.Parameters.AddWithValue("@image", (object?)product.ImagePath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@active", product.IsActive ? 1 : 0);
        var id = (long)(await cmd.ExecuteScalarAsync() ?? 0);
        return (int)id;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        await using var connection = DatabaseService.CreateConnection();
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            @"SELECT id, name, description, price, category, image_path, is_active
              FROM products WHERE id=@id;";
        cmd.Parameters.AddWithValue("@id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Product
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Description = reader.GetString(2),
            Price = (decimal)reader.GetDouble(3),
            Category = reader.GetString(4),
            ImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
            IsActive = reader.GetInt32(6) == 1
        };
    }

    public async Task UpdateAsync(Product product)
    {
        await using var connection = DatabaseService.CreateConnection();
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"UPDATE products
                            SET name=@name, description=@description, price=@price,
                                category=@category, image_path=@image, is_active=@active
                            WHERE id=@id;";
        cmd.Parameters.AddWithValue("@name", product.Name);
        cmd.Parameters.AddWithValue("@description", product.Description);
        cmd.Parameters.AddWithValue("@price", product.Price);
        cmd.Parameters.AddWithValue("@category", product.Category);
        cmd.Parameters.AddWithValue("@image", (object?)product.ImagePath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@active", product.IsActive ? 1 : 0);
        cmd.Parameters.AddWithValue("@id", product.Id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SetActiveAsync(int productId, bool active)
    {
        await using var connection = DatabaseService.CreateConnection();
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE products SET is_active=@active WHERE id=@id;";
        cmd.Parameters.AddWithValue("@active", active ? 1 : 0);
        cmd.Parameters.AddWithValue("@id", productId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeactivateAsync(int productId)
    {
        await using var connection = DatabaseService.CreateConnection();
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE products SET is_active = 0 WHERE id=@id;";
        cmd.Parameters.AddWithValue("@id", productId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int productId)
    {
        await using var connection = DatabaseService.CreateConnection();
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM products WHERE id=@id;";
        cmd.Parameters.AddWithValue("@id", productId);
        await cmd.ExecuteNonQueryAsync();
    }

    public string? SaveImageCopy(string? sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
        {
            return null;
        }

        AppDataPaths.EnsureCreated();
        var extension = Path.GetExtension(sourcePath);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var destination = Path.Combine(AppDataPaths.ImagesDirectory, fileName);
        File.Copy(sourcePath, destination, overwrite: true);
        return destination;
    }
}
