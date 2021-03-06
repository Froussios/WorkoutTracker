﻿using System;
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
    public partial class EntryViewer : PhoneApplicationPage
    {
        public new Entry DataContext
        {
            get { return base.DataContext as Entry; }
            set { base.DataContext = value; }
        }

        public EntryViewer()
        {
            InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            popup.IsOpen = false;
            string parameter = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("entryId", out parameter))
            {
                long entryId = Int64.Parse(parameter);
                Entry entry = App.ViewModel.GetEntry(entryId);
                if (entry != null)
                    this.DataContext = entry;
                else
                    popup.IsOpen = true;
            }
            else
            {
                popup.IsOpen = true;
                VisualStateManager.GoToState(this, "EntryDoesNotExist", true);
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


        private void Delete_Click(object sender, EventArgs e)
        {
            App.ViewModel.DeleteEntry(DataContext as Entry);
            //NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            VisualStateManager.GoToState(this, "EntryDoesNotExist", true);
            //popup.IsOpen = true;
        }


        private void ApplicationBarBack_Click(object sender, EventArgs e)
        {
            this.NavigationService.GoBack();
        }


        private void ApplicationBarSave_Click(object sender, EventArgs e)
        {
            this.SaveChanges();
            this.NavigationService.GoBack();
        }

        
    }
}