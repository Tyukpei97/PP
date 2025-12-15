using System;
using System.Drawing;
using System.Windows.Forms;
using MyCalendarApp.Models;

namespace MyCalendarApp.Views
{
    public class EventEditorForm : Form
    {
        private TextBox titleTextBox;
        private DateTimePicker datePicker;
        private DateTimePicker timePicker;
        private TextBox descriptionTextBox;
        private Button btnSave;
        private Button btnCancel;
        private Button btnDelete;

        private EventItem _editingEvent;
        public bool IsDeleted { get; private set; }

        public EventEditorForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.titleTextBox = new TextBox() { Location = new Point(12, 25), Width = 360 };
            var titleLabel = new Label() { Text = "Title:", Location = new Point(12, 5), AutoSize = true };

            this.datePicker = new DateTimePicker() { Location = new Point(12, 65), Width = 180, Format = DateTimePickerFormat.Short };
            var dateLabel = new Label() { Text = "Date:", Location = new Point(12, 45), AutoSize = true };

            this.timePicker = new DateTimePicker() { Location = new Point(200, 65), Width = 120, Format = DateTimePickerFormat.Time, ShowUpDown = true }; 
            var timeLabel = new Label() { Text = "Time:", Location = new Point(200, 45), AutoSize = true };

            var descLabel = new Label() { Text = "Description:", Location = new Point(12, 95), AutoSize = true };
            this.descriptionTextBox = new TextBox() { Location = new Point(12, 115), Width = 360, Height = 80, Multiline = true, ScrollBars = ScrollBars.Vertical };

            this.btnSave = new Button() { Text = "OK", Location = new Point(216, 205), Width = 75 };
            this.btnCancel = new Button() { Text = "Cancel", Location = new Point(297, 205), Width = 75 };
            this.btnDelete = new Button() { Text = "Delete", Location = new Point(12, 205), Width = 75, ForeColor = Color.Red };

            this.btnSave.Click += BtnSave_Click;
            this.btnCancel.Click += BtnCancel_Click;
            this.btnDelete.Click += BtnDelete_Click;

            this.ClientSize = new Size(390, 240);
            this.Text = "Event Editor";

            this.Controls.Add(titleLabel);
            this.Controls.Add(this.titleTextBox);
            this.Controls.Add(dateLabel);
            this.Controls.Add(this.datePicker);
            this.Controls.Add(timeLabel);
            this.Controls.Add(this.timePicker);
            this.Controls.Add(descLabel);
            this.Controls.Add(this.descriptionTextBox);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnDelete);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            IsDeleted = false;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(this, "Delete this event?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res == DialogResult.Yes)
            {
                IsDeleted = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        public void LoadEvent(EventItem eventItem)
        {
            if (eventItem == null) return;
            _editingEvent = eventItem;
            titleTextBox.Text = eventItem.Title;
            datePicker.Value = eventItem.Date.Date;
            // time stored as TimeSpan — set DateTimePicker's time portion
            var dt = DateTime.Today + eventItem.Time;
            timePicker.Value = dt;
            descriptionTextBox.Text = eventItem.Description;
        }

        public EventItem GetEvent()
        {
            if (_editingEvent == null)
            {
                var item = new EventItem
                {
                    Title = titleTextBox.Text.Trim(),
                    Date = datePicker.Value.Date,
                    Time = timePicker.Value.TimeOfDay,
                    Description = descriptionTextBox.Text.Trim()
                };
                return item;
            }
            else
            {
                _editingEvent.Title = titleTextBox.Text.Trim();
                _editingEvent.Date = datePicker.Value.Date;
                _editingEvent.Time = timePicker.Value.TimeOfDay;
                _editingEvent.Description = descriptionTextBox.Text.Trim();
                return _editingEvent;
            }
        }
    }
}