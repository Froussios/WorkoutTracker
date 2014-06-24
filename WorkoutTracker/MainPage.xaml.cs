using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WorkoutTracker.Resources;

namespace WorkoutTracker
{
    public class Settings
    {
        public static Settings Singleton = new Settings();

        private static class Keys
        {
            public static string SessionInterval = "SessionInterval";
        }

        public Settings()
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(Keys.SessionInterval))
                IsolatedStorageSettings.ApplicationSettings[Keys.SessionInterval] = 20;
        }

        public int SessionInterval
        {
            get { return (int)IsolatedStorageSettings.ApplicationSettings[Keys.SessionInterval]; }
            set { IsolatedStorageSettings.ApplicationSettings[Keys.SessionInterval] = value; }
        }
    }

    public partial class MainPage : PhoneApplicationPage
    {
        class SelectActivityButton : Button
        {
            public delegate void AddEntryDelegate(Activity activity);

            AddEntryDelegate del;
            private AddEntryDelegate AddEntryMethod
            {
                get { return del; }
                set { del = value; }
            }

            Activity activity;

            public SelectActivityButton(Activity inActivity, AddEntryDelegate addDel)
            {
                this.activity = inActivity;
                this.Content = activity.Name;
                this.AddEntryMethod = addDel;
                this.Tap += SelectActivityButton_Tap;
            }

            void SelectActivityButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
            {
                AddEntryMethod(activity);
            }
        }

        static ViewModel Persistense = App.ViewModel;
        ObservableCollection<Entry> justNowCollection = new ObservableCollection<Entry>();
        public ObservableCollection<Entry> JustNowCollection { get { return justNowCollection; } }

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;

            //ActivitiesList.ItemsSource = Persistense.AllActivities;
            //HistoryEntriesList.ItemsSource = Persistense.AllEntries;

            foreach (Activity activity in Persistense.AllActivities)
            {
                ActivitySelectorPanel.Children.Add(new SelectActivityButton(activity, AddEntry));
            }

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        public void AddEntry(Activity activity)
        {
            int count;
            try
            {
                count = Int16.Parse(ActivityCount.Text);
            }
            catch (Exception e) { return; }

            if (count > 0)
            {
                Entry newEntry = new Entry
                { 
                    Count = count,
                    Activity = activity,
                    Date = DateTime.Now
                };

                Persistense.AddEntry(newEntry);
                justNowCollection.Add(newEntry);
            }
        }

        
        private void PivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            //CreateGraphs();
        }


        static class GraphData 
        {
            public static string DailyTotal = "DailyTotal"; 
            public static string DailyTimes = "DailyTimes";
            public static string DailySessions = "DailySessions";
            public static string DailySessionMax = "DailySessionMax";
        };
        private void CreateGraphs()
        {
            ChartStackPanel.Children.Clear();

            // Create graphs for every activity
            foreach (Activity activity in App.ViewModel.AllActivities)
            {
                // Analyse entries
                IEnumerable<Entry> entries = App.ViewModel.GetEntries(activity);
                Dictionary<string, Dictionary<int, int>> data;

                data = AnalyseData(entries);


                // Presentation
                StackPanel sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

                // Title
                TextBlock tb = new TextBlock { Text = activity.Name };
                sp.Children.Add(tb);

                // Graph displaying total count
                System.Windows.Media.Brush areaFillBrush = Resources["PhoneAccentBrush"] as System.Windows.Media.Brush;
                areaFillBrush.Opacity = 0.55;
                Sparrow.Chart.SeriesBase aseries = new Sparrow.Chart.AreaSeries { Stroke = areaFillBrush, Fill = areaFillBrush };
                Sparrow.Chart.LineSeries lseries = new Sparrow.Chart.LineSeries { StrokeThickness = 3, Stroke = Resources["PhoneAccentBrush"] as System.Windows.Media.Brush };
                foreach (KeyValuePair<int, int> kv in data[GraphData.DailyTotal])
                {
                    Sparrow.Chart.ChartPoint point = new Sparrow.Chart.DoublePoint { Data = kv.Key, Value = kv.Value };
                    aseries.Points.Add(point);
                    lseries.Points.Add(point);
                }
                // Graph displaying average set
                Sparrow.Chart.SplineSeries sseries = new Sparrow.Chart.SplineSeries { Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange) };
                foreach (KeyValuePair<int, int> kv in data[GraphData.DailyTimes])
                {
                    int t = data[GraphData.DailyTotal][kv.Key];
                    int c = kv.Value;
                    int value = (c > 0) ? t / c : 0;
                    Sparrow.Chart.ChartPoint point = new Sparrow.Chart.DoublePoint { Data = kv.Key, Value = value };
                    sseries.Points.Add(point);
                }
                // Graph display max session
                Sparrow.Chart.SplineSeries maxSessionSeries = new Sparrow.Chart.SplineSeries { Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Magenta) };
                foreach (KeyValuePair<int, int> kv in data[GraphData.DailySessionMax])
                {
                    Sparrow.Chart.ChartPoint point = new Sparrow.Chart.DoublePoint { Data = kv.Key, Value = kv.Value };
                    maxSessionSeries.Points.Add(point);
                }

                // The chart that all the graphs appear in
                Sparrow.Chart.SparrowChart chart = new Sparrow.Chart.SparrowChart
                {
                    Height = 200,
                    XAxis = new Sparrow.Chart.LinearXAxis { Interval = 7 },
                    YAxis = new Sparrow.Chart.LinearYAxis { Interval = 10 }
                };
                chart.Series.Add(aseries);
                chart.Series.Add(lseries);
                chart.Series.Add(sseries);
                chart.Series.Add(maxSessionSeries);
                sp.Children.Add(chart);

                ChartStackPanel.Children.Add(sp);
            }
        }



        private static Dictionary<string, Dictionary<int, int>> AnalyseData(IEnumerable<Entry> entries)
        {
            Dictionary<string, Dictionary<int, int>> data;

            // Analyse per day

            Dictionary<int, int> total = new Dictionary<int, int>();
            Dictionary<int, int> entriesCount = new Dictionary<int, int>();
            Dictionary<int, int> sessionsCount = new Dictionary<int, int>();
            Dictionary<int, int> sessionMax = new Dictionary<int, int>();

            // Filter to last month
            DateTime graphStart = DateTime.Today.Subtract(new TimeSpan(31, 0, 0, 0));
            entries = entries.Where(x => x.Date.Ticks >= graphStart.Ticks).OrderBy(x => x.Date);
            // Group by day
            IEnumerable<IGrouping<DateTime, Entry>> groupByDay = entries.GroupBy(x => x.Date.Date);

            // Initialize to zero
            for (DateTime i = new DateTime(graphStart.Ticks); !i.Date.Equals(DateTime.Today); i = i.AddDays(1))
            {
                int d = -DateTime.Today.Subtract(i).Days;
                total[d] = 0;
                entriesCount[d] = 0;
                sessionsCount[d] = 0;
                sessionMax[d] = 0;
            }
            total[0] = 0;
            entriesCount[0] = 0;
            sessionsCount[0] = 0;
            sessionMax[0] = 0;

            // Construct values for graph
            foreach (IGrouping<DateTime, Entry> group in groupByDay)
            {
                int day = -DateTime.Today.Subtract(group.Key.Date).Days;

                // Find maximum session
                int sessionIntervalMinutes = Settings.Singleton.SessionInterval;
                TimeSpan sessionInterval = new TimeSpan(0, sessionIntervalMinutes, 0);
                int max = 0;
                int currentSession = 0;
                int sessionCount = 0;
                Entry previousEntry = null;
                foreach (Entry entry in group.OrderBy(x => x.Date))
                {
                    if (previousEntry == null)
                    {
                        currentSession = entry.Count;
                        sessionCount++;
                    }
                    else
                        if (entry.Date.Subtract(previousEntry.Date).CompareTo(sessionInterval) > 0)
                        {
                            currentSession = 0;
                            sessionCount++;
                        }
                        else
                            currentSession += entry.Count;

                    previousEntry = entry;
                    if (max < currentSession) max = currentSession;
                }

                entriesCount[day] = group.Count();
                total[day] = group.Sum(x => x.Count);
                sessionMax[day] = max;
                sessionsCount[day] = sessionCount;
            }

            // Duplicate last entry for better visivility
            total[1] = total[0];
            entriesCount[1] = entriesCount[0];
            sessionsCount[1] = sessionsCount[0];
            sessionMax[1] = sessionMax[0];

            data = new Dictionary<string, Dictionary<int, int>>();
            data[GraphData.DailyTotal] = total;
            data[GraphData.DailyTimes] = entriesCount;
            data[GraphData.DailySessions] = sessionsCount;
            data[GraphData.DailySessionMax] = sessionMax;

            return data;
        }



        private void ActivityItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Grid senderGrid = sender as Grid;
            Activity activity = senderGrid.DataContext as Activity;
            if (activity != null)
            {
                NavigationService.Navigate(new Uri("/ActivityViewer.xaml?activityId=" + activity.Id, UriKind.Relative));
            }
            else
            {
                PopupMessage.Text = "null activity :(";
                Popup.IsOpen = true;
            }
        }



        private void EntryItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Grid senderGrid = sender as Grid;
            Entry entry = senderGrid.DataContext as Entry;
            if (entry != null)
            {
                NavigationService.Navigate(new Uri("/EntryViewer.xaml?entryId=" + entry.Id, UriKind.Relative));
            }
            else
            {
                PopupMessage.Text = "null entry :(";
                Popup.IsOpen = true;
            }
        }

        private void Button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Popup.IsOpen = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreateGraphs();
        }

        private void AddNewActivityButton_Click(object sender, RoutedEventArgs e)
        {
            String name = NewActivityNameTextBox.Text;
            if (name != "")
            {
                App.ViewModel.AddActivity(new Activity { Name = name });
            }
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}