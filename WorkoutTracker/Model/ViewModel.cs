using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics.Contracts;

namespace WorkoutTracker
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ViewModel()
        {
            this.LoadData();

            //// Add dummies
            //if (this.GetActivity("Pushups") == null)
            //{
            //    this.AddActivity(new Activity { Name = "Pushups" });
            //    this.AddActivity(new Activity { Name = "Situps" });

            //    Activity a = this.GetActivity("Pushups");
            //    this.AddEntry(new Entry { Count = 20, Activity = a, Date = DateTime.Now });
            //    this.AddEntry(new Entry { Count = 15, Activity = a, Date = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)) });
            //    this.AddEntry(new Entry { Count = 15, Activity = a, Date = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)) });
            //    this.AddEntry(new Entry { Count = 10, Activity = a, Date = DateTime.Now.Subtract(new TimeSpan(3, 0, 0, 0)) });
            //}

            // Add welcome dummy
            if (this._allActivities.Count == 0)
            {
                this.AddActivity(new Activity { Name = "Being Awesome"});

                Activity awesome = this.GetActivity("Being Awesome");
                this.AddEntry(new Entry { Count = 1, Activity = awesome, Date = DateTime.Now});
            }
        }


        public static class PropertyNames
        {
            public static String AllEntries = "AllEntries";
            public static String AllActivities = "AllActivities";
            public static String EntriesLastMonth = "EntriesLastMonth";
            public static String EntriesToday = "EntriesToday";
            public static String EntriesBeforeToday = "EntriesBeforeToday";
            public static String TotalsToday = "TotalsToday";
            
        }
        private Dictionary<String, ICollection<String>> viewAliases = new Dictionary<string, ICollection<String>>() 
        {
            {
                PropertyNames.AllEntries, new String[] 
                {
                    PropertyNames.EntriesLastMonth,
                    PropertyNames.EntriesToday,
                    PropertyNames.EntriesBeforeToday,
                    PropertyNames.TotalsToday,
                }
            }
        };


        // LINQ to SQL data context for the local database.
        private WorkoutTrackerDataContext _DB = null;

        public WorkoutTrackerDataContext DataContext
        {
            get
            {
                if (_DB != null)
                    return _DB;
                return _DB = new WorkoutTrackerDataContext("isostore:/MyDatabase.sdf");
            }
        }



        public bool IsDataLoaded
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            // Sample data; replace with real data
            LoadEntries();
            LoadActivities();

            this.IsDataLoaded = true;
        }



        private ObservableCollection<Entry> _allEntries;
        /// <summary>
        /// Get all the entries
        /// </summary>
        public ObservableCollection<Entry> AllEntries
        {
            get { return _allEntries; }
            set
            {
                _allEntries = value;
                NotifyPropertyChanged(PropertyNames.AllEntries);
            }
        }


        /// <summary>
        /// Get a view of AllEntries that includes only views from the last 30 days
        /// </summary>
        public IEnumerable<Entry> EntriesLastMonth
        {
            get 
            {
                DateTime lastMonth = DateTime.Now.Date.Subtract(new TimeSpan(30, 0, 0 ,0));
                return AllEntries.Where(entry => entry.Date.Ticks >= lastMonth.Ticks);
            }
        }


        /// <summary>
        /// Get a view of AllEntries that includes only views from the last day
        /// </summary>
        public IEnumerable<Entry> EntriesToday
        {
            get
            {
                DateTime today = DateTime.Now.Date.Subtract(new TimeSpan(12, 0, 0));
                return AllEntries.Where(entry => entry.Date.Date.Ticks >= today.Ticks);
            }
        }


        /// <summary>
        /// Get last month's entries, excluding today
        /// </summary>
        public IEnumerable<Entry> EntriesBeforeToday
        {
            get
            {
                DateTime today = DateTime.Now.Date.Subtract(new TimeSpan(12, 0, 0));
                DateTime lastMonth = DateTime.Now.Date.Subtract(new TimeSpan(30, 0, 0, 0));
                return AllEntries.Where(entry => entry.Date.Date.Ticks <= today.Ticks &&
                                                 entry.Date.Date.Ticks >= lastMonth.Ticks);
            }
        }


        public IEnumerable<Tuple<Activity, int, int>> TotalsToday
        {
            get
            {
                return EntriesToday.GroupBy(x => x.Activity)
                                   .OrderBy(group => group.Key.Name)
                                   .Select(group => new Tuple<Activity, int, int>
                                   (
                                       group.Key,
                                       group.Sum(entry => entry.Count),
                                       group.Count()
                                   ));

            }
        }


        private ObservableCollection<Activity> _allActivities;
        /// <summary>
        /// View of all the activities
        /// </summary>
        public ObservableCollection<Activity> AllActivities
        {
            get { return _allActivities; }
            set
            {
                _allActivities = value;
                NotifyPropertyChanged(PropertyNames.AllActivities);
            }
        }


        /// <summary>
        /// Commit all changes to the database
        /// </summary>
        public void SaveChangesToDB()
        {
            this.DataContext.SubmitChanges();

            this.NotifyPropertyChanged("AllEntries");
            this.NotifyPropertyChanged("AllActivities");
        }


        /// <summary>
        /// Loads all entries from the database into memory
        /// </summary>
        public void LoadEntries()
        {
            var entriesInDB = from Entry entry in this.DataContext.Entries
                              select entry;
            AllEntries = new ObservableCollection<Entry>(entriesInDB);
            AllEntries = new ObservableCollection<Entry>(AllEntries.OrderByDescending(x => x.Date));
        }


        /// <summary>
        /// Loads all activities from the database into memory
        /// </summary>
        public void LoadActivities()
        {
            var activitiesInDB = from Activity entry in this.DataContext.Activities
                                select entry;
            AllActivities = new ObservableCollection<Activity>(activitiesInDB);
        }


        /// <summary>
        /// Get all the entries to an activity
        /// </summary>
        /// <param name="activityName">The name of the activity</param>
        /// <returns></returns>
        public IEnumerable<Entry> GetEntries(String activityName)
        {
            return from Entry entry in this.AllEntries
                           where entry.Activity.Name == activityName
                           select entry;
        }


        /// <summary>
        /// Get all the entries to an activity
        /// </summary>
        /// <param name="activity">The activity</param>
        /// <returns></returns>
        public IEnumerable<Entry> GetEntries(Activity activity)
        {
            return from Entry entry in this.AllEntries
                   where entry.Activity.Id == activity.Id
                   select entry;
        }


        /// <summary>
        /// Get the entry with the specified ID
        /// </summary>
        /// <param name="id">The entry's ID</param>
        /// <returns></returns>
        public Entry GetEntry(long id)
        {
            return AllEntries.First(x => x.Id == id);
        }


        /// <summary>
        /// Store a new entry
        /// </summary>
        /// <param name="inEntry">The new entry</param>
        public void AddEntry(Entry inEntry)
        {
            this.DataContext.Entries.InsertOnSubmit(inEntry);
            this.DataContext.SubmitChanges();
            this.LoadEntries();

            NotifyPropertyChanged("AllEntries");
        }


        /// <summary>
        /// Remove entry from database
        /// </summary>
        /// <param name="entry">The entry to be removed</param>
        public void DeleteEntry(Entry entry)
        {
            //Contract.Requires<ArgumentNullException>(entry != null);

            if (entry != null)
            {
                this.DataContext.Entries.DeleteOnSubmit(entry);
                this.DataContext.SubmitChanges();
                this.LoadEntries();
            }
        }



        /// <summary>
        /// Get the activity by id
        /// </summary>
        /// <param name="id">The activity's id</param>
        /// <returns>The activity</returns>
        public Activity GetActivity(long id)
        {
            var activities = from Activity activity in AllActivities
                             where activity.Id == id
                             select activity;

            if (activities.Count() == 0)
                return null;
            else
                return activities.First();
        }


        /// <summary>
        /// Get an activity of that name. Note that there can multiple activities with the same. Only one is returned.
        /// </summary>
        /// <param name="name">The name of the activity</param>
        /// <returns>The activity instance</returns>
        public Activity GetActivity(String name)
        {
            //Contract.Requires<ArgumentNullException>(name != null);

            var activities = from Activity activity in AllActivities
                             where activity.Name == name
                             select activity;

            if (activities.Count() == 0)
                return null;
            else
                return activities.First();
        }


        /// <summary>
        /// Get the  activity of the specified and name and category, creating of necessary
        /// </summary>
        /// <param name="name">The name of the activity</param>
        /// <returns>The activity</returns>
        public Activity GetOrCreateActivity(String name)
        {
            //Contract.Requires<ArgumentNullException>(name != null);

            var activities = from Activity activity in AllActivities
                             where activity.Name == name
                             select activity;

            if (activities.Count() == 0)
            {
                this.AddActivity(new Activity { Name = name });
                return GetActivity(name);
            }
            else
                return activities.First();
        }


        /// <summary>
        /// Creates a new actvity.
        /// If an activity with the same name exists, the action will be ignored.
        /// </summary>
        /// <param name="inActivity">The activity to be created</param>
        public void AddActivity(Activity inActivity)
        {
            //Contract.Requires<ArgumentNullException>(inActivity != null);

            // Search if category already exists
            var activities = from Activity activity in AllActivities
                             where activity.Name == inActivity.Name
                             select activity;

            // Insert if category doesn't already exist
            if (activities.Count() == 0)
            {
                this.DataContext.Activities.InsertOnSubmit(inActivity);
                this.DataContext.SubmitChanges();
                this.LoadActivities();

                NotifyPropertyChanged(PropertyNames.AllActivities);
            }
        }


        /// <summary>
        /// Remove activity from database
        /// </summary>
        /// <param name="activity">The activity to be removed</param>
        public void DeleteActivity(Activity activity)
        {
            //Contract.Requires<ArgumentNullException>(activity != null);

            this.DataContext.Activities.DeleteOnSubmit(activity);
            this.DataContext.SubmitChanges();
            this.LoadActivities();
        }




        #region INotifyPropertyChanged Members


        /// <summary>
        /// Implementation of INotifyPropertyChanged 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify Silverlight that a property has changed.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                
                if (this.viewAliases.ContainsKey(propertyName))
                    foreach (String propertyAlias in this.viewAliases[propertyName])
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(propertyAlias));
                    }
            }
        }


        #endregion
    }
}
