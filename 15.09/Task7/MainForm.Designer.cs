using System.Drawing;
using System.Windows.Forms;

namespace MiniEmployeeDatabase;

partial class MainForm
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
        tableLayoutPanel1 = new TableLayoutPanel();
        topLayout = new TableLayoutPanel();
        filtersFlow = new FlowLayoutPanel();
        lblSearch = new Label();
        txtSearch = new TextBox();
        btnSearch = new Button();
        lblPosition = new Label();
        cmbPositionFilter = new ComboBox();
        lblSalary = new Label();
        numSalaryMin = new NumericUpDown();
        lblSalarySeparator = new Label();
        numSalaryMax = new NumericUpDown();
        btnClearFilters = new Button();
        actionsFlow = new FlowLayoutPanel();
        btnAdd = new Button();
        btnEdit = new Button();
        btnDelete = new Button();
        btnExport = new Button();
        dgvEmployees = new DataGridView();
        colName = new DataGridViewTextBoxColumn();
        colPosition = new DataGridViewTextBoxColumn();
        colSalary = new DataGridViewTextBoxColumn();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
        tableLayoutPanel1.SuspendLayout();
        topLayout.SuspendLayout();
        filtersFlow.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numSalaryMin).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numSalaryMax).BeginInit();
        actionsFlow.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvEmployees).BeginInit();
        SuspendLayout();
        // 
        // tableLayoutPanel1
        // 
        tableLayoutPanel1.ColumnCount = 1;
        tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayoutPanel1.Controls.Add(topLayout, 0, 0);
        tableLayoutPanel1.Controls.Add(dgvEmployees, 0, 1);
        tableLayoutPanel1.Dock = DockStyle.Fill;
        tableLayoutPanel1.Location = new Point(0, 0);
        tableLayoutPanel1.Name = "tableLayoutPanel1";
        tableLayoutPanel1.RowCount = 2;
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutPanel1.Size = new Size(984, 561);
        tableLayoutPanel1.TabIndex = 0;
        // 
        // topLayout
        // 
        topLayout.ColumnCount = 2;
        topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320F));
        topLayout.Controls.Add(filtersFlow, 0, 0);
        topLayout.Controls.Add(actionsFlow, 1, 0);
        topLayout.Dock = DockStyle.Fill;
        topLayout.Location = new Point(3, 3);
        topLayout.Name = "topLayout";
        topLayout.RowCount = 1;
        topLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        topLayout.Size = new Size(978, 74);
        topLayout.TabIndex = 1;
        // 
        // filtersFlow
        // 
        filtersFlow.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        filtersFlow.AutoSize = true;
        filtersFlow.Controls.Add(lblSearch);
        filtersFlow.Controls.Add(txtSearch);
        filtersFlow.Controls.Add(btnSearch);
        filtersFlow.Controls.Add(lblPosition);
        filtersFlow.Controls.Add(cmbPositionFilter);
        filtersFlow.Controls.Add(lblSalary);
        filtersFlow.Controls.Add(numSalaryMin);
        filtersFlow.Controls.Add(lblSalarySeparator);
        filtersFlow.Controls.Add(numSalaryMax);
        filtersFlow.Controls.Add(btnClearFilters);
        filtersFlow.Location = new Point(3, 10);
        filtersFlow.Name = "filtersFlow";
        filtersFlow.Size = new Size(652, 53);
        filtersFlow.TabIndex = 0;
        filtersFlow.WrapContents = false;
        // 
        // lblSearch
        // 
        lblSearch.Anchor = AnchorStyles.Left;
        lblSearch.AutoSize = true;
        lblSearch.Location = new Point(3, 6);
        lblSearch.Name = "lblSearch";
        lblSearch.Size = new Size(45, 15);
        lblSearch.TabIndex = 0;
        lblSearch.Text = "Search:";
        // 
        // txtSearch
        // 
        txtSearch.Location = new Point(54, 3);
        txtSearch.Name = "txtSearch";
        txtSearch.PlaceholderText = "Name or position";
        txtSearch.Size = new Size(160, 23);
        txtSearch.TabIndex = 1;
        txtSearch.TextChanged += txtSearch_TextChanged;
        // 
        // btnSearch
        // 
        btnSearch.AutoSize = true;
        btnSearch.Location = new Point(220, 3);
        btnSearch.Name = "btnSearch";
        btnSearch.Size = new Size(54, 25);
        btnSearch.TabIndex = 2;
        btnSearch.Text = "Search";
        btnSearch.UseVisualStyleBackColor = true;
        btnSearch.Click += btnSearch_Click;
        // 
        // lblPosition
        // 
        lblPosition.Anchor = AnchorStyles.Left;
        lblPosition.AutoSize = true;
        lblPosition.Location = new Point(280, 6);
        lblPosition.Name = "lblPosition";
        lblPosition.Size = new Size(55, 15);
        lblPosition.TabIndex = 3;
        lblPosition.Text = "Position:";
        // 
        // cmbPositionFilter
        // 
        cmbPositionFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPositionFilter.FormattingEnabled = true;
        cmbPositionFilter.Location = new Point(341, 3);
        cmbPositionFilter.Name = "cmbPositionFilter";
        cmbPositionFilter.Size = new Size(130, 23);
        cmbPositionFilter.TabIndex = 4;
        cmbPositionFilter.SelectedIndexChanged += FiltersChanged;
        // 
        // lblSalary
        // 
        lblSalary.Anchor = AnchorStyles.Left;
        lblSalary.AutoSize = true;
        lblSalary.Location = new Point(477, 6);
        lblSalary.Name = "lblSalary";
        lblSalary.Size = new Size(42, 15);
        lblSalary.TabIndex = 5;
        lblSalary.Text = "Salary:";
        // 
        // numSalaryMin
        // 
        numSalaryMin.DecimalPlaces = 2;
        numSalaryMin.Location = new Point(525, 3);
        numSalaryMin.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
        numSalaryMin.Name = "numSalaryMin";
        numSalaryMin.Size = new Size(80, 23);
        numSalaryMin.TabIndex = 6;
        numSalaryMin.ThousandsSeparator = true;
        numSalaryMin.ValueChanged += FiltersChanged;
        // 
        // lblSalarySeparator
        // 
        lblSalarySeparator.Anchor = AnchorStyles.Left;
        lblSalarySeparator.AutoSize = true;
        lblSalarySeparator.Location = new Point(611, 6);
        lblSalarySeparator.Name = "lblSalarySeparator";
        lblSalarySeparator.Size = new Size(23, 15);
        lblSalarySeparator.TabIndex = 7;
        lblSalarySeparator.Text = "to:";
        // 
        // numSalaryMax
        // 
        numSalaryMax.DecimalPlaces = 2;
        numSalaryMax.Location = new Point(640, 3);
        numSalaryMax.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
        numSalaryMax.Name = "numSalaryMax";
        numSalaryMax.Size = new Size(80, 23);
        numSalaryMax.TabIndex = 8;
        numSalaryMax.ThousandsSeparator = true;
        numSalaryMax.Value = new decimal(new int[] { 1000000000, 0, 0, 0 });
        numSalaryMax.ValueChanged += FiltersChanged;
        // 
        // btnClearFilters
        // 
        btnClearFilters.AutoSize = true;
        btnClearFilters.Location = new Point(3, 32);
        btnClearFilters.Name = "btnClearFilters";
        btnClearFilters.Size = new Size(80, 25);
        btnClearFilters.TabIndex = 9;
        btnClearFilters.Text = "Clear filters";
        btnClearFilters.UseVisualStyleBackColor = true;
        btnClearFilters.Click += btnClearFilters_Click;
        // 
        // actionsFlow
        // 
        actionsFlow.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        actionsFlow.AutoSize = true;
        actionsFlow.Controls.Add(btnAdd);
        actionsFlow.Controls.Add(btnEdit);
        actionsFlow.Controls.Add(btnDelete);
        actionsFlow.Controls.Add(btnExport);
        actionsFlow.FlowDirection = FlowDirection.RightToLeft;
        actionsFlow.Location = new Point(668, 3);
        actionsFlow.Name = "actionsFlow";
        actionsFlow.Size = new Size(307, 68);
        actionsFlow.TabIndex = 1;
        // 
        // btnAdd
        // 
        btnAdd.AutoSize = true;
        btnAdd.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
        btnAdd.Location = new Point(233, 3);
        btnAdd.Name = "btnAdd";
        btnAdd.Size = new Size(71, 29);
        btnAdd.TabIndex = 0;
        btnAdd.Text = "Add";
        btnAdd.UseVisualStyleBackColor = true;
        btnAdd.Click += btnAdd_Click;
        // 
        // btnEdit
        // 
        btnEdit.AutoSize = true;
        btnEdit.Location = new Point(152, 3);
        btnEdit.Name = "btnEdit";
        btnEdit.Size = new Size(75, 29);
        btnEdit.TabIndex = 1;
        btnEdit.Text = "Edit";
        btnEdit.UseVisualStyleBackColor = true;
        btnEdit.Click += btnEdit_Click;
        // 
        // btnDelete
        // 
        btnDelete.AutoSize = true;
        btnDelete.Location = new Point(71, 3);
        btnDelete.Name = "btnDelete";
        btnDelete.Size = new Size(75, 29);
        btnDelete.TabIndex = 2;
        btnDelete.Text = "Delete";
        btnDelete.UseVisualStyleBackColor = true;
        btnDelete.Click += btnDelete_Click;
        // 
        // btnExport
        // 
        btnExport.AutoSize = true;
        btnExport.Location = new Point(229, 38);
        btnExport.Name = "btnExport";
        btnExport.Size = new Size(75, 27);
        btnExport.TabIndex = 3;
        btnExport.Text = "Export...";
        btnExport.UseVisualStyleBackColor = true;
        btnExport.Click += btnExport_Click;
        // 
        // dgvEmployees
        // 
        dgvEmployees.AllowUserToAddRows = false;
        dgvEmployees.AllowUserToDeleteRows = false;
        dgvEmployees.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvEmployees.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvEmployees.Columns.AddRange(new DataGridViewColumn[] { colName, colPosition, colSalary });
        dgvEmployees.Dock = DockStyle.Fill;
        dgvEmployees.Location = new Point(3, 83);
        dgvEmployees.MultiSelect = false;
        dgvEmployees.Name = "dgvEmployees";
        dgvEmployees.ReadOnly = true;
        dgvEmployees.RowHeadersVisible = false;
        dgvEmployees.RowTemplate.Height = 25;
        dgvEmployees.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvEmployees.Size = new Size(978, 475);
        dgvEmployees.TabIndex = 2;
        dgvEmployees.ColumnHeaderMouseClick += dgvEmployees_ColumnHeaderMouseClick;
        // 
        // colName
        // 
        colName.DataPropertyName = "Name";
        colName.FillWeight = 35F;
        colName.HeaderText = "Name";
        colName.Name = "colName";
        colName.ReadOnly = true;
        colName.SortMode = DataGridViewColumnSortMode.Programmatic;
        // 
        // colPosition
        // 
        colPosition.DataPropertyName = "Position";
        colPosition.FillWeight = 35F;
        colPosition.HeaderText = "Position";
        colPosition.Name = "colPosition";
        colPosition.ReadOnly = true;
        colPosition.SortMode = DataGridViewColumnSortMode.Programmatic;
        // 
        // colSalary
        // 
        colSalary.DataPropertyName = "Salary";
        dataGridViewCellStyle1.Format = "C2";
        dataGridViewCellStyle1.NullValue = null;
        colSalary.DefaultCellStyle = dataGridViewCellStyle1;
        colSalary.FillWeight = 30F;
        colSalary.HeaderText = "Salary";
        colSalary.Name = "colSalary";
        colSalary.ReadOnly = true;
        colSalary.SortMode = DataGridViewColumnSortMode.Programmatic;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(984, 561);
        Controls.Add(tableLayoutPanel1);
        MinimumSize = new Size(780, 480);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Mini Employee Database";
        Load += MainForm_Load;
        tableLayoutPanel1.ResumeLayout(false);
        topLayout.ResumeLayout(false);
        topLayout.PerformLayout();
        filtersFlow.ResumeLayout(false);
        filtersFlow.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numSalaryMin).EndInit();
        ((System.ComponentModel.ISupportInitialize)numSalaryMax).EndInit();
        actionsFlow.ResumeLayout(false);
        actionsFlow.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvEmployees).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel tableLayoutPanel1;
    private DataGridView dgvEmployees;
    private TableLayoutPanel topLayout;
    private FlowLayoutPanel filtersFlow;
    private Label lblSearch;
    private TextBox txtSearch;
    private Button btnSearch;
    private Label lblPosition;
    private ComboBox cmbPositionFilter;
    private Label lblSalary;
    private NumericUpDown numSalaryMin;
    private Label lblSalarySeparator;
    private NumericUpDown numSalaryMax;
    private Button btnClearFilters;
    private FlowLayoutPanel actionsFlow;
    private Button btnAdd;
    private Button btnEdit;
    private Button btnDelete;
    private Button btnExport;
    private DataGridViewTextBoxColumn colName;
    private DataGridViewTextBoxColumn colPosition;
    private DataGridViewTextBoxColumn colSalary;
}
