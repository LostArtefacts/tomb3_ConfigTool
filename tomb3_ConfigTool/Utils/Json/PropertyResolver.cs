using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using tomb3_ConfigTool.Models;

namespace tomb3_ConfigTool.Utils;

public class PropertyResolver : DefaultContractResolver
{
    protected override JsonConverter ResolveContractConverter(Type objectType)
    {
        return typeof(BaseProperty).IsAssignableFrom(objectType)
            ? null
            : base.ResolveContractConverter(objectType);
    }
}
