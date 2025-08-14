using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace TicTacToe
{
    // Удобный Boolean -> текст для ToggleButton
    public class BooleanToTextConverter : MarkupExtension, IValueConverter
    {
        public string TrueText { get; set; } = "On";
        public string FalseText { get; set; } = "Off";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value as bool? == true) ? TrueText : FalseText;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    // Если Highlight == true -> вернуть "win", иначе null
    public class HighlightTagConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 0 && values[0] is bool b && b) return "win";
            return null!;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => Array.Empty<object>();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
