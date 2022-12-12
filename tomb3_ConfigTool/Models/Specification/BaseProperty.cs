using System.Collections.Generic;

namespace tomb3_ConfigTool.Models;

public abstract class BaseProperty : BaseNotifyPropertyChanged
{
    public abstract DataType DataType { get; }
    public string Field { get; set; }
    public Dependency Dependency { get; set; }

    private bool _isAvailable;
    public bool IsAvailable
    {
        get => _isAvailable;
        set
        {
            if (_isAvailable != value)
            {
                _isAvailable = value;
                NotifyPropertyChanged();
            }
        }
    }

    public string Title
    {
        get => Language.Instance.Properties[Field].Title;
    }

    public string Description
    {
        get => Language.Instance.Properties[Field].Description;
    }

    public BaseProperty()
    {
        IsAvailable = true;
    }

    public abstract object ExportValue();
    public abstract void LoadValue(string value);
    public abstract void SetToDefault();
    public abstract bool IsDefault { get; }

    public virtual void Initialise(Specification specification)
    {
        SetToDefault();
    }
}
