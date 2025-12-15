namespace SortingAlgorithmVisualizer;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private Panel panelCanvas;
    private Button btnGenerate;
    private Button btnStart;
    private Button btnPauseStop;
    private TrackBar trackBarSpeed;
    private Label labelSpeed;
    private NumericUpDown numericSize;
    private Label labelSize;
    private Label labelStatus;
    private Panel panelControls;
    private Label labelMaxValue;
    private NumericUpDown numericMaxValue;

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
        this.panelCanvas = new System.Windows.Forms.Panel();
        this.panelControls = new System.Windows.Forms.Panel();
        this.btnGenerate = new System.Windows.Forms.Button();
        this.btnStart = new System.Windows.Forms.Button();
        this.btnPauseStop = new System.Windows.Forms.Button();
        this.labelSize = new System.Windows.Forms.Label();
        this.numericSize = new System.Windows.Forms.NumericUpDown();
        this.labelSpeed = new System.Windows.Forms.Label();
        this.trackBarSpeed = new System.Windows.Forms.TrackBar();
        this.labelStatus = new System.Windows.Forms.Label();
        this.labelMaxValue = new System.Windows.Forms.Label();
        this.numericMaxValue = new System.Windows.Forms.NumericUpDown();
        this.panelControls.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numericSize)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.trackBarSpeed)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numericMaxValue)).BeginInit();
        this.SuspendLayout();
        // 
        // panelCanvas
        // 
        this.panelCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.panelCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
        this.panelCanvas.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.panelCanvas.Location = new System.Drawing.Point(12, 90);
        this.panelCanvas.Name = "panelCanvas";
        this.panelCanvas.Size = new System.Drawing.Size(960, 520);
        this.panelCanvas.TabIndex = 0;
        this.panelCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.panelCanvas_Paint);
        this.panelCanvas.Resize += new System.EventHandler(this.panelCanvas_Resize);
        // 
        // panelControls
        // 
        this.panelControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.panelControls.BackColor = System.Drawing.Color.WhiteSmoke;
        this.panelControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.panelControls.Controls.Add(this.btnGenerate);
        this.panelControls.Controls.Add(this.btnStart);
        this.panelControls.Controls.Add(this.btnPauseStop);
        this.panelControls.Controls.Add(this.labelSize);
        this.panelControls.Controls.Add(this.numericSize);
        this.panelControls.Controls.Add(this.labelSpeed);
        this.panelControls.Controls.Add(this.labelMaxValue);
        this.panelControls.Controls.Add(this.numericMaxValue);
        this.panelControls.Controls.Add(this.trackBarSpeed);
        this.panelControls.Location = new System.Drawing.Point(12, 12);
        this.panelControls.Name = "panelControls";
        this.panelControls.Size = new System.Drawing.Size(960, 64);
        this.panelControls.TabIndex = 5;
        // 
        // btnGenerate
        // 
        this.btnGenerate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnGenerate.Location = new System.Drawing.Point(10, 12);
        this.btnGenerate.Name = "btnGenerate";
        this.btnGenerate.Size = new System.Drawing.Size(110, 40);
        this.btnGenerate.TabIndex = 0;
        this.btnGenerate.Text = "Randomize";
        this.btnGenerate.UseVisualStyleBackColor = true;
        this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
        // 
        // btnStart
        // 
        this.btnStart.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnStart.Location = new System.Drawing.Point(130, 12);
        this.btnStart.Name = "btnStart";
        this.btnStart.Size = new System.Drawing.Size(90, 40);
        this.btnStart.TabIndex = 1;
        this.btnStart.Text = "Start";
        this.btnStart.UseVisualStyleBackColor = true;
        this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
        // 
        // btnPauseStop
        // 
        this.btnPauseStop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnPauseStop.Location = new System.Drawing.Point(230, 12);
        this.btnPauseStop.Name = "btnPauseStop";
        this.btnPauseStop.Size = new System.Drawing.Size(110, 40);
        this.btnPauseStop.TabIndex = 2;
        this.btnPauseStop.Text = "Pause / Stop";
        this.btnPauseStop.UseVisualStyleBackColor = true;
        this.btnPauseStop.Click += new System.EventHandler(this.btnPauseStop_Click);
        // 
        // labelSize
        // 
        this.labelSize.AutoSize = true;
        this.labelSize.Location = new System.Drawing.Point(360, 10);
        this.labelSize.Name = "labelSize";
        this.labelSize.Size = new System.Drawing.Size(62, 15);
        this.labelSize.TabIndex = 3;
        this.labelSize.Text = "Array size:";
        // 
        // numericSize
        // 
        this.numericSize.Location = new System.Drawing.Point(360, 28);
        this.numericSize.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
        this.numericSize.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
        this.numericSize.Name = "numericSize";
        this.numericSize.Size = new System.Drawing.Size(70, 23);
        this.numericSize.TabIndex = 4;
        this.numericSize.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
        this.numericSize.ValueChanged += new System.EventHandler(this.numericSize_ValueChanged);
        // 
        // labelSpeed
        // 
        this.labelSpeed.AutoSize = true;
        this.labelSpeed.Location = new System.Drawing.Point(450, 10);
        this.labelSpeed.Name = "labelSpeed";
        this.labelSpeed.Size = new System.Drawing.Size(113, 15);
        this.labelSpeed.TabIndex = 5;
        this.labelSpeed.Text = "Step delay: 275 ms";
        // 
        // labelMaxValue
        // 
        this.labelMaxValue.AutoSize = true;
        this.labelMaxValue.Location = new System.Drawing.Point(720, 10);
        this.labelMaxValue.Name = "labelMaxValue";
        this.labelMaxValue.Size = new System.Drawing.Size(65, 15);
        this.labelMaxValue.TabIndex = 7;
        this.labelMaxValue.Text = "Max value:";
        // 
        // numericMaxValue
        // 
        this.numericMaxValue.Location = new System.Drawing.Point(720, 28);
        this.numericMaxValue.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
        this.numericMaxValue.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
        this.numericMaxValue.Name = "numericMaxValue";
        this.numericMaxValue.Size = new System.Drawing.Size(80, 23);
        this.numericMaxValue.TabIndex = 8;
        this.numericMaxValue.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
        this.numericMaxValue.ValueChanged += new System.EventHandler(this.numericMaxValue_ValueChanged);
        // 
        // trackBarSpeed
        // 
        this.trackBarSpeed.LargeChange = 10;
        this.trackBarSpeed.Location = new System.Drawing.Point(450, 25);
        this.trackBarSpeed.Maximum = 100;
        this.trackBarSpeed.Minimum = 1;
        this.trackBarSpeed.Name = "trackBarSpeed";
        this.trackBarSpeed.Size = new System.Drawing.Size(250, 45);
        this.trackBarSpeed.TabIndex = 6;
        this.trackBarSpeed.TickFrequency = 10;
        this.trackBarSpeed.Value = 50;
        this.trackBarSpeed.Scroll += new System.EventHandler(this.trackBarSpeed_Scroll);
        // 
        // labelStatus
        // 
        this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.labelStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.labelStatus.Location = new System.Drawing.Point(12, 617);
        this.labelStatus.Name = "labelStatus";
        this.labelStatus.Size = new System.Drawing.Size(960, 23);
        this.labelStatus.TabIndex = 6;
        this.labelStatus.Text = "Status: Ready";
        this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(984, 649);
        this.Controls.Add(this.labelStatus);
        this.Controls.Add(this.panelControls);
        this.Controls.Add(this.panelCanvas);
        this.MinimumSize = new System.Drawing.Size(840, 600);
        this.Name = "Form1";
        this.Text = "Sorting Algorithm Visualizer";
        this.panelControls.ResumeLayout(false);
        this.panelControls.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numericSize)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.trackBarSpeed)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numericMaxValue)).EndInit();
        this.ResumeLayout(false);
    }

    #endregion
}
