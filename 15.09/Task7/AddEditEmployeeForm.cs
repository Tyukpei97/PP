using System;
using System.Windows.Forms;

namespace MiniEmployeeDatabase;

public partial class AddEditEmployeeForm : Form
{
    public Employee? Employee { get; private set; }

    public AddEditEmployeeForm(Employee? employee = null)
    {
        InitializeComponent();
        if (employee != null)
        {
            Text = "Edit Employee";
            txtName.Text = employee.Name;
            txtPosition.Text = employee.Position;
            numSalary.Value = Math.Max(numSalary.Minimum, Math.Min(numSalary.Maximum, employee.Salary));
            Employee = employee.Clone();
        }
        else
        {
            Text = "Add Employee";
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        lblError.Text = string.Empty;
        lblError.Visible = false;
        var name = txtName.Text.Trim();
        var position = txtPosition.Text.Trim();
        var salary = numSalary.Value;

        if (string.IsNullOrWhiteSpace(name))
        {
            ShowValidation("Name is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(position))
        {
            ShowValidation("Position is required.");
            return;
        }

        Employee ??= new Employee();
        Employee.Name = name;
        Employee.Position = position;
        Employee.Salary = salary;

        DialogResult = DialogResult.OK;
        Close();
    }

    private void ShowValidation(string message)
    {
        lblError.Text = message;
        lblError.Visible = true;
    }
}
