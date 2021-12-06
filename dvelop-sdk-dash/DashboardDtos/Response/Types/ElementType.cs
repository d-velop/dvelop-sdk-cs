using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dvelop.Sdk.Dashboard.Dto.Response.Types
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ElementType
    {
        [EnumMember(Value = null)]
        None,

        [EnumMember(Value = "image")]
        Image,

        [EnumMember(Value = "icon")]
        Icon,

        [EnumMember(Value = "caption")]
        Caption
    }
}