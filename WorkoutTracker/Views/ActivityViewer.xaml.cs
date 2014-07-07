using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WorkoutTracker
{
    public partial class ActivityViewer : PhoneApplicationPage, INotifyPropertyChanged
    {
        public Activity DataContext
        {
            get { return base.DataContext as Activity; }
            set
            {
                base.DataContext = value;
                NotifyPropertyChanged("DailyTotals");
                NotifyPropertyChanged("DailyAverages");
                NotifyPropertyChanged("DailyMaximums");
            }
        }

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
                if (entries.Count() > 0)
                {
                    HistoryEntriesList.ItemsSource = entries;
                    TotalTextBlock.Text = entries.Sum(x => x.Count).ToString();
                    AverageSetTextBlock.Text = entries.Average(x => x.Count).ToString();
                    MaximumSetTextBlock.Text = entries.Max(x => x.Count).ToString();
                }

                AverageGraph.Data = this.DailyAverages;
                MaximumGraph.Data = this.DailyMaxima;
                TotalGraph.Data = this.DailyTotals;
            }
        }

        private static DateTime Today 
        {
            get { return DateTime.Today; }
        }
        private static DateTime LastMonth
        {
            get { return DateTime.Today.AddMonths(-1); }
        }



        public class MyGrouping<TKey, TElement> : List<TElement>, IGrouping<TKey, TElement>
        {
            public TKey Key { get; set; }
        }

        public IEnumerable<IGrouping<DateTime, Entry>> MonthDummies =
            Enumerable.Range(0, 1 + Today.Subtract(LastMonth).Days)
                .Select(offset => LastMonth.AddDays(offset))
                .Select(date => new MyGrouping<DateTime, Entry> {Key = date})
                .ToArray();

        public class C : IEqualityComparer<IGrouping<DateTime, Entry>>
        {
            public bool Equals(IGrouping<DateTime, Entry> x, IGrouping<DateTime, Entry> y)
            {
                return x.Key.Equals(y.Key);
            }

            public int GetHashCode(IGrouping<DateTime, Entry> obj)
            {
                return obj.Key.GetHashCode();
            }
        }

        public IEnumerable<double> DailyTotals
        {
            get
            {
                return App.ViewModel.EntriesLastMonth
                                    .Where(entry => entry.Activity.Equals(DataContext))
                                    .GroupBy(entry => entry.Date.Date)
                                    .Union(MonthDummies, new C())
                                    .OrderBy(group => group.Key)
                                    .Select(group => (double)(group.Count() > 0 ? group.Sum(entry => entry.Count) : 0));
            }
        }


        public IEnumerable<double> DailyAverages
        {
            get
            {
                return App.ViewModel.EntriesLastMonth
                                    .Where(entry => entry.Activity.Equals(DataContext))
                                    .GroupBy(entry => entry.Date.Date)
                                    .Union(MonthDummies, new C())
                                    .OrderBy(group => group.Key)
                                    .Select(group => group.Count() > 0 ? group.Average(entry => entry.Count) : 0);
            }
        }


        public IEnumerable<double> DailyMaxima
        {
            get
            {
                return App.ViewModel.EntriesLastMonth
                                    .Where(entry => entry.Activity.Equals(DataContext))
                                    .GroupBy(entry => entry.Date.Date)
                                    .Union(MonthDummies, new C())
                                    .OrderBy(group => group.Key)
                                    .Select(group => (double)(group.Count() > 0 ? group.Max(entry => entry.Count) : 0));
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


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}