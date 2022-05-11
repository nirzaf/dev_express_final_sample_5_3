using DevExpress.XtraEditors;
using EventsDB;
using DevExpress.XtraBars.Docking2010;
using System;
using System.Linq;

namespace sample_5_3 {
  public partial class EventView : XtraUserControl {
    public EventView() {
      InitializeComponent();

      eventGrid.BindingContext[eventGrid.DataSource].CurrentChanged += new EventHandler(eventGridView_CurrentChanged);
    }

    private void EventView_Load(object sender, System.EventArgs e) {
      TransactionalChangeHandler.Hookup<Event>(
        DocumentManager.FromControl(ParentForm).View, unitOfWork, commitChangesItem,
        discardChangesItem, eventCollection);
    }

    private void eventGrid_Load(object sender, System.EventArgs e) {
      ConfigureScheduler();
    }

    private Event GetFocusedEvent() {
      return (Event)eventGridView.GetFocusedRow();
    }

    private void ConfigureScheduler() {
      const int maxVisibleDays = 5;

      var ev = GetFocusedEvent();
      if (ev != null) {
        scheduler.Enabled = true;
        scheduler.LimitInterval.Start = ev.StartDate;
        scheduler.LimitInterval.End = ev.EndDate.AddDays(1);
        int days = ((int)(ev.EndDate - ev.StartDate).TotalDays) + 1;
        scheduler.DayView.DayCount = Math.Min(days, maxVisibleDays);
        if (days > maxVisibleDays) {
          dayScrollerPanel.Visible = true;
          dayScrollbar.Maximum = days - maxVisibleDays;
          dayScrollbar.Value = 0;
        }
        else
          dayScrollerPanel.Visible = false;

        scheduler.DayView.TopRowTime = new TimeSpan(
          ev.CapacityRequirements.Min(cr => cr.StartTime.TimeOfDay.Ticks)) -
          new TimeSpan(0, 30, 0);

        storage.Appointments.DataSource = ev.CapacityRequirements;
      }
      else {
        storage.Appointments.DataSource = null;
        scheduler.Enabled = false;
      }
    }

    void eventGridView_CurrentChanged(object sender, EventArgs e) {
      ConfigureScheduler();
    }

    private void dayScrollbar_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e) {
      scheduler.Start = scheduler.LimitInterval.Start.AddDays(e.NewValue);
    }

    private void scheduler_VisibleIntervalChanged(object sender, EventArgs e) {
      dayScrollbar.Value = scheduler.Start > scheduler.LimitInterval.Start ?
      (int)(scheduler.Start - scheduler.LimitInterval.Start).TotalDays : 0;
    }
  }
}