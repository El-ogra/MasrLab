using System.Globalization;
using System.Windows.Data;

namespace MasrLab.Presentation.Resources.Converters;

public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue && parameter is string paramString)
        {
            return enumValue.ToString() == paramString;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter is string paramString && targetType.IsEnum)
        {
            return Enum.Parse(targetType, paramString);
        }
        return Binding.DoNothing;
    }
}
