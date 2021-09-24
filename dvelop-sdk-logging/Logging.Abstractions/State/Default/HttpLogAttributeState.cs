using System.Collections.Generic;
using System.Globalization;
using Dvelop.Sdk.Logging.Abstractions.State.Attribute;

namespace Dvelop.Sdk.Logging.Abstractions.State.Default
{
    public class HttpLogAttributeState : CustomLogAttributeState
    {
        /// <summary>
        /// HTTP request method.
        /// <example><c>GET</c>; <c>POST</c>; <c>HEAD</c></example>
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// <a href="https://tools.ietf.org/html/rfc7231#section-6">HTTP response status code</a>.
        /// <example><c>200</c></example>
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        /// Full HTTP request URL in the form <c>scheme://host[:port]/path?query[#fragment]</c>.
        /// Usually the fragment is not transmitted over HTTP, but if it is known, it should be included nevertheless.
        /// <example><c>https://www.foo.bar/search?q=OpenTelemetry#SemConv</c></example>
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// The full request target as passed in a HTTP request line or equivalent.
        /// <example><c>/path/12314/?q=ddds#123</c></example>
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// The value of the <a href="https://tools.ietf.org/html/rfc7230#section-5.4">HTTP host header</a>.
        /// An empty Host header should also be reported, see note.
        /// <example><c>www.example.org</c></example>
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// The URI scheme identifying the used protocol.
        /// <example><c>http</c>; <c>https</c></example>
        /// </summary>
        public string Scheme { get; set; }
        /// <summary>
        /// The matched route (path template).
        /// <example><c>/users/:userId</c></example>
        /// </summary>
        public string Route { get; set; }
        /// <summary>
        /// Value of the <a href="https://tools.ietf.org/html/rfc7231#section-5.5.3">HTTP User-Agent</a> header sent by the client.
        /// <example><c>CERN-LineMode/2.15 libwww/2.17b3</c></example>
        /// </summary>
        public string UserAgent { get; set; }
        /// <summary>
        /// The IP address of the original client behind all proxies, if known
        /// (e.g. from <a href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Forwarded-For">X-Forwarded-For</a>).
        /// <example><c>83.164.160.102</c></example>
        /// </summary>
        public string ClientIp { get; set; }
        /// <summary>
        /// Measures the duration of the inbound HTTP request in milliseconds.
        /// </summary>
        public long ServerDuration { get; set; }
        /// <summary>
        /// Measure the duration of the outbound HTTP request in milliseconds.
        /// </summary>
        public long ClientDuration { get; set; }

        public override IEnumerable<CustomLogAttribute> Attributes
        {
            get
            {
                var attributes = new List<CustomLogAttribute>();

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

                if (ServerDuration > 0)
                {
                    var server = new List<CustomLogAttribute>
                    {
                        new CustomLogAttributeProperty("duration", ServerDuration.ToString())
                    };
                    httpAttributes.Add(new CustomLogAttributeObject("server", server));
                }

                if (ClientDuration > 0)
                {
                    var client = new List<CustomLogAttribute>
                    {
                        new CustomLogAttributeProperty("duration", ClientDuration.ToString())
                    };
                    httpAttributes.Add(new CustomLogAttributeObject("client", client));
                }

                attributes.Add(new CustomLogAttributeObject("http", httpAttributes));


                return attributes;
            }
        }
    }
}
