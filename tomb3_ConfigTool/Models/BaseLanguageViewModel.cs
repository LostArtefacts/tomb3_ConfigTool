using System.Collections.Generic;

namespace tomb3_ConfigTool.Models;

public class BaseLanguageViewModel : BaseNotifyPropertyChanged
{
    public static Dictionary<string, string> ViewText
    {
        get => Language.Instance.Controls;
    }
}
