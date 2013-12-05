using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace _4tecture.UI.Common.Converters
{
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public sealed class BoolToColorBrushConverter : IValueConverter
    {
        
        public Color TrueColor { get; set; }
        public Color FalseColor { get; set; }

        public bool InvertValue { get; set; }

        public BoolToColorBrushConverter()
        {
            // set defaults
            TrueColor = (Color)ColorConverter.ConvertFromString("Green");
            FalseColor = (Color)ColorConverter.ConvertFromString("Red");
        }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return null;

            var val = InvertValue ? !(bool)value : (bool)value;
            return (bool)val ? new SolidColorBrush(TrueColor) : new SolidColorBrush(FalseColor);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush)
            {
                if (Equals(((SolidColorBrush)value).Color, TrueColor))
                    return InvertValue ? false : true;
                if (Equals(((SolidColorBrush)value).Color, FalseColor))
                    return InvertValue ? true : false;
            }
            return null;
        }
    }
}
