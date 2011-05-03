using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.ComponentModel;

namespace VisualBF {
    public partial class BoxControl : UserControl {
        private static SolidColorBrush black = new SolidColorBrush(Colors.Black);
        private static SolidColorBrush trans = new SolidColorBrush(Colors.Transparent);

        private bool showBorder;
        public bool ShowBorder {
            get { return showBorder; }
            set {
                showBorder = value;
                if (showBorder) {
                    boxBorder.BorderBrush = black;
                    boxBorder.BorderThickness = new Thickness(1);
                }
                else {
                    boxBorder.BorderBrush = trans;
                    boxBorder.BorderThickness = new Thickness(0);
                }
            }
        }

        public bool ShowIndex {
            get { return indexTextBlock.Visibility == System.Windows.Visibility.Visible; }
            set { indexTextBlock.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        public BoxData Data { get { return DataContext as BoxData; } }

        public BoxControl() {
            InitializeComponent();
            DataContext = new BoxData();
            ShowBorder = false;
        }

        public BoxControl(int index, string value, bool isActive, bool showBorder = true)
            : this() {
            Data.Index = index;
            Data.Value = value;
            Data.IsActive = isActive;
            ShowBorder = showBorder;
        }
    }

    public class BoxData : INotifyPropertyChanged {
        int index;
        public int Index {
            get {
                return index;
            }
            set {
                index = value;
                OnPropertyChanged("Index");
            }
        }

        private string val;
        public string Value {
            get { return val; }
            set {
                val = value;
                OnPropertyChanged("Value");
            }
        }

        private bool isActive;
        public bool IsActive {
            get { return isActive; }
            set {
                isActive = value;
                OnPropertyChanged("IsActive");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string p) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }
        #endregion
    }

    public class IsActiveConverter : IValueConverter {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            bool isActive = (bool)value;
            if (isActive)
                return new SolidColorBrush(Colors.Green);
            else return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return null;
        }

        #endregion
    }

}
