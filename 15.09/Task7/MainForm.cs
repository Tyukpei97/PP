using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MiniEmployeeDatabase;

public partial class MainForm : Form
{
    private readonly EmployeeRepository _repository;
    private readonly BindingSource _bindingSource = new();
    private List<Employee> _allEmployees = new();
    private string _sortProperty = "Name";
    private ListSortDirection _sortDirection = ListSortDirection.Ascending;

    public MainForm()
    {
        InitializeComponent();
        _repository = new EmployeeRepository(Path.Combine(AppContext.BaseDirectory, "employees.csv"));
        dgvEmployees.AutoGenerateColumns = false;
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        try
        {
            _allEmployees = _repository.LoadEmployees();
        }
        catch (Exception ex)
        {
            ShowError("Failed to load employees. Starting with an empty list.", ex);
            _allEmployees = new List<Employee>();
        }

        RefreshPositionFilter();
        ApplyFilters();
    }

    private void btnAdd_Click(object sender, EventArgs e)
    {
        using var dialog = new AddEditEmployeeForm();
        if (dialog.ShowDialog(this) == DialogResult.OK && dialog.Employee != null)
        {
            _allEmployees.Add(dialog.Employee);
            PersistChanges();
            RefreshPositionFilter();
            ApplyFilters();
            SelectEmployee(dialog.Employee.Id);
        }
    }

