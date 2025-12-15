namespace Task2;

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
        this.components = new System.ComponentModel.Container();
        this.fractalBox = new System.Windows.Forms.PictureBox();
        this.statusStrip = new System.Windows.Forms.StatusStrip();
        this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
        this.infoLabel = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.fractalBox)).BeginInit();
        this.statusStrip.SuspendLayout();
        this.SuspendLayout();
        // 
        // fractalBox
        // 
        this.fractalBox.BackColor = System.Drawing.Color.Black;
        this.fractalBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.fractalBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.fractalBox.Location = new System.Drawing.Point(0, 0);
        this.fractalBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        this.fractalBox.Name = "fractalBox";
        this.fractalBox.Size = new System.Drawing.Size(984, 641);
        this.fractalBox.TabIndex = 0;
        this.fractalBox.TabStop = false;
        // 
        // statusStrip
        // 
        this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
        this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
        this.statusStrip.Location = new System.Drawing.Point(0, 619);
        this.statusStrip.Name = "statusStrip";
        this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
        this.statusStrip.Size = new System.Drawing.Size(984, 22);
        this.statusStrip.TabIndex = 1;
        // 
        // statusLabel
        // 
        this.statusLabel.Name = "statusLabel";
        this.statusLabel.Size = new System.Drawing.Size(171, 17);
        this.statusLabel.Text = "Готово к визуализации...";
        // 
        // infoLabel
        // 
        this.infoLabel.AutoSize = true;
        this.infoLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
        this.infoLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.infoLabel.ForeColor = System.Drawing.Color.Gainsboro;
        this.infoLabel.Location = new System.Drawing.Point(12, 11);
        this.infoLabel.Name = "infoLabel";
        this.infoLabel.Padding = new System.Windows.Forms.Padding(6);
        this.infoLabel.Size = new System.Drawing.Size(308, 27);
        this.infoLabel.TabIndex = 2;
        this.infoLabel.Text = "Колесо мыши – масштаб, ЛКМ – перетащить";
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(984, 641);
        this.Controls.Add(this.infoLabel);
        this.Controls.Add(this.statusStrip);
        this.Controls.Add(this.fractalBox);
        this.MinimumSize = new System.Drawing.Size(640, 480);
        this.Name = "Form1";
        this.Text = "Фрактал Мандельброта";
        ((System.ComponentModel.ISupportInitialize)(this.fractalBox)).EndInit();
        this.statusStrip.ResumeLayout(false);
        this.statusStrip.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.PictureBox fractalBox;
    private System.Windows.Forms.StatusStrip statusStrip;
    private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    private System.Windows.Forms.Label infoLabel;
}
