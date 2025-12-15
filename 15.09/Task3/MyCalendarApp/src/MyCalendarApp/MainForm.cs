using System;
using System.Windows.Forms;

namespace MyCalendarApp
{
    public partial class MainForm : Form
    {
        private readonly Services.EventService _eventService;
        private readonly Services.PersistenceService _persistenceService;

        public MainForm()
        {
            InitializeComponent();

            var dataFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "events.json");
            _persistenceService = new Services.PersistenceService(dataFile);
            _eventService = new Services.EventService();

            // Load persisted events asynchronously
            _ = LoadEventsAsync();
        }

        private async System.Threading.Tasks.Task LoadEventsAsync()
        {
            var loaded = await _persistenceService.LoadEventsAsync();
            if (loaded != null)
            {
                foreach (var e in loaded)
                    _eventService.AddEvent(e);
            }
            UpdateEventListForSelectedDate();
        }

        private void UpdateEventListForSelectedDate()
        {
            if (dateTimePicker == null || eventListBox == null) return;
            var selected = dateTimePicker.Value.Date;
            eventListBox.Items.Clear();
            var items = _eventService.GetAllEvents().FindAll(e => e.Date.Date == selected);
            items.Sort((a, b) => a.Time.CompareTo(b.Time));
            foreach (var it in items)
            {
                eventListBox.Items.Add(it);
            }
        }

        private void dateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            UpdateEventListForSelectedDate();
        }

        private async void addEventButton_Click(object sender, EventArgs e)
        {
            var editor = new Views.EventEditorForm();
            editor.LoadEvent(new Models.EventItem { Date = dateTimePicker.Value.Date });
            var res = editor.ShowDialog(this);
            if (res == DialogResult.OK && !editor.IsDeleted)
            {
                var newEvent = editor.GetEvent();
                _eventService.AddEvent(newEvent);
                await _persistenceService.SaveEventsAsync(_eventService.GetAllEvents());
                UpdateEventListForSelectedDate();
            }
        }

        private async void editEventButton_Click(object sender, EventArgs e)
        {
            if (eventListBox.SelectedItem is Models.EventItem selected)
            {
                var editor = new Views.EventEditorForm();
                editor.LoadEvent(selected);
                var res = editor.ShowDialog(this);
                if (res == DialogResult.OK)
                {
                    if (editor.IsDeleted)
                    {
                        _eventService.DeleteEvent(selected.Id);
                    }
                    else
                    {
                        var updated = editor.GetEvent();
                        _eventService.UpdateEvent(updated);
                    }
                    await _persistenceService.SaveEventsAsync(_eventService.GetAllEvents());
                    UpdateEventListForSelectedDate();
                }
            }
            else
            {
                MessageBox.Show(this, "Select an event to edit.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void deleteEventButton_Click(object sender, EventArgs e)
        {
            if (eventListBox.SelectedItem is Models.EventItem selected)
            {
                var ans = MessageBox.Show(this, "Delete selected event?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (ans == DialogResult.Yes)
                {
                    _eventService.DeleteEvent(selected.Id);
                    await _persistenceService.SaveEventsAsync(_eventService.GetAllEvents());
                    UpdateEventListForSelectedDate();
                }
            }
            else
            {
                MessageBox.Show(this, "Select an event to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}