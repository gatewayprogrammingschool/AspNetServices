using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MDS.Tests.Desktop.Converters;
public class BooleanToVisibilityConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language) => value switch
    {
        true => Visibility.Visible,
        false => Visibility.Collapsed,
        _ => null
    };

    public object ConvertBack(object value, Type targetType, object parameter, string language) 
        => throw new NotImplementedException();
}
