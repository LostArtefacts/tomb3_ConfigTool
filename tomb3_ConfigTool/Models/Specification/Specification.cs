using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using tomb3_ConfigTool.Utils;

namespace tomb3_ConfigTool.Models;

public class Specification
{
    public Dictionary<string, List<EnumOption>> Enums { get; private set; }
    public List<Category> CategorisedProperties { get; private set; }
    public List<BaseProperty> Properties { get; private set; }

    public Specification(string sourceData)
    {
        JObject data = JObject.Parse(sourceData);
        JObject enumData = data[nameof(Enums)].ToObject<JObject>();
        Enums = new();

        foreach (KeyValuePair<string, JToken> enumType in enumData)
        {
            List<string> enumValues = enumType.Value.ToObject<List<string>>();
            IEnumerable<EnumOption> options = enumValues.Select(val => new EnumOption
            {
                EnumName = enumType.Key,
                ID = val
            });
            Enums[enumType.Key] = options.ToList();
        }

        string categoryData = data[nameof(CategorisedProperties)].ToString();
        PropertyConverter converter = new();
        CategorisedProperties = JsonConvert.DeserializeObject<List<Category>>(categoryData, converter);
        Properties = new();

        InitialiseProperties();
    }

    private void InitialiseProperties()
    {
        foreach (Category category in CategorisedProperties)
        {
            foreach (BaseProperty property in category.Properties)
            {
                property.Initialise(this);
                Properties.Add(property);
            }
        }

        foreach (BaseProperty property in Properties)
        {
            if (property.Dependency == null)
            {
                continue;
            }
            
            if (Properties.Find(p => p.Field == property.Dependency.Field) is BaseProperty otherProperty)
            {
                otherProperty.PropertyChanged += (o, e) =>
                {
                    TestDependency(property, otherProperty);
                };
                TestDependency(property, otherProperty);
            }
        }
    }

    private void TestDependency(BaseProperty property1, BaseProperty property2)
    {
        property1.IsAvailable = property2.ExportValue().Equals(property1.Dependency.Value);
    }
}
