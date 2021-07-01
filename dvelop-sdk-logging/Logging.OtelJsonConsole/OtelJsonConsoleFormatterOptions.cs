using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Logging.Console;

namespace Dvelop.Sdk.Logging.OtelJsonConsole
{
    public class OtelJsonConsoleFormatterOptions : ConsoleFormatterOptions
    {
        public JsonWriterOptions JsonWriterOptions;

        public OtelJsonConsoleFormatterOptions()
        {
            UseUtcTimestamp = true;
            TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'";
            JsonWriterOptions = new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }
    }
}
