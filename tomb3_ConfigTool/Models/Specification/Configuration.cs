using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using tomb3_ConfigTool.Utils;

namespace tomb3_ConfigTool.Models;

public class Configuration
{
    private static readonly string _regPath = @"Software\Core Design\Tomb Raider III\tomb3";

    private static readonly string _specificationPath = "Resources.specification.json";
    private static readonly JsonSerializerSettings _serializerSettings = new()
    {
        Converters = new NumericConverter[] { new NumericConverter() },
        Formatting = Formatting.Indented
    };

    private readonly Specification _specification;
    private JObject _activeData;

    public IReadOnlyList<Category> Categories
    {
        get => _specification.CategorisedProperties;
    }

    public IReadOnlyList<BaseProperty> Properties
    {
        get => _specification.Properties;
    }

    public Configuration()
    {
        using Stream stream = AssemblyUtils.GetResourceStream(_specificationPath);
        using StreamReader reader = new(stream);
        _specification = new Specification(reader.ReadToEnd());
        RestoreDefaults();
    }

    public void RestoreDefaults()
    {
        foreach (BaseProperty property in Properties)
        {
            property.SetToDefault();
        }
    }

    public bool IsDataDirty()
    {
        if (_activeData != null)
        {
            JObject convertedData = GetConvertedData();
            if (convertedData.Count != _activeData.Count)
            {
                return true;
            }

            foreach (KeyValuePair<string, JToken> pair in convertedData)
            {
                if (!_activeData.ContainsKey(pair.Key) || !_activeData[pair.Key].Equals(pair.Value))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsDataDefault()
    {
        foreach (BaseProperty property in Properties)
        {
            if (!property.IsDefault)
            {
                return false;
            }
        }

        return true;
    }

    public void Read()
    {
        JObject activeData = new();
        using RegistryKey reg = Registry.CurrentUser.OpenSubKey(_regPath);
        List<string> values = reg == null
            ? new()
            : reg.GetValueNames().ToList();

        foreach (BaseProperty property in Properties)
        {
            if (values.Contains(property.Field))
            {
                property.LoadValue(reg.GetValue(property.Field).ToString());
            }
            else
            {
                property.SetToDefault();
            }

            activeData[property.Field] = JToken.FromObject(property.ExportValue());
        }

        _activeData = activeData;
    }

    public void Write()
    {
        JObject data = GetConvertedData();
        JObject newActiveData = new(data);

        using RegistryKey reg = Registry.CurrentUser.CreateSubKey(_regPath, true);
        foreach (BaseProperty property in Properties)
        {
            object value = property.ExportValue();
            RegistryValueKind valueKind = RegistryValueKind.String;
            switch (property.DataType)
            {
                case DataType.Bool:
                    value = (bool)value ? 1 : 0;
                    valueKind = RegistryValueKind.DWord;
                    break;
                case DataType.Enum:
                    if (int.TryParse(value.ToString(), out int _))
                    {
                        valueKind = RegistryValueKind.DWord;
                    }
                    break;
            }

            reg.SetValue(property.Field, value, valueKind);
        }

        _activeData = newActiveData;
    }

    public void Import(string jsonPath)
    {
        JObject externalData = JObject.Parse(File.ReadAllText(jsonPath));

        foreach (BaseProperty property in Properties)
        {
            if (externalData.ContainsKey(property.Field))
            {
                property.LoadValue(externalData[property.Field].ToString());
                externalData.Remove(property.Field);
            }

            _activeData[property.Field] = JToken.FromObject(property.ExportValue());
        }
    }

    public void Export(string jsonPath)
    {
        File.WriteAllText(jsonPath, JsonConvert.SerializeObject(GetConvertedData(), _serializerSettings));
    }

    private JObject GetConvertedData()
    {
        JObject data = new();
        foreach (BaseProperty property in Properties)
        {
            data[property.Field] = JToken.FromObject(property.ExportValue());
        }
        return data;
    }
}
