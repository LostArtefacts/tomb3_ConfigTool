using System;
using System.Globalization;
using tomb3_ConfigTool.Models;

namespace tomb3_ConfigTool.Utils;

public class ConditionalViewTextConverter : ConditionalMarkupConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Language.Instance.Controls[base.Convert(value, targetType, parameter, culture).ToString()];
    }
}
