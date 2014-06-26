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

        private static class Keys
        {
            public static string SessionInterval = "SessionInterval";
        }

        public SettingsAccessor()
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



    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            InitializeComponent();
        }
    }
}