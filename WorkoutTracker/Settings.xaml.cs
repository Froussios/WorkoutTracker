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
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace WorkoutTracker
{
    /// <summary>
    /// Accessor to application settings
    /// </summary>
    public class SettingsAccessor : INotifyPropertyChanged
    { 
        public static SettingsAccessor _singleton = new SettingsAccessor();
        public static SettingsAccessor Default
        {
            get { return (SettingsAccessor) App.Current.Resources["Settings"]; }
        }


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
            this.initialise(Keys.AmountShorthands, new ObservableCollection<AmountShorthand>(new AmountShorthand[] 
            { 
                new AmountShorthand{Amount="+10"},
                new AmountShorthand{Amount="-5"},
                new AmountShorthand{Amount="-1"},
            }));
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
            set 
            { 
                IsolatedStorageSettings.ApplicationSettings[Keys.SessionInterval] = value;
                this.NotifyPropertyChanged("SessionInterval");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<AmountShorthand> AmountShorthands
        {
            get { return (ObservableCollection<AmountShorthand>)IsolatedStorageSettings.ApplicationSettings[Keys.AmountShorthands]; }
            set 
            { 
                IsolatedStorageSettings.ApplicationSettings[Keys.AmountShorthands] = value;
                this.NotifyPropertyChanged("AmountShorthands");
            }
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


    public class AmountShorthand : INotifyPropertyChanged
    {
        private string _amount = "0";
        public string Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                NotifyPropertyChanged("Amount");
            }
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



    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            InitializeComponent();
        }


        private void TextBox_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            ObservableCollection<AmountShorthand> shorthands = new ObservableCollection<AmountShorthand>();
            foreach (var item in ShorthandsControl.Items)
            {
                TextBox tb = (TextBox)ShorthandsControl.ItemContainerGenerator.ContainerFromItem(item);
                shorthands.Add(new AmountShorthand { Amount = tb.Text });
            }

            SettingsAccessor.Default.AmountShorthands = shorthands;
        }
    }
}