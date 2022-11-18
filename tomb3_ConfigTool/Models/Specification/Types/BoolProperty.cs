namespace tomb3_ConfigTool.Models;

public class BoolProperty : BaseProperty
{
    public override DataType DataType => DataType.Bool;

    private bool _value;

    public bool Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                NotifyPropertyChanged();
            }
        }
    }

    public bool DefaultValue { get; set; }

    public bool Flip { get; set; }

    public override bool IsDefault
    {
        get => Value == DefaultValue;
    }

    public override object ExportValue()
    {
        return FlipValue(Value);
    }

    private bool FlipValue(bool value)
    {
        return Flip ? !value : value;
    }

    public override void LoadValue(string value)
    {
        if (value == "0")
        {
            Value = FlipValue(false);
        }
        else if (value == "1")
        {
            Value = FlipValue(true);
        }
        else if (bool.TryParse(value, out bool val))
        {
            Value = FlipValue(val);
        }
    }

    public override void SetToDefault()
    {
        Value = DefaultValue;
    }
}
