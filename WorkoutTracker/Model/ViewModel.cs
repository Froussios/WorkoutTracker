﻿using System;
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

            // Add dummies
            if (this.GetActivity("Pushups") == null)
            {
                this.AddActivity(new Activity { Name = "Pushups" });
                this.AddActivity(new Activity { Name = "Situps" });

                Activity a = this.GetActivity("Pushups");
                this.AddEntry(new Entry { Count = 20, Activity = a, Date = DateTime.Now });
                this.AddEntry(new Entry { Count = 15, Activity = a, Date = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)) });
                this.AddEntry(new Entry { Count = 15, Activity = a, Date = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)) });
                this.AddEntry(new Entry { Count = 10, Activity = a, Date = DateTime.Now.Subtract(new TimeSpan(3, 0, 0, 0)) });
            }
        }


        // LINQ to SQL data context for the local database.
        private WorkoutTrackerDataContext DB = null;

        public WorkoutTrackerDataContext DataContext
        {
            get
            {
                if (DB != null)
                    return DB;
                return DB = new WorkoutTrackerDataContext("isostore:/MyDatabase.sdf");
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
        public ObservableCollection<Entry> AllEntries
        {
            get { return _allEntries; }
            set
            {
                _allEntries = value;
                NotifyPropertyChanged("AllEntries");
            }
        }


        private ObservableCollection<Activity> _allActivities;
        public ObservableCollection<Activity> AllActivities
        {
            get { return _allActivities; }
            set
            {
                _allActivities = value;
                NotifyPropertyChanged("AllActivities");
            }
        }



        /// <summary>
        /// Commit all changes to the database
        /// </summary>
        public void SaveChangesToDB()
        {
            DB.SubmitChanges();
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



        public IEnumerable<Entry> GetEntries(String activityName)
        {
            return from Entry entry in this.AllEntries
                           where entry.Activity.Name == activityName
                           select entry;
        }

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
            //Contract.Requires<ArgumentNullException>(category != null);

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

                NotifyPropertyChanged("AllActivities");
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

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify Silverlight that a property has changed.
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