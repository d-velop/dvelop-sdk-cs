using System.Collections.Generic;
using Dvelop.Sdk.Logging.Abstractions.State.Attribute;

namespace Dvelop.Sdk.Logging.Abstractions.State
{
    public class OutgoingHttpRequestLogState : CustomLogAttributeState
    {
        public string Method { get; set; }
        public string Target { get; set; }
        public int? StatusCode { get; set; }
        public long? ClientDuration { get; set; }

        public override IEnumerable<CustomLogAttribute> Attributes
        {
            get
            {
                var attributes = new List<CustomLogAttribute>();
                var httpAttributes = new List<CustomLogAttribute>();
                if (!string.IsNullOrEmpty(Method))
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("method", Method));
                }

                if (!string.IsNullOrEmpty(Target))
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("target", Target));
                }

                if (StatusCode.HasValue)
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("statusCode", StatusCode.Value));
                }

                if (ClientDuration.HasValue)
                {
                    httpAttributes.Add(new CustomLogAttributeObject("client", new List<CustomLogAttribute>{
                        new CustomLogAttributeProperty("duration", ClientDuration.Value)
                    }));
                }

                httpAttributes.Add(new CustomLogAttributeProperty("direction", "outbound"));
                attributes.Add(new CustomLogAttributeObject("http", httpAttributes));
                return attributes;
            }
        }

        
    }
}