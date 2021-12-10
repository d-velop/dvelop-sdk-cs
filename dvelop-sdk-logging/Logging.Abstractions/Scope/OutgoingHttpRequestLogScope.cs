using System.Collections.Generic;
using Dvelop.Sdk.Logging.Abstractions.State;
using Dvelop.Sdk.Logging.Abstractions.State.Attribute;

namespace Dvelop.Sdk.Logging.Abstractions.Scope
{
    public class OutgoingHttpRequestLogScope : CustomLogAttributeState
    {
        public string Method { get; set; }
        public string Target { get; set; }
        public int StatusCode { get; set; }
        public long ClientDuration { get; set; }

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

                if (StatusCode != 0)
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("statusCode", StatusCode));
                }

                if (ClientDuration != 0)
                {
                    httpAttributes.Add(new CustomLogAttributeObject("client", new List<CustomLogAttribute>{
                        new CustomLogAttributeProperty("duration", ClientDuration)
                    }));
                }

                httpAttributes.Add(new CustomLogAttributeProperty("direction", "outbound"));
                attributes.Add(new CustomLogAttributeObject("http", httpAttributes));
                return attributes;
            }
        }

        
    }
}