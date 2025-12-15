using System;
using System.Drawing;
using System.Windows.Forms;
using WinFormsShop.Models;
using WinFormsShop.Services;

namespace WinFormsShop.Forms;

public class ProductEditForm : Form
{
    private readonly ProductService _productService;
    public Product Product { get; private set; }

    private TextBox _nameBox = null!;
    private TextBox _descriptionBox = null!;
    private NumericUpDown _priceBox = null!;
    private TextBox _categoryBox = null!;
    private TextBox _imageBox = null!;
    private CheckBox _activeBox = null!;

    public ProductEditForm(ProductService productService, Product product)
    {
        _productService = productService;
        Product = product;
        InitializeComponent();
        FillFields(product);
    }

    private void InitializeComponent()
    {
        Text = "Товар";
        Width = 520;
        Height = 520;
        StartPosition = FormStartPosition.CenterParent;

        var nameLabel = new Label { Text = "Название", Location = new Point(20, 20), AutoSize = true };
        _nameBox = new TextBox { Location = new Point(150, 15), Width = 320 };

        var descLabel = new Label { Text = "Описание", Location = new Point(20, 60), AutoSize = true };
        _descriptionBox = new TextBox { Location = new Point(150, 55), Width = 320, Height = 120, Multiline = true };

        var priceLabel = new Label { Text = "Цена", Location = new Point(20, 190), AutoSize = true };
        _priceBox = new NumericUpDown { Location = new Point(150, 185), Width = 120, Maximum = 1_000_000, DecimalPlaces = 2, Increment = 100 };

        var categoryLabel = new Label { Text = "Категория", Location = new Point(20, 230), AutoSize = true };
        _categoryBox = new TextBox { Location = new Point(150, 225), Width = 320 };

        var imageLabel = new Label { Text = "Изображение", Location = new Point(20, 270), AutoSize = true };
        _imageBox = new TextBox { Location = new Point(150, 265), Width = 240 };
        var chooseButton = new Button { Text = "Выбрать", Location = new Point(400, 263), Width = 70 };
        chooseButton.Click += (_, _) => ChooseImage();

        _activeBox = new CheckBox { Text = "Активен", Location = new Point(150, 300), AutoSize = true };

        var saveButton = new Button { Text = "Сохранить", Location = new Point(150, 340), Width = 120 };
        saveButton.Click += (_, _) => SaveProduct();

        var cancelButton = new Button { Text = "Отмена", Location = new Point(280, 340), Width = 120 };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;

        Controls.AddRange(new Control[]
        {
            nameLabel, _nameBox, descLabel, _descriptionBox, priceLabel, _priceBox,
            categoryLabel, _categoryBox, imageLabel, _imageBox, chooseButton, _activeBox,
            saveButton, cancelButton
        });
    }

    private void FillFields(Product product)
    {
        _nameBox.Text = product.Name;
        _descriptionBox.Text = product.Description;
        _priceBox.Value = product.Price > _priceBox.Maximum ? _priceBox.Maximum : product.Price;
        _categoryBox.Text = product.Category;
        _imageBox.Text = product.ImagePath ?? string.Empty;
        _activeBox.Checked = product.IsActive;
    }

    private void ChooseImage()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp|Все файлы|*.*"
        };
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _imageBox.Text = dialog.FileName;
        }
    }

    private void SaveProduct()
    {
        if (string.IsNullOrWhiteSpace(_nameBox.Text))
        {
            MessageBox.Show("Введите название товара.", "Валидация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string? storedImage = null;
        if (!string.IsNullOrWhiteSpace(_imageBox.Text))
        {
            storedImage = _productService.SaveImageCopy(_imageBox.Text);
        }
        Product = new Product
        {
            Id = Product.Id,
            Name = _nameBox.Text.Trim(),
            Description = _descriptionBox.Text.Trim(),
            Price = _priceBox.Value,
            Category = _categoryBox.Text.Trim(),
            ImagePath = storedImage ?? Product.ImagePath,
            IsActive = _activeBox.Checked
        };

        DialogResult = DialogResult.OK;
    }
}
