namespace Puzzle15Game;

partial class Form1
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
        this.boardTable = new System.Windows.Forms.TableLayoutPanel();
        this.headerPanel = new System.Windows.Forms.Panel();
        this.statusLabel = new System.Windows.Forms.Label();
        this.newGameButton = new System.Windows.Forms.Button();
        this.headerPanel.SuspendLayout();
        this.SuspendLayout();
        // 
        // boardTable
        // 
        this.boardTable.ColumnCount = 4;
        this.boardTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.boardTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.boardTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.boardTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.boardTable.Dock = System.Windows.Forms.DockStyle.Fill;
        this.boardTable.Location = new System.Drawing.Point(0, 60);
        this.boardTable.Name = "boardTable";
        this.boardTable.Padding = new System.Windows.Forms.Padding(10);
        this.boardTable.RowCount = 4;
        this.boardTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.boardTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.boardTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.boardTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.boardTable.Size = new System.Drawing.Size(444, 449);
        this.boardTable.TabIndex = 1;
        // 
        // headerPanel
        // 
        this.headerPanel.Controls.Add(this.statusLabel);
        this.headerPanel.Controls.Add(this.newGameButton);
        this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this.headerPanel.Location = new System.Drawing.Point(0, 0);
        this.headerPanel.Name = "headerPanel";
        this.headerPanel.Padding = new System.Windows.Forms.Padding(10, 10, 10, 0);
        this.headerPanel.Size = new System.Drawing.Size(444, 60);
        this.headerPanel.TabIndex = 0;
        // 
        // statusLabel
        // 
        this.statusLabel.AutoSize = true;
        this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.statusLabel.Location = new System.Drawing.Point(130, 18);
        this.statusLabel.Name = "statusLabel";
        this.statusLabel.Size = new System.Drawing.Size(155, 23);
        this.statusLabel.TabIndex = 1;
        this.statusLabel.Text = "Arrange tiles 1-15.";
        // 
        // newGameButton
        // 
        this.newGameButton.AutoSize = true;
        this.newGameButton.BackColor = System.Drawing.Color.SteelBlue;
        this.newGameButton.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
        this.newGameButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.newGameButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.newGameButton.ForeColor = System.Drawing.Color.White;
        this.newGameButton.Location = new System.Drawing.Point(10, 14);
        this.newGameButton.Name = "newGameButton";
        this.newGameButton.Size = new System.Drawing.Size(109, 33);
        this.newGameButton.TabIndex = 0;
        this.newGameButton.Text = "New Game";
        this.newGameButton.UseVisualStyleBackColor = false;
        this.newGameButton.Click += new System.EventHandler(this.NewGameButton_Click);
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(444, 509);
        this.Controls.Add(this.boardTable);
        this.Controls.Add(this.headerPanel);
        this.MinimumSize = new System.Drawing.Size(360, 430);
        this.Name = "Form1";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "15-Puzzle Mini Game";
        this.headerPanel.ResumeLayout(false);
        this.headerPanel.PerformLayout();
        this.ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel boardTable;
    private System.Windows.Forms.Panel headerPanel;
    private System.Windows.Forms.Label statusLabel;
    private System.Windows.Forms.Button newGameButton;
}
