namespace FunctionGraphingCalculator;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.TableLayoutPanel mainLayout;
    private System.Windows.Forms.FlowLayoutPanel controlPanel;
    private System.Windows.Forms.TextBox txtExpression;
    private System.Windows.Forms.TextBox txtFromX;
    private System.Windows.Forms.TextBox txtToX;
    private System.Windows.Forms.TextBox txtSamples;
    private System.Windows.Forms.Button btnPlot;
    private System.Windows.Forms.Button btnClear;
    private System.Windows.Forms.Button btnZoomIn;
    private System.Windows.Forms.Button btnZoomOut;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.PictureBox canvas;

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
        this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
        this.controlPanel = new System.Windows.Forms.FlowLayoutPanel();
        System.Windows.Forms.Label labelFx = new System.Windows.Forms.Label();
        System.Windows.Forms.Label labelFrom = new System.Windows.Forms.Label();
        System.Windows.Forms.Label labelTo = new System.Windows.Forms.Label();
        System.Windows.Forms.Label labelSamples = new System.Windows.Forms.Label();
        this.txtExpression = new System.Windows.Forms.TextBox();
        this.txtFromX = new System.Windows.Forms.TextBox();
        this.txtToX = new System.Windows.Forms.TextBox();
        this.txtSamples = new System.Windows.Forms.TextBox();
        this.btnPlot = new System.Windows.Forms.Button();
        this.btnClear = new System.Windows.Forms.Button();
        this.btnZoomIn = new System.Windows.Forms.Button();
        this.btnZoomOut = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        this.canvas = new System.Windows.Forms.PictureBox();
        this.mainLayout.SuspendLayout();
        this.controlPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.canvas)).BeginInit();
        this.SuspendLayout();
        // 
        // mainLayout
        // 
        this.mainLayout.ColumnCount = 1;
        this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.mainLayout.Controls.Add(this.controlPanel, 0, 0);
        this.mainLayout.Controls.Add(this.lblStatus, 0, 1);
        this.mainLayout.Controls.Add(this.canvas, 0, 2);
        this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this.mainLayout.Location = new System.Drawing.Point(0, 0);
        this.mainLayout.Name = "mainLayout";
        this.mainLayout.Padding = new System.Windows.Forms.Padding(10, 8, 10, 10);
        this.mainLayout.RowCount = 3;
        this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.mainLayout.Size = new System.Drawing.Size(1064, 681);
        this.mainLayout.TabIndex = 0;
        // 
        // controlPanel
        // 
        this.controlPanel.AutoSize = true;
        this.controlPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.controlPanel.Controls.Add(labelFx);
        this.controlPanel.Controls.Add(this.txtExpression);
        this.controlPanel.Controls.Add(labelFrom);
        this.controlPanel.Controls.Add(this.txtFromX);
        this.controlPanel.Controls.Add(labelTo);
        this.controlPanel.Controls.Add(this.txtToX);
        this.controlPanel.Controls.Add(labelSamples);
        this.controlPanel.Controls.Add(this.txtSamples);
        this.controlPanel.Controls.Add(this.btnPlot);
        this.controlPanel.Controls.Add(this.btnClear);
        this.controlPanel.Controls.Add(this.btnZoomIn);
        this.controlPanel.Controls.Add(this.btnZoomOut);
        this.controlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.controlPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this.controlPanel.Location = new System.Drawing.Point(10, 8);
        this.controlPanel.Margin = new System.Windows.Forms.Padding(0);
        this.controlPanel.Name = "controlPanel";
        this.controlPanel.Padding = new System.Windows.Forms.Padding(2);
        this.controlPanel.Size = new System.Drawing.Size(1044, 41);
        this.controlPanel.TabIndex = 0;
        this.controlPanel.WrapContents = false;
        // 
        // labelFx
        // 
        labelFx.AutoSize = true;
        labelFx.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        labelFx.Location = new System.Drawing.Point(5, 5);
        labelFx.Margin = new System.Windows.Forms.Padding(3);
        labelFx.Name = "labelFx";
        labelFx.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
        labelFx.Size = new System.Drawing.Size(31, 21);
        labelFx.TabIndex = 0;
        labelFx.Text = "f(x)";
        // 
        // txtExpression
        // 
        this.txtExpression.Location = new System.Drawing.Point(42, 5);
        this.txtExpression.Margin = new System.Windows.Forms.Padding(3, 5, 6, 3);
        this.txtExpression.Name = "txtExpression";
        this.txtExpression.PlaceholderText = "sin(x)";
        this.txtExpression.Size = new System.Drawing.Size(220, 27);
        this.txtExpression.TabIndex = 1;
        this.txtExpression.Text = "sin(x)";
        // 
        // labelFrom
        // 
        labelFrom.AutoSize = true;
        labelFrom.Location = new System.Drawing.Point(268, 8);
        labelFrom.Margin = new System.Windows.Forms.Padding(0, 8, 3, 0);
        labelFrom.Name = "labelFrom";
        labelFrom.Size = new System.Drawing.Size(59, 20);
        labelFrom.TabIndex = 2;
        labelFrom.Text = "From X";
        // 
        // txtFromX
        // 
        this.txtFromX.Location = new System.Drawing.Point(333, 5);
        this.txtFromX.Margin = new System.Windows.Forms.Padding(3, 5, 6, 3);
        this.txtFromX.Name = "txtFromX";
        this.txtFromX.Size = new System.Drawing.Size(80, 27);
        this.txtFromX.TabIndex = 3;
        this.txtFromX.Text = "-10";
        // 
        // labelTo
        // 
        labelTo.AutoSize = true;
        labelTo.Location = new System.Drawing.Point(422, 8);
        labelTo.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
        labelTo.Name = "labelTo";
        labelTo.Size = new System.Drawing.Size(45, 20);
        labelTo.TabIndex = 4;
        labelTo.Text = "To X";
        // 
        // txtToX
        // 
        this.txtToX.Location = new System.Drawing.Point(473, 5);
        this.txtToX.Margin = new System.Windows.Forms.Padding(3, 5, 6, 3);
        this.txtToX.Name = "txtToX";
        this.txtToX.Size = new System.Drawing.Size(80, 27);
        this.txtToX.TabIndex = 5;
        this.txtToX.Text = "10";
        // 
        // labelSamples
        // 
        labelSamples.AutoSize = true;
        labelSamples.Location = new System.Drawing.Point(562, 8);
        labelSamples.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
        labelSamples.Name = "labelSamples";
        labelSamples.Size = new System.Drawing.Size(66, 20);
        labelSamples.TabIndex = 6;
        labelSamples.Text = "Samples";
        // 
        // txtSamples
        // 
        this.txtSamples.Location = new System.Drawing.Point(634, 5);
        this.txtSamples.Margin = new System.Windows.Forms.Padding(3, 5, 6, 3);
        this.txtSamples.Name = "txtSamples";
        this.txtSamples.Size = new System.Drawing.Size(70, 27);
        this.txtSamples.TabIndex = 7;
        this.txtSamples.Text = "400";
        // 
        // btnPlot
        // 
        this.btnPlot.AutoSize = true;
        this.btnPlot.Location = new System.Drawing.Point(713, 4);
        this.btnPlot.Margin = new System.Windows.Forms.Padding(3, 4, 3, 3);
        this.btnPlot.Name = "btnPlot";
        this.btnPlot.Size = new System.Drawing.Size(60, 30);
        this.btnPlot.TabIndex = 8;
        this.btnPlot.Text = "Plot";
        this.btnPlot.UseVisualStyleBackColor = true;
        this.btnPlot.Click += new System.EventHandler(this.BtnPlot_Click);
        // 
        // btnClear
        // 
        this.btnClear.AutoSize = true;
        this.btnClear.Location = new System.Drawing.Point(779, 4);
        this.btnClear.Margin = new System.Windows.Forms.Padding(6, 4, 3, 3);
        this.btnClear.Name = "btnClear";
        this.btnClear.Size = new System.Drawing.Size(64, 30);
        this.btnClear.TabIndex = 9;
        this.btnClear.Text = "Clear";
        this.btnClear.UseVisualStyleBackColor = true;
        this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
        // 
        // btnZoomIn
        // 
        this.btnZoomIn.AutoSize = true;
        this.btnZoomIn.Location = new System.Drawing.Point(849, 4);
        this.btnZoomIn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 3);
        this.btnZoomIn.Name = "btnZoomIn";
        this.btnZoomIn.Size = new System.Drawing.Size(79, 30);
        this.btnZoomIn.TabIndex = 10;
        this.btnZoomIn.Text = "Zoom In";
        this.btnZoomIn.UseVisualStyleBackColor = true;
        this.btnZoomIn.Click += new System.EventHandler(this.BtnZoomIn_Click);
        // 
        // btnZoomOut
        // 
        this.btnZoomOut.AutoSize = true;
        this.btnZoomOut.Location = new System.Drawing.Point(934, 4);
        this.btnZoomOut.Margin = new System.Windows.Forms.Padding(3, 4, 3, 3);
        this.btnZoomOut.Name = "btnZoomOut";
        this.btnZoomOut.Size = new System.Drawing.Size(88, 30);
        this.btnZoomOut.TabIndex = 11;
        this.btnZoomOut.Text = "Zoom Out";
        this.btnZoomOut.UseVisualStyleBackColor = true;
        this.btnZoomOut.Click += new System.EventHandler(this.BtnZoomOut_Click);
        // 
        // lblStatus
        // 
        this.lblStatus.AutoSize = true;
        this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(96)))), ((int)(((byte)(32)))));
        this.lblStatus.Location = new System.Drawing.Point(13, 49);
        this.lblStatus.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new System.Drawing.Size(1038, 20);
        this.lblStatus.TabIndex = 1;
        this.lblStatus.Text = "Enter a function and press Plot.";
        // 
        // canvas
        // 
        this.canvas.BackColor = System.Drawing.Color.White;
        this.canvas.Dock = System.Windows.Forms.DockStyle.Fill;
        this.canvas.Location = new System.Drawing.Point(13, 75);
        this.canvas.Name = "canvas";
        this.canvas.Size = new System.Drawing.Size(1038, 593);
        this.canvas.TabIndex = 2;
        this.canvas.TabStop = false;
        this.canvas.Paint += new System.Windows.Forms.PaintEventHandler(this.Canvas_Paint);
        this.canvas.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseDown);
        this.canvas.MouseEnter += new System.EventHandler(this.Canvas_MouseEnter);
        this.canvas.MouseLeave += new System.EventHandler(this.Canvas_MouseLeave);
        this.canvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseMove);
        this.canvas.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseUp);
        this.canvas.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseWheel);
        this.canvas.Resize += new System.EventHandler(this.Canvas_Resize);
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1064, 681);
        this.Controls.Add(this.mainLayout);
        this.MinimumSize = new System.Drawing.Size(900, 620);
        this.Name = "Form1";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Function Graphing Calculator";
        this.mainLayout.ResumeLayout(false);
        this.mainLayout.PerformLayout();
        this.controlPanel.ResumeLayout(false);
        this.controlPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.canvas)).EndInit();
        this.ResumeLayout(false);

    }

    #endregion
}
