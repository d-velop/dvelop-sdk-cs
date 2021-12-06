using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dvelop.Sdk.Dashboard.Dto.Registration.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NavigationType
    {
        [EnumMember(Value = "outerSupply")]
        OuterSupply,

        [EnumMember(Value = "dapiNavigate")]
        DapiNavigate,
        
        [EnumMember(Value = "innerSupply")]
        InnerSupply
    }
}
