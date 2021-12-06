using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dvelop.Sdk.Dashboard.Dto.Response.Types
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LineVariant
    {
        [EnumMember(Value = null)]
        OneLine,

        [EnumMember(Value = "twoline")]
        TwoLine
    }
}