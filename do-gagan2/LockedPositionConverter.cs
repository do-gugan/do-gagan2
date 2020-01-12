using System;
using System.Windows.Data;

namespace do_gagan2
{
    public class LockedPositionConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var totalSec = (double)value;
            int mm = (int)totalSec / 60;
            int ss = (int)totalSec - (mm * 60);
            return (string)String.Format("{0:000}", mm) + ":" + String.Format("{0:00}", ss);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)0.0;
        }
    }
}
