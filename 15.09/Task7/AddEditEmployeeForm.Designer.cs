using System.Drawing;
using System.Windows.Forms;

namespace MiniEmployeeDatabase;

partial class AddEditEmployeeForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        lblName = new Label();
        txtName = new TextBox();
        lblPosition = new Label();
        txtPosition = new TextBox();
        lblSalary = new Label();
        numSalary = new NumericUpDown();
        lblError = new Label();
        flowLayoutPanel1 = new FlowLayoutPanel();
        btnSave = new Button();
        btnCancel = new Button();
        tableLayoutPanel1 = new TableLayoutPanel();
        ((System.ComponentModel.ISupportInitialize)numSalary).BeginInit();
        flowLayoutPanel1.SuspendLayout();
        tableLayoutPanel1.SuspendLayout();
        SuspendLayout();
        // 
        // lblName
        // 
        lblName.Anchor = AnchorStyles.Left;
        lblName.AutoSize = true;
        lblName.Location = new Point(3, 12);
        lblName.Name = "lblName";
        lblName.Size = new Size(45, 15);
        lblName.TabIndex = 0;
        lblName.Text = "Name*";
        // 
        // txtName
        // 
        txtName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtName.Location = new Point(90, 8);
        txtName.Name = "txtName";
        txtName.Size = new Size(281, 23);
        txtName.TabIndex = 1;
        // 
        // lblPosition
        // 
        lblPosition.Anchor = AnchorStyles.Left;
        lblPosition.AutoSize = true;
        lblPosition.Location = new Point(3, 51);
        lblPosition.Name = "lblPosition";
        lblPosition.Size = new Size(58, 15);
        lblPosition.TabIndex = 2;
        lblPosition.Text = "Position*";
        // 
        // txtPosition
        // 
        txtPosition.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtPosition.Location = new Point(90, 47);
        txtPosition.Name = "txtPosition";
        txtPosition.Size = new Size(281, 23);
        txtPosition.TabIndex = 3;
        // 
        // lblSalary
        // 
        lblSalary.Anchor = AnchorStyles.Left;
        lblSalary.AutoSize = true;
        lblSalary.Location = new Point(3, 90);
        lblSalary.Name = "lblSalary";
        lblSalary.Size = new Size(44, 15);
        lblSalary.TabIndex = 4;
        lblSalary.Text = "Salary*";
        // 
        // numSalary
        // 
        numSalary.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        numSalary.DecimalPlaces = 2;
        numSalary.Location = new Point(90, 86);
        numSalary.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
        numSalary.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
        numSalary.Name = "numSalary";
        numSalary.Size = new Size(281, 23);
        numSalary.TabIndex = 5;
        numSalary.ThousandsSeparator = true;
        // 
        // lblError
        // 
        lblError.Anchor = AnchorStyles.Left;
        lblError.AutoSize = true;
        lblError.ForeColor = Color.Firebrick;
        lblError.Location = new Point(3, 128);
        lblError.Name = "lblError";
        lblError.Size = new Size(0, 15);
        lblError.TabIndex = 6;
        lblError.Visible = false;
        // 
        // flowLayoutPanel1
        // 
        flowLayoutPanel1.Anchor = AnchorStyles.Right;
        flowLayoutPanel1.AutoSize = true;
        flowLayoutPanel1.Controls.Add(btnSave);
        flowLayoutPanel1.Controls.Add(btnCancel);
        flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
        flowLayoutPanel1.Location = new Point(156, 158);
        flowLayoutPanel1.Name = "flowLayoutPanel1";
        flowLayoutPanel1.Size = new Size(215, 35);
        flowLayoutPanel1.TabIndex = 7;
        // 
        // btnSave
        // 
        btnSave.AutoSize = true;
        btnSave.Location = new Point(137, 3);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(75, 27);
        btnSave.TabIndex = 0;
        btnSave.Text = "Save";
        btnSave.UseVisualStyleBackColor = true;
        btnSave.Click += btnSave_Click;
        // 
        // btnCancel
        // 
        btnCancel.AutoSize = true;
        btnCancel.Location = new Point(56, 3);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(75, 27);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Cancel";
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.Click += btnCancel_Click;
        // 
        // tableLayoutPanel1
        // 
        tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        tableLayoutPanel1.ColumnCount = 2;
        tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 87F));
        tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayoutPanel1.Controls.Add(lblName, 0, 0);
        tableLayoutPanel1.Controls.Add(txtName, 1, 0);
        tableLayoutPanel1.Controls.Add(lblPosition, 0, 1);
        tableLayoutPanel1.Controls.Add(txtPosition, 1, 1);
        tableLayoutPanel1.Controls.Add(lblSalary, 0, 2);
        tableLayoutPanel1.Controls.Add(numSalary, 1, 2);
        tableLayoutPanel1.Controls.Add(lblError, 0, 3);
        tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 1, 4);
        tableLayoutPanel1.Location = new Point(12, 12);
        tableLayoutPanel1.Name = "tableLayoutPanel1";
        tableLayoutPanel1.RowCount = 5;
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutPanel1.Size = new Size(374, 196);
        tableLayoutPanel1.TabIndex = 8;
        // 
        // AddEditEmployeeForm
        // 
        AcceptButton = btnSave;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnCancel;
        ClientSize = new Size(398, 220);
        Controls.Add(tableLayoutPanel1);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "AddEditEmployeeForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Add/Edit Employee";
        ((System.ComponentModel.ISupportInitialize)numSalary).EndInit();
        flowLayoutPanel1.ResumeLayout(false);
        flowLayoutPanel1.PerformLayout();
        tableLayoutPanel1.ResumeLayout(false);
        tableLayoutPanel1.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private Label lblName;
    private TextBox txtName;
    private Label lblPosition;
    private TextBox txtPosition;
    private Label lblSalary;
    private NumericUpDown numSalary;
    private Label lblError;
    private FlowLayoutPanel flowLayoutPanel1;
    private Button btnSave;
    private Button btnCancel;
    private TableLayoutPanel tableLayoutPanel1;
}
