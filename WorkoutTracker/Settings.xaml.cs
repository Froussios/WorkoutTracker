using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace WorkoutTracker
{
    /// <summary>
    /// Accessor to application settings
    /// </summary>
    public class SettingsAccessor
    {
        public static SettingsAccessor Singleton = new SettingsAccessor();


        /// <summary>
        /// The names of the keys for the settings entries
        /// </summary>
        private static class Keys
        {
            public static string SessionInterval = "SessionInterval";
            public static string AmountShorthands = "AmountShorthands";
        }

        
        /// <summary>
        /// Creates a new settings view.
        /// Fields that are not set are initialised.
        /// </summary>
        public SettingsAccessor()
        {
            this.initialise(Keys.SessionInterval, 20);
            this.initialise(Keys.AmountShorthands, new int[] { 10, -5, -1 });
        }


        /// <summary>
        /// If the setting does not already exist, it is set to the value provided.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void initialise(String key, Object value)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(key))
                IsolatedStorageSettings.ApplicationSettings[key] = value;
        }


        /// <summary>
        /// The maximum amount of minutes between two entries, so that they are considered part of the same workout session.
        /// </summary>
        public int SessionInterval
        {
            get { return (int)IsolatedStorageSettings.ApplicationSettings[Keys.SessionInterval]; }
            set { IsolatedStorageSettings.ApplicationSettings[Keys.SessionInterval] = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        public ICollection<int> AmountShorthands
        {
            get { return (ICollection<int>)IsolatedStorageSettings.ApplicationSettings[Keys.AmountShorthands]; }
            set { IsolatedStorageSettings.ApplicationSettings[Keys.AmountShorthands] = value; }
        }
    }



    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            InitializeComponent();
        }
    }
}