using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dvelop.Sdk.Dashboard.Dto.Response.Types
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ImageSize
    {
        [EnumMember(Value = null)]
        None,
        
        [EnumMember(Value = "icon")]
        Icon,
        
        [EnumMember(Value = "avatar")]
        Avatar, 
        
        [EnumMember(Value = "medium")]
        Medium,
        
        [EnumMember(Value = "large")]
        Large
    }
}