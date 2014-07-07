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
            protected set
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


        private IEnumerable<double> _data = new List<double>();
        public IEnumerable<double> Data
        {
            get { return _data; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Data list cannot be null.");

                this._data = value;
                this.OnPropertyChanged("Data");

                this.redraw();
            }
        }


        private ObservableCollection<Datum> _columnSetup = new ObservableCollection<Datum>();
        public ObservableCollection<Datum> ColumnSetup
        {
            get { return _columnSetup; }
            set
            {
                _columnSetup = value;
                this.OnPropertyChanged("ColumnSetup");
            }
        }


        private ObservableCollection<Tuple<double, double, Brush>> _rowGrid = new ObservableCollection<Tuple<double, double, Brush>>();
        public ObservableCollection<Tuple<double, double, Brush>> RowGrid
        {
            get { return _rowGrid; }
            protected set
            {
                this._rowGrid = value;
                this.OnPropertyChanged("RowGrid");
            }
        }


        private ObservableCollection<Double> _columnGrid = new ObservableCollection<Double>();
        public ObservableCollection<Double> ColumnGrid
        {
            get { return _columnGrid; }
            protected set
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
                    this.calculateRowGrid();
                }
            }
        }


        /// <summary>
        /// New empty statser
        /// </summary>
        public Statser()
        {
            InitializeComponent();
            this.DataContext = this;

            //Data.Add(new Datum() { OriginalValue = 10 });
            //Data.Add(new Datum() { OriginalValue = 15 });

            this.SizeChanged += Statser_SizeChanged;
        }


        /// <summary>
        /// The data that are graphed here have changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void data_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            calculateColumnHeights();
            
            // Row grid will be updated automatically if the MaxValue has changed
        }

        
        /// <summary>
        /// The UserControl has been resized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Statser_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            calculateColumnHeights();

            if (e.NewSize.Height != e.PreviousSize.Height)
                calculateRowGrid();
        }


        /// <summary>
        /// Calculate new maximum, normalise all columns and stretch to fill available height
        /// </summary>
        private void calculateColumnHeights()
        {
            MaxValue = (Data != null && Data.Count()>0)
                       ? Data.Max()
                       : 0;

            this.ColumnSetup = new ObservableCollection<Datum>(Data.Select(datum => new Datum(datum)));

            foreach (Datum datum in ColumnSetup)
            {
                double desiredHeight = (datum.OriginalValue / MaxValue) * ColumnContainer.ActualHeight;
                double desiredWidth = ColumnContainer.ActualWidth / ColumnSetup.Count;

                desiredWidth *= 0.95;

                // Only update when changing the actual value
                if (datum.ProjectedValue != desiredHeight)
                    datum.ProjectedValue = desiredHeight;
                if (datum.ProjectedValue2 != desiredWidth)
                    datum.ProjectedValue2 = desiredWidth;

            }
        }


        /// <summary>
        /// Run when the <code>MaxValue</code> changes
        /// </summary>
        private void calculateRowGrid()
        {
            MaxValue = (Data != null && Data.Count() > 0)
                       ? Data.Max()
                       : 0;

            int rows = (int) (MaxValue / Interval);
            double height = RowGridContainer.ActualHeight;
            double width = RowGridContainer.ActualWidth;

            this.RowGrid.Clear();
            for (int i=0 ; i<rows ; i++)
            {
                this.RowGrid.Add(new Tuple<double, double, Brush>(
                    height / rows,
                    width,
                    (i % 2 == 0) ? Resources["PhoneBackgroundBrush"] as Brush
                                 : Resources["PhoneChromeBrush"] as Brush
                ));
            }
        }


        /// <summary>
        /// Recalculates everything from the provided data list
        /// </summary>
        private void redraw()
        {
            calculateColumnHeights();
            calculateRowGrid();
        }


        /// <summary>
        /// Select the appropriate snap value and ceil to that value
        /// </summary>
        /// <param name="maxValue">The value to ceil</param>
        /// <returns>The ceiled value</returns>
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


        /// <summary>
        /// Get the smallest multiple of <code>step</code> that is greater than <code>value</code>
        /// </summary>
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
