using System.Collections.Generic;
using Dvelop.Sdk.Logging.Abstractions.State;
using Dvelop.Sdk.Logging.Abstractions.State.Attribute;

namespace Dvelop.Sdk.Logging.Abstractions.Scope
{
    public class IncomingHttpRequestLogScope : CustomLogAttributeState
    {
        public string Method { get; set; }
        public string Target { get; set; }
        public string UserAgent { get; set; }
        public int? Status { get; set; }
        public long? Elapsed { get; set; }

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
                
                if (!string.IsNullOrEmpty(UserAgent))
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("userAgent", UserAgent));
                }

                if (Status != 0)
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("status", Status));
                }

                if (Elapsed != 0)
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("elapsed", Elapsed));
                }

                httpAttributes.Add(new CustomLogAttributeProperty("direction", "inbound"));
                attributes.Add(new CustomLogAttributeObject("http", httpAttributes));
                return attributes;
            }
        }
    }
}