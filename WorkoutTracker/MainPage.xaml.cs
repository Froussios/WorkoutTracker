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
using System.ComponentModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WorkoutTracker.Resources;

namespace WorkoutTracker
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        static ViewModel Persistense = App.ViewModel;
        ObservableCollection<Entry> justNowCollection = new ObservableCollection<Entry>();
        public ObservableCollection<Entry> JustNowCollection { get { return justNowCollection; } }


        private UInt16 _activityAmount;
        /// <summary>
        /// The activity amount value from the form
        /// </summary>
        public UInt16 ActivityAmount
        {
            get { return _activityAmount; }
            set
            {
                _activityAmount = value;
                OnPropertyChanged("ActivityAmount");
            }
        }


        private bool _graphsDirty = true;
        private bool GraphsDirty
        {
            get { return this._graphsDirty; }
            set { this._graphsDirty = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;

            App.ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }


        void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ViewModel.PropertyNames.EntriesLastMonth)
                // Mark graphs for redrawing
                GraphsDirty = true;
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }


        /// <summary>
        /// Add a new entry.
        /// Will read the amount for this entry from the GUI.
        /// </summary>
        /// <param name="activity">The activity type of the entry</param>
        public void AddEntry(Activity activity)
        {
            int count;
            try
            {
                count = Int16.Parse(ActivityCount.Text);
                //count = ActivityAmount; // ActivityAmount gets updated after AddEntry is called
            }
            catch (Exception) { return; }

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


        /// <summary>
        /// Add a new entry. The amount is taken from the GUI
        /// </summary>
        /// <param name="sender">The UI element that contains the activity as its DataContext</param>
        /// <param name="e"></param>
        void SelectActivityButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Button button = sender as Button;
            Activity activity = button.DataContext as Activity;
            AddEntry(activity);
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            //CreateGraphs();
        }


        /// <summary>
        /// Statically declared indexing keys
        /// </summary>
        static class GraphData 
        {
            public static string DailyTotal = "DailyTotal"; 
            public static string DailyTimes = "DailyTimes";
            public static string DailySessions = "DailySessions";
            public static string DailySessionMax = "DailySessionMax";
        };


        /// <summary>
        /// Calculate and draw graphs for the last month
        /// </summary>
        [Obsolete]
        private void CreateGraphs()
        {
            ChartStackPanel.Children.Clear();
            ChartStackPanel.Children.Add(new TextBlock() 
            {
                Text = "Crunching numbers...",
                Margin = new Thickness(0, 40, 0, 0),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            });

            Dispatcher.BeginInvoke(new Action(() => 
            {
                List<UIElement> graphs = new List<UIElement>();

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
                    Sparrow.Chart.LineSeries lseries = new Sparrow.Chart.LineSeries 
                    {
                        StrokeThickness = 3, Stroke = Resources["PhoneAccentBrush"] as System.Windows.Media.Brush 
                    };
                    foreach (KeyValuePair<int, int> kv in data[GraphData.DailyTotal])
                    {
                        Sparrow.Chart.ChartPoint point = new Sparrow.Chart.DoublePoint { Data = kv.Key, Value = kv.Value };
                        aseries.Points.Add(point);
                        lseries.Points.Add(point);
                    }
                    // Graph displaying average set
                    Sparrow.Chart.SplineSeries seriesAverage = new Sparrow.Chart.SplineSeries 
                    {
                        Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange) 
                    };
                    foreach (KeyValuePair<int, int> kv in data[GraphData.DailyTimes])
                    {
                        int t = data[GraphData.DailyTotal][kv.Key];
                        int c = kv.Value;
                        int value = (c > 0) ? t / c : 0;
                        Sparrow.Chart.ChartPoint point = new Sparrow.Chart.DoublePoint { Data = kv.Key, Value = value };
                        seriesAverage.Points.Add(point);
                    }
                    // Graph display max session
                    Sparrow.Chart.SplineSeries maxSessionSeries = new Sparrow.Chart.SplineSeries 
                    {
                        Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Magenta) 
                    };
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
                    //chart.Series.Add(seriesAverage);
                    //chart.Series.Add(maxSessionSeries);
                    sp.Children.Add(chart);

                    graphs.Add(sp);
                }

                // Show graphs
                ChartStackPanel.Children.Clear();
                foreach (UIElement graph in graphs) 
                {
                    ChartStackPanel.Children.Add(graph);
                }
            }));
            
        }

        private void createGraphs2()
        {
            ChartStackPanel.Children.Clear();

            // Create graphs for every activity
            foreach (Activity activity in App.ViewModel.AllActivities)
            {
                // Analyse entries
                IEnumerable<Entry> entries = App.ViewModel.GetEntries(activity);
                Dictionary<string, Dictionary<int, int>> data;
                data = AnalyseData(entries);

                // Graph displaying total count
                List<double> list = new List<double>();
                foreach (KeyValuePair<int, int> kv in data[GraphData.DailyTotal].OrderBy(x => x.Key))
                {
                    list.Add(kv.Value);
                }
                Statser statser = new Statser()
                {
                    Title = activity.Name,
                    Data = new ObservableCollection<Datum>(list.Select(x => new Datum(x))),
                    Height=150,
                    Margin = new Thickness(0, 30, 0, 0),
                };
                ChartStackPanel.Children.Add(statser);
            }

            GraphsDirty = false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
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
                int sessionIntervalMinutes = SettingsAccessor.Singleton.SessionInterval;
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
            //total[1] = total[0];
            //entriesCount[1] = entriesCount[0];
            //sessionsCount[1] = sessionsCount[0];
            //sessionMax[1] = sessionMax[0];

            data = new Dictionary<string, Dictionary<int, int>>();
            data[GraphData.DailyTotal] = total;
            data[GraphData.DailyTimes] = entriesCount;
            data[GraphData.DailySessions] = sessionsCount;
            data[GraphData.DailySessionMax] = sessionMax;

            return data;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Popup.IsOpen = false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            createGraphs2();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNewActivityButton_Click(object sender, RoutedEventArgs e)
        {
            String name = NewActivityNameTextBox.Text;
            if (name != "")
            {
                App.ViewModel.AddActivity(new Activity { Name = name });
            }
        }


        /// <summary>
        /// View settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplicationBarSettings_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }


        /// <summary>
        /// Edit activity amount with shorthands
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AmountOperation_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Button button = sender as Button;
            int amount = int.Parse(button.Content as String);
            int current = (int) ActivityAmount;
            //int current = 0;
            //int.TryParse(ActivityCount.Text, out current);

            //ActivityCount.Text = Math.Max(current + amount, 0).ToString();
            ActivityAmount = (ushort) Math.Max(current + amount, 0);
        }


        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot pivot = sender as Pivot;
            PivotItem item = pivot.SelectedItem as PivotItem;

            if (item.Equals(this.PivotItemGraphs))
                if (GraphsDirty)
                    this.createGraphs2();
        }



        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Create the OnPropertyChanged method to raise the event 
        /// </summary>
        /// <param name="name"></param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion



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