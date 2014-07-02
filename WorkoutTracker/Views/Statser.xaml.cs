using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using System.ComponentModel;

namespace WorkoutTracker
{
    public class Datum : INotifyPropertyChanged
    {
        private double _originalValue;
        private double _projectedValue;
        private double _projectedValue2;


        public Datum() { }
        public Datum(double originalValue)
        {
            this.OriginalValue = originalValue;
        }


        /// <summary>
        /// The original value this datum represents
        /// </summary>
        public double OriginalValue 
        {
            get { return this._originalValue; }
            set
            {
                if (value != OriginalValue)
                {
                    this._originalValue = value;
                    this.OnPropertyChanged("OriginalValue");
                }
            }
        }

        /// <summary>
        /// The value to be used, after normalising, scaling etc.
        /// </summary>
        public double ProjectedValue
        {
            get { return this._projectedValue; }
            set
            {
                if (value != ProjectedValue)
                {
                    this._projectedValue = value;
                    this.OnPropertyChanged("ProjectedValue");
                }
            }
        }

        /// <summary>
        /// A secondary value to be used, after normalising, scaling etc.
        /// </summary>
        public double ProjectedValue2
        {
            get { return this._projectedValue2; }
            set
            {
                if (value != ProjectedValue2)
                {
                    this._projectedValue2 = value;
                    this.OnPropertyChanged("ProjectedValue2");
                }
            }
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
    }



    public partial class Statser : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<Datum> _data = new ObservableCollection<Datum>();
        public ObservableCollection<Datum> Data
        {
            get { return _data; }
            set
            {
                this._data = value;
                this.OnPropertyChanged("Data");
            }
        }


        public Statser()
        {
            Data.CollectionChanged += data_CollectionChanged;

            InitializeComponent();
            this.DataContext = this;

            //Data.Add(new Datum() { OriginalValue = 10 });
            //Data.Add(new Datum() { OriginalValue = 15 });

            this.SizeChanged += Statser_SizeChanged;
        }


        void data_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CalculateColumnHeights();
        }

        

        void Statser_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateColumnHeights();
        }


        /// <summary>
        /// Calculate new maximum, normalise all columns and stretch to fill available height
        /// </summary>
        private void CalculateColumnHeights()
        {
            double max = Data.Max(x => x.OriginalValue);
            foreach (Datum datum in Data)
            {
                double desiredHeight = (datum.OriginalValue / max) * ColumnContainer.ActualHeight;
                double desiredWidth = ColumnContainer.ActualWidth / Data.Count;

                // Only update when changing the actual value
                if (datum.ProjectedValue != desiredHeight)
                    datum.ProjectedValue = desiredHeight;
                if (datum.ProjectedValue2 != desiredWidth)
                    datum.ProjectedValue2 = desiredWidth;

            }
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
    }

}
