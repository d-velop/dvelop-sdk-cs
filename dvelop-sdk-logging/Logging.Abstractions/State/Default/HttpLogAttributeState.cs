using System.Collections.Generic;
using System.Globalization;
using Dvelop.Sdk.Logging.Abstractions.State.Attribute;

namespace Dvelop.Sdk.Logging.Abstractions.State.Default
{
    public class HttpLogAttributeState : CustomLogAttributeState
    {
        public string Method { get; set; }
        public int StatusCode { get; set; }
        public string Url { get; set; }
        public string Target { get; set; }
        public string Host { get; set; }
        public string Scheme { get; set; }
        public string Route { get; set; }
        public string UserAgent { get; set; }
        public string ClientIp { get; set; }
        public double TimeUsed { get; set; }

        public override IEnumerable<CustomLogAttribute> Attributes
        {
            get
            {
                var attributes = new List<CustomLogAttribute>();

                if (TimeUsed > 0)
                {
                    attributes.Add(new CustomLogAttributeProperty("timeUsed", TimeUsed.ToString(CultureInfo.InvariantCulture)));
                }

                var httpAttributes = new List<CustomLogAttribute>();

                if (!string.IsNullOrWhiteSpace(Method))
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("method", Method));
                }
                if (StatusCode > 0)
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("statusCode", StatusCode));
                }
                if (!string.IsNullOrWhiteSpace(Url))
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("url", Url));
                }
                if (!string.IsNullOrWhiteSpace(Target))
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("target", Target));
                }
                if (!string.IsNullOrWhiteSpace(Host))
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("host", Host));
                }
                if (!string.IsNullOrWhiteSpace(Scheme))
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("scheme", Scheme));
                }
                if (!string.IsNullOrWhiteSpace(Route))
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("route", Route));
                }
                if (!string.IsNullOrWhiteSpace(UserAgent))
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("userAgent", UserAgent));
                }
                if (!string.IsNullOrWhiteSpace(ClientIp))
                {
                    httpAttributes.Add(new CustomLogAttributeProperty("clientIP", ClientIp));
                }

                attributes.Add(new CustomLogAttributeObject("http", httpAttributes));


                return attributes;
            }
        }
    }
}
