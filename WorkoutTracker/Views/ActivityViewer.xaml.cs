using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WorkoutTracker
{
    public partial class ActivityViewer : PhoneApplicationPage
    {
        public ActivityViewer()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string parameter = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("activityId", out parameter))
            {
                long activityId = Int64.Parse(parameter);
                Activity activity = App.ViewModel.GetActivity(activityId);
                this.DataContext = activity;

                IEnumerable<Entry> entries = App.ViewModel.GetEntries(activity);
                if (entries.Count(x => true) > 0)
                {
                    HistoryEntriesList.ItemsSource = entries;
                    TotalTextBlock.Text = entries.Sum(x => x.Count).ToString();
                    AverageSetTextBlock.Text = entries.Average(x => x.Count).ToString();
                }
            }
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            Activity activity = DataContext as Activity;
            if (App.ViewModel.GetEntries(activity).Count(x => true) > 0)
            {
                PopupMessage.Text = "Cannot delete while entries of this activity exist";
                Popup.IsOpen = true;
            }
            else
            {
                App.ViewModel.DeleteActivity(DataContext as Activity);
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }

        private void Button_Tap(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = false;
        }

        private void EntryItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Grid senderGrid = sender as Grid;
            Entry entry = senderGrid.DataContext as Entry;
            if (entry != null)
            {
                NavigationService.Navigate(new Uri("/Views/EntryViewer.xaml?entryId=" + entry.Id, UriKind.Relative));
            }
            else
            {
                PopupMessage.Text = "null entry :(";
                Popup.IsOpen = true;
            }
        }

        public void SaveChanges()
        {
            App.ViewModel.SaveChangesToDB();
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SaveChanges();
        }
    }
}