using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dvelop.Sdk.Logging.OtelJsonConsole.Extension
{
    public static class LoggerRegistrationExtensions
    {
        public static ILoggingBuilder AddOtelJsonConsole(this ILoggingBuilder builder, Action<OtelJsonConsoleFormatterOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddOtelJsonConsole();
            builder.Services.Configure(configure);

            return builder;
        }

        public static ILoggingBuilder AddOtelJsonConsole(this ILoggingBuilder builder)
        {
            builder.AddConsoleFormatter<OtelJsonConsoleFormatter, OtelJsonConsoleFormatterOptions>();
            builder.AddConsole(options => options.FormatterName = "oteljson");

            return builder;
        }
    }
}
