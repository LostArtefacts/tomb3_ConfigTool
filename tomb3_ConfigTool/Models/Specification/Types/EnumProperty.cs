using System.Collections.Generic;

namespace tomb3_ConfigTool.Models;

public class EnumProperty : BaseProperty
{
    public override DataType DataType => DataType.Enum;

    public string EnumKey { get; set; }
    
    private EnumOption _value;

    public EnumOption Value
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

    public string DefaultValue { get; set; }

    public override bool IsDefault
    {
        get => Value.ID == DefaultValue;
    }

    public List<EnumOption> Options { get; set; }

    public override object ExportValue()
    {
        return Value.ID;
    }

    public override void LoadValue(string value)
    {
        if (Options.Find(o => o.ID == value) is EnumOption option)
        {
            Value = option;
        }
    }

    public override void SetToDefault()
    {
        Value = Options.Find(o => o.ID == DefaultValue);
    }

    public override void Initialise(Specification specification)
    {
        if (specification.Enums.ContainsKey(EnumKey))
        {
            Options = specification.Enums[EnumKey];
        }
        base.Initialise(specification);
    }
}