    private void btnEdit_Click(object sender, EventArgs e)
    {
        var selected = GetSelectedEmployee();
        if (selected == null)
        {
            MessageBox.Show(this, "Please select an employee to edit.", "Edit Employee",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new AddEditEmployeeForm(selected);
        if (dialog.ShowDialog(this) == DialogResult.OK && dialog.Employee != null)
        {
            var existing = _allEmployees.FirstOrDefault(emp => emp.Id == selected.Id);
            if (existing != null)
            {
                existing.Name = dialog.Employee.Name;
                existing.Position = dialog.Employee.Position;
                existing.Salary = dialog.Employee.Salary;
                PersistChanges();
                RefreshPositionFilter();
                ApplyFilters();
                SelectEmployee(existing.Id);
            }
        }
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        var selected = GetSelectedEmployee();
        if (selected == null)
        {
            MessageBox.Show(this, "Please select an employee to delete.", "Delete Employee",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(this,
            $"Delete employee \"{selected.Name}\"?",
            "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (confirm == DialogResult.Yes)
        {
            _allEmployees.RemoveAll(emp => emp.Id == selected.Id);
            PersistChanges();
            RefreshPositionFilter();
            ApplyFilters();
        }
    }

    private async void btnExport_Click(object sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Text Files|*.txt|All Files|*.*",
            FileName = "employees.txt",
            Title = "Export Employees"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var exportLines = BuildExportLines();
        try
        {
            await File.WriteAllLinesAsync(dialog.FileName, exportLines);
            MessageBox.Show(this, "Employees exported successfully.", "Export",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowError("Failed to export employees.", ex);
        }
    }

    private void btnSearch_Click(object sender, EventArgs e) => ApplyFilters();

    private void txtSearch_TextChanged(object sender, EventArgs e) => ApplyFilters();

    private void FiltersChanged(object sender, EventArgs e) => ApplyFilters();

    private void btnClearFilters_Click(object sender, EventArgs e)
    {
        txtSearch.Clear();
        cmbPositionFilter.SelectedIndex = 0;
        numSalaryMin.Value = 0;
        numSalaryMax.Value = numSalaryMax.Maximum;
        ApplyFilters();
    }

    private void dgvEmployees_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        var column = dgvEmployees.Columns[e.ColumnIndex];
        var property = column.DataPropertyName;
        if (string.IsNullOrWhiteSpace(property))
        {
            return;
        }

        if (_sortProperty == property)
        {
            _sortDirection = _sortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
        }
        else
        {
            _sortProperty = property;
            _sortDirection = ListSortDirection.Ascending;
        }

        ApplyFilters();
        UpdateSortGlyphs(column);
    }

    private void ApplyFilters()
    {
        if (numSalaryMin.Value > numSalaryMax.Value)
        {
            MessageBox.Show(this, "Minimum salary cannot exceed maximum salary.", "Filter",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var searchText = txtSearch.Text.Trim();
        var position = cmbPositionFilter.SelectedItem as string;
        var minSalary = numSalaryMin.Value;
        var maxSalary = numSalaryMax.Value;

        var query = _allEmployees.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(emp =>
                emp.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                emp.Position.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(position) && !string.Equals(position, "All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(emp =>
                string.Equals(emp.Position, position, StringComparison.OrdinalIgnoreCase));
        }

        query = query.Where(emp => emp.Salary >= minSalary && emp.Salary <= maxSalary);

        var filtered = SortEmployees(query, _sortProperty, _sortDirection).ToList();
        var currentId = GetSelectedEmployee()?.Id;

        _bindingSource.DataSource = new BindingList<Employee>(filtered);
        dgvEmployees.DataSource = _bindingSource;

        if (currentId.HasValue)
        {
            SelectEmployee(currentId.Value);
        }

        var sortColumn = dgvEmployees.Columns
            .Cast<DataGridViewColumn>()
            .FirstOrDefault(c => string.Equals(c.DataPropertyName, _sortProperty, StringComparison.OrdinalIgnoreCase));
        if (sortColumn != null)
        {
            UpdateSortGlyphs(sortColumn);
        }
    }

    private IEnumerable<Employee> SortEmployees(IEnumerable<Employee> source, string property, ListSortDirection direction)
    {
        var asc = direction == ListSortDirection.Ascending;
        return property switch
        {
            "Position" => asc
                ? source.OrderBy(emp => emp.Position, StringComparer.CurrentCultureIgnoreCase)
                : source.OrderByDescending(emp => emp.Position, StringComparer.CurrentCultureIgnoreCase),
            "Salary" => asc
                ? source.OrderBy(emp => emp.Salary)
                : source.OrderByDescending(emp => emp.Salary),
            _ => asc
                ? source.OrderBy(emp => emp.Name, StringComparer.CurrentCultureIgnoreCase)
                : source.OrderByDescending(emp => emp.Name, StringComparer.CurrentCultureIgnoreCase)
        };
    }

    private Employee? GetSelectedEmployee()
    {
        if (dgvEmployees.CurrentRow?.DataBoundItem is Employee employee)
        {
            return employee;
        }

        return null;
    }

    private void SelectEmployee(Guid id)
    {
        foreach (DataGridViewRow row in dgvEmployees.Rows)
        {
            if (row.DataBoundItem is Employee emp && emp.Id == id)
            {
                row.Selected = true;
                dgvEmployees.CurrentCell = row.Cells[0];
                dgvEmployees.FirstDisplayedScrollingRowIndex = row.Index;
                break;
            }
        }
    }

    private void RefreshPositionFilter()
    {
        var positions = _allEmployees
            .Select(emp => emp.Position)
            .Where(pos => !string.IsNullOrWhiteSpace(pos))
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(pos => pos, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var previousSelection = cmbPositionFilter.SelectedItem as string;
        positions.Insert(0, "All");
        cmbPositionFilter.DataSource = positions;

        if (!string.IsNullOrWhiteSpace(previousSelection))
        {
            var index = positions.FindIndex(p => string.Equals(p, previousSelection, StringComparison.OrdinalIgnoreCase));
            cmbPositionFilter.SelectedIndex = index >= 0 ? index : 0;
        }
        else
        {
            cmbPositionFilter.SelectedIndex = 0;
        }
    }

    private void PersistChanges()
    {
        try
        {
            _repository.SaveEmployees(_allEmployees);
        }
        catch (Exception ex)
        {
            ShowError("Failed to save employees.", ex);
        }
    }

    private List<string> BuildExportLines()
    {
        var lines = new List<string>
        {
            "Employee List",
            $"Generated: {DateTime.Now}",
            string.Empty,
            "Name | Position | Salary"
        };

        foreach (DataGridViewRow row in dgvEmployees.Rows)
        {
            if (row.DataBoundItem is Employee emp)
            {
                lines.Add($"{emp.Name} | {emp.Position} | {emp.Salary.ToString("C2", CultureInfo.CurrentCulture)}");
            }
        }

        return lines;
    }

    private void UpdateSortGlyphs(DataGridViewColumn sortedColumn)
    {
        foreach (DataGridViewColumn column in dgvEmployees.Columns)
        {
            column.HeaderCell.SortGlyphDirection = SortOrder.None;
        }

        sortedColumn.HeaderCell.SortGlyphDirection =
            _sortDirection == ListSortDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending;
    }

    private void ShowError(string message, Exception ex)
    {
        MessageBox.Show(this, $"{message}{Environment.NewLine}{ex.Message}", "Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
