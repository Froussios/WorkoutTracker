using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace WorkoutTracker
{
    /// <summary>
    /// Data context
    /// </summary>
    public class WorkoutTrackerDataContext : DataContext
    {
        // Pass the connection string to the base class.
        public WorkoutTrackerDataContext(string connectionString)
            : base(connectionString)
        {
            if (!this.DatabaseExists())
                this.CreateDatabase();
        }

        // Specify a table for the items.
        public Table<Entry> Entries;

        // Specify a table for the activities.
        public Table<Activity> Activities;
    }


    /// <summary>
    /// Represents a set of an activity,
    /// e.g. "20 pushups"
    /// </summary>
    [Table]
    public class Entry : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private int entryId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id
        {
            get { return entryId; }
            set
            {
                if (entryId != value)
                {
                    NotifyPropertyChanging("Id");
                    entryId = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }


        private int _count;

        [Column]
        public int Count
        {
            get { return _count; }
            set
            {
                if (_count != value)
                {
                    NotifyPropertyChanging("Count");
                    _count = value;
                    NotifyPropertyChanged("Count");
                }
            }
        }


        private long _date;
        [Column]
        public long DateTicks
        {
            get { return _date; }
            set
            {
                if (_date != value)
                {
                    NotifyPropertyChanging("Date");
                    _date = value;
                    NotifyPropertyChanged("Date");
                }
            }
        }
        public DateTime Date
        {
            get { return new DateTime(_date); }
            set
            {
                long v = value.Ticks;
                if (_date != v)
                {
                    NotifyPropertyChanging("Date");
                    _date = v;
                    NotifyPropertyChanged("Date");
                }
            }
        }

        // Internal column for the associated ToDoCategory ID value
        [Column]
        internal int _activityId;
        // Entity reference, to identify the ToDoCategory "storage" table
        private EntityRef<Activity> _activity;
        // Association
        [Association(Storage = "_activity", ThisKey = "_activityId", OtherKey = "Id", IsForeignKey = true)]
        public Activity Activity
        {
            get { return _activity.Entity; }
            set
            {
                NotifyPropertyChanging("Activity");
                _activity.Entity = value;

                if (value != null)
                {
                    _activityId = value.Id;
                }

                NotifyPropertyChanging("Activity");
            }
        }


        public String PresentationAll
        {
            get { return PresentationDate + ": " + PresentationCount + " " + PresentationActivity; }
        }
        public String PresentationDate
        {
            get { return Date.ToShortDateString(); }
        }
        public String PresentationCount
        {
            get { return Count.ToString(); }
        }
        public String PresentationActivity
        {
            get { return Activity.Name; }
        }

        public override string ToString()
        {
            return Count.ToString() + " " + Activity.Name;
        }

        public override bool Equals(object obj)
        {
            Entry other = obj as Entry;
            if (other != null)
                return this.Id == other.Id;
            return false;
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

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }


    /// <summary>
    /// A workout activity, which is part of workout program,
    /// e.g. pushups, pullups, etc.
    /// </summary>
    [Table]
    public class Activity : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private int _id;
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    NotifyPropertyChanging("Id");
                    _id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }


        private string _name;
        [Column]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    NotifyPropertyChanging("Name");
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private int _dailyGoal=0;
        [Column]
        public int DailyGoal
        {
            get { return _dailyGoal; }
            set
            {
                if (_dailyGoal != value)
                {
                    NotifyPropertyChanging("DailyGoal");
                    _dailyGoal = value;
                    NotifyPropertyChanged("DailyGoal");
                }
            }
        }

        private int _weeklyGoal=0;
        [Column]
        public int WeeklyGoal
        {
            get { return _weeklyGoal; }
            set
            {
                if (_weeklyGoal != value)
                {
                    NotifyPropertyChanging("WeeklyGoal");
                    _weeklyGoal = value;
                    NotifyPropertyChanged("WeeklyGoal");
                }
            }
        }

        public override string ToString()
        {
            return "(" + Id + ") " + Name;
        }


        /// <summary>
        /// Two activities are equal if they have the same name
        /// </summary>
        public override bool Equals(object obj)
        {
            Activity other = obj as Activity;
            if (other != null)
                return this.Name == other.Name;
            return false;
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

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }
}
