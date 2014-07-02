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
        private Brush _fill = new SolidColorBrush(Colors.Transparent);
        public Brush Fill
        {
            get { return _fill; }
            set
            {
                if (value != Fill)
                {
                    _fill = value;
                    OnPropertyChanged("Fill");
                }
            }
        }


        private double[] snapIntervals = { 5, 20, 50, 100, 1000, 5000, 10000 };
        private double _interval = 20;
        public double Interval
        {
            get { return _interval; }
            set
            {
                if (value != Interval)
                {
                    _interval = value;
                    OnPropertyChanged("Interval");
                }
            }
        }


        private string _title = "Title";
        public String Title
        {
            get { return _title; }
            set
            {
                if (value != Title)
                {
                    _title = value;
                    OnPropertyChanged("Title");
                }
            }
        }


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


        private ObservableCollection<Tuple<double, double, Brush>> _rowGrid = new ObservableCollection<Tuple<double, double, Brush>>();
        public ObservableCollection<Tuple<double, double, Brush>> RowGrid
        {
            get { return _rowGrid; }
            set
            {
                this._rowGrid = value;
                this.OnPropertyChanged("RowGrid");
            }
        }


        private ObservableCollection<Double> _columnGrid = new ObservableCollection<Double>();
        public ObservableCollection<Double> ColumnGrid
        {
            get { return _columnGrid; }
            set
            {
                this._columnGrid = value;
                this.OnPropertyChanged("ColumnGrid");
            }
        }


        private double _maxValue;
        public double MaxValue
        {
            get { return _maxValue; }
            protected set
            {
                value = CeilToMultiple(value);
                if (value != MaxValue)
                {
                    this._maxValue = value;
                    this.OnPropertyChanged("MaxValue");
                }
            }
        }


        public Statser()
        {
            Data.CollectionChanged += data_CollectionChanged;

            InitializeComponent();
            this.DataContext = this;

            Data.Add(new Datum() { OriginalValue = 10 });
            Data.Add(new Datum() { OriginalValue = 15 });

            this.SizeChanged += Statser_SizeChanged;
        }


        void data_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            calculateColumnHeights();
            calculateRowGrid();
        }

        

        void Statser_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            calculateColumnHeights();
            calculateRowGrid();
        }


        /// <summary>
        /// Calculate new maximum, normalise all columns and stretch to fill available height
        /// </summary>
        private void calculateColumnHeights()
        {
            MaxValue = Data.Max(x => x.OriginalValue);
            foreach (Datum datum in Data)
            {
                double desiredHeight = (datum.OriginalValue / MaxValue) * ColumnContainer.ActualHeight;
                double desiredWidth = ColumnContainer.ActualWidth / Data.Count;

                // Only update when changing the actual value
                if (datum.ProjectedValue != desiredHeight)
                    datum.ProjectedValue = desiredHeight;
                if (datum.ProjectedValue2 != desiredWidth)
                    datum.ProjectedValue2 = desiredWidth;

            }
        }


        private void calculateRowGrid()
        {
            MaxValue = Data.Max(x => x.OriginalValue);
            int rows = (int) (MaxValue / Interval);
            double height = RowGridContainer.ActualHeight;
            double width = RowGridContainer.ActualWidth;

            this.RowGrid.Clear();
            for (int i=0 ; i<rows ; i++)
            {
                this.RowGrid.Add(new Tuple<double, double, Brush>(
                    height / rows,
                    width,
                    (i % 2 == 0) ? new SolidColorBrush(Color.FromArgb(255, 25, 25, 25))
                                 : new SolidColorBrush(Color.FromArgb(255, 50, 50, 50))
                ));
            }
        }


        protected double CeilToMultiple(double maxValue)
        {
            double select = snapIntervals[0];
            foreach (double interval in snapIntervals)
            {
                select = interval;
                if (maxValue / interval < 6)
                    break;
            }
            this.Interval = select;
            return CeilToMultiple(maxValue, Interval);
        }


        protected static double CeilToMultiple(double value, double step)
        {
            if (value % step == 0)
                return value;

            else
                return value - (value % step) + step;
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
