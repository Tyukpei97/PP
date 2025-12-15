// Этот файл автоматически сгенерирован и содержит код для дизайна главной формы, включая элементы управления.

namespace MyCalendarApp
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освобождает все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">true, если управляемые ресурсы должны быть освобождены; иначе — false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный Windows Form Designer

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.eventListBox = new System.Windows.Forms.ListBox();
            this.addEventButton = new System.Windows.Forms.Button();
            this.editEventButton = new System.Windows.Forms.Button();
            this.deleteEventButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // dateTimePicker
            // 
            this.dateTimePicker.Location = new System.Drawing.Point(12, 12);
            this.dateTimePicker.Name = "dateTimePicker";
            this.dateTimePicker.Size = new System.Drawing.Size(200, 23);
            this.dateTimePicker.TabIndex = 0;
            this.dateTimePicker.ValueChanged += new System.EventHandler(this.dateTimePicker_ValueChanged);
            // 
            // eventListBox
            // 
            this.eventListBox.FormattingEnabled = true;
            this.eventListBox.ItemHeight = 15;
            this.eventListBox.Location = new System.Drawing.Point(12, 41);
            this.eventListBox.Name = "eventListBox";
            this.eventListBox.Size = new System.Drawing.Size(260, 169);
            this.eventListBox.TabIndex = 1;
            // 
            // addEventButton
            // 
            this.addEventButton.Location = new System.Drawing.Point(12, 216);
            this.addEventButton.Name = "addEventButton";
            this.addEventButton.Size = new System.Drawing.Size(75, 23);
            this.addEventButton.TabIndex = 2;
            this.addEventButton.Text = "Добавить";
            this.addEventButton.UseVisualStyleBackColor = true;
            this.addEventButton.Click += new System.EventHandler(this.addEventButton_Click);
            // 
            // editEventButton
            // 
            this.editEventButton.Location = new System.Drawing.Point(93, 216);
            this.editEventButton.Name = "editEventButton";
            this.editEventButton.Size = new System.Drawing.Size(75, 23);
            this.editEventButton.TabIndex = 3;
            this.editEventButton.Text = "Редактировать";
            this.editEventButton.UseVisualStyleBackColor = true;
            this.editEventButton.Click += new System.EventHandler(this.editEventButton_Click);
            // 
            // deleteEventButton
            // 
            this.deleteEventButton.Location = new System.Drawing.Point(174, 216);
            this.deleteEventButton.Name = "deleteEventButton";
            this.deleteEventButton.Size = new System.Drawing.Size(75, 23);
            this.deleteEventButton.TabIndex = 4;
            this.deleteEventButton.Text = "Удалить";
            this.deleteEventButton.UseVisualStyleBackColor = true;
            this.deleteEventButton.Click += new System.EventHandler(this.deleteEventButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.deleteEventButton);
            this.Controls.Add(this.editEventButton);
            this.Controls.Add(this.addEventButton);
            this.Controls.Add(this.eventListBox);
            this.Controls.Add(this.dateTimePicker);
            this.Name = "MainForm";
            this.Text = "Календарь событий";
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.DateTimePicker dateTimePicker;
        private System.Windows.Forms.ListBox eventListBox;
        private System.Windows.Forms.Button addEventButton;
        private System.Windows.Forms.Button editEventButton;
        private System.Windows.Forms.Button deleteEventButton;
    }
}