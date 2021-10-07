using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Dvelop.Sdk.Logging.Abstractions.Resource;
using Dvelop.Sdk.Logging.Abstractions.Scope;
using Dvelop.Sdk.Logging.Abstractions.State;
using Dvelop.Sdk.Logging.OtelJsonConsole.Scope;
using Dvelop.Sdk.Logging.OtelJsonConsole.State;
using Dvelop.Sdk.Logging.OtelJsonConsole.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Dvelop.Sdk.Logging.OtelJsonConsole
{
    public class OtelJsonConsoleFormatter : ConsoleFormatter, IDisposable
    {
        private readonly IResourceDescriptor _resourceDescriptor;
        private readonly IDisposable _optionsReloadToken;

        private OtelJsonConsoleFormatterOptions _formatterOptions;

        public OtelJsonConsoleFormatter(IOptionsMonitor<OtelJsonConsoleFormatterOptions> options, IResourceDescriptor resourceDescriptor) : base("oteljson")
        {
            ReloadLoggerOptions(options.CurrentValue);
            _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
            _resourceDescriptor = resourceDescriptor;
        }

        public OtelJsonConsoleFormatter(IOptionsMonitor<OtelJsonConsoleFormatterOptions> options) : this(options, null)
        {
        }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
        {
            string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            if (logEntry.Exception == null && message == null)
            {
                return;
            }

            Exception exception = logEntry.Exception;
            const int defaultBufferSize = 1024;
            using (var output = new PooledByteBufferWriter(defaultBufferSize))
            {
                using (var writer = new Utf8JsonWriter(output, _formatterOptions.JsonWriterOptions))
                {
                    writer.WriteStartObject();

                    WriteTime(writer);
                    WriteSeverity(writer, logEntry.LogLevel);
                    WriteName(writer, logEntry.EventId.Name);
                    WriteBody(writer, message);

                    var scopeInfo = GetScopeInformation(writer, scopeProvider);

                    WriteTenant(writer, scopeInfo.TenantLogScope?.TenantId);
                    WriteTracing(writer, scopeInfo.TracingLogScope?.Trace, scopeInfo.TracingLogScope?.Span);

                    WriteResource(writer);
                    WriteAttributes(writer, scopeInfo, exception, logEntry);
                    WriteVisibility(writer, scopeInfo.Visible);

                    writer.WriteEndObject();
                    writer.Flush();
                }

                textWriter.Write(Encoding.UTF8.GetString(output.WrittenMemory.Span));

            }
            textWriter.Write(Environment.NewLine);
        }

        private void WriteAttributes<TState>(Utf8JsonWriter writer, ScopeInfo scopeInfo, Exception exception, LogEntry<TState> logEntry)
        {
            if (!ShouldWriteAttributesSection(scopeInfo, exception, logEntry.State))
            {
                return;
            }

            writer.WriteStartObject("attr");
            WriteCustomAttributes(writer, scopeInfo.CustomAttributes);
            WriteScopes(writer, scopeInfo.Scopes);
            WriteException(writer, exception);
            WriteState(writer, logEntry.State);
            writer.WriteEndObject();
        }

        private void WriteTime(Utf8JsonWriter writer)
        {
            var timestampFormat = _formatterOptions.TimestampFormat;
            if (timestampFormat != null)
            {
                DateTimeOffset dateTimeOffset = _formatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
                writer.WriteString("time", dateTimeOffset.ToString(timestampFormat));
            }
        }

        private void WriteSeverity(Utf8JsonWriter writer, LogLevel logLevel)
        {
            writer.WriteNumber("sev", GetLogLevelSeverity(logLevel));
        }

        private void WriteName(Utf8JsonWriter writer, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            writer.WriteString("name", name);
        }

        private void WriteBody(Utf8JsonWriter writer, string message)
        {
            writer.WriteString("body", message);
        }

        private void WriteTenant(Utf8JsonWriter writer, string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return;
            }

            writer.WriteString("tn", tenantId);
        }

        private void WriteTracing(Utf8JsonWriter writer, string traceId, string spanId)
        {
            if (string.IsNullOrWhiteSpace(traceId))
            {
                return;
            }

            writer.WriteString("trace", traceId);
            writer.WriteString("span", spanId);
        }

        private void WriteResource(Utf8JsonWriter writer)
        {
            var resource = _resourceDescriptor?.GetResourceInfo();

            if (resource == null)
            {
                return;
            }

            writer.WriteStartObject("res");

            if (resource.Service != null)
            {
                writer.WriteStartObject("svc");
                writer.WriteString("name", resource.Service.Name);
                writer.WriteString("ver", resource.Service.Version);
                writer.WriteString("inst", resource.Service.Instance);
                writer.WriteEndObject();
            }

            if (resource.Host != null)
            {
                writer.WriteStartObject("host");
                writer.WriteString("name", resource.Host.Hostname);
                writer.WriteEndObject();
            }

            if (resource.Process != null)
            {
                writer.WriteStartObject("process");
                writer.WriteNumber("pid", resource.Process.Pid);
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }

        private void WriteVisibility(Utf8JsonWriter writer, bool visible)
        {
            writer.WriteNumber("vis", visible ? 1 : 0);
        }

        private void WriteException(Utf8JsonWriter writer, Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            var exceptionWithStacktrace = exception.ToString();
            if (!_formatterOptions.JsonWriterOptions.Indented)
            {
                exceptionWithStacktrace = exceptionWithStacktrace.Replace(Environment.NewLine, " ");
            }

            writer.WriteStartObject("exception");
            writer.WriteString("type", exception.GetType().ToString());
            writer.WriteString("message", exception.Message);
            writer.WriteString("stacktrace", exceptionWithStacktrace);
            writer.WriteEndObject();
        }

        private void WriteCustomAttributes(Utf8JsonWriter writer, IList<ICustomAttributesLogScope> customAttributes)
        {
            if (!customAttributes?.Any() ?? true)
            {
                return;
            }

            foreach (ICustomAttributesLogScope customAttribute in customAttributes)
            {
                writer.WriteStartObject(customAttribute.Name);
                foreach (KeyValuePair<string, object> item in customAttribute.Items)
                {
                    writer.WriteItem(item);
                }
                writer.WriteEndObject();
            }
        }

        private void WriteScopes(Utf8JsonWriter writer, IList<object> scopes)
        {
            if (!scopes?.Any() ?? true)
            {
                return;
            }

            writer.WriteStartArray("scopes");

            foreach (object scope in scopes)
            {
                writer.WriteStringValue(Utf8JsonWriterUtils.ToInvariantString(scope));
            }

            writer.WriteEndArray();
        }

        private void WriteState<TState>(Utf8JsonWriter writer, TState logEntryState)
        {
            if (logEntryState == null)
            {
                return;
            }

            if (logEntryState is CustomLogAttributeState customLogAttributeState)
            {
                customLogAttributeState.Render(new JsonCustomStateRenderer(writer));
            }
        }

        private ScopeInfo GetScopeInformation(Utf8JsonWriter writer, IExternalScopeProvider scopeProvider)
        {
            var scopeInfo = new ScopeInfo
            {
                CustomAttributes = new List<ICustomAttributesLogScope>(),
                Scopes = new List<object>(),
                Visible = true
            };

            scopeProvider?.ForEachScope((scope, state) =>
            {
                switch (scope)
                {
                    case TenantLogScope requestScope:
                        scopeInfo.TenantLogScope = requestScope;
                        break;
                    case TracingLogScope tracingLogScope:
                        scopeInfo.TracingLogScope = tracingLogScope;
                        break;
                    case InvisibilityLogScope _:
                        scopeInfo.Visible = false;
                        break;
                    case ICustomAttributesLogScope customAttributesScope:
                        scopeInfo.CustomAttributes.Add(customAttributesScope);
                        break;
                }

                if (_formatterOptions.IncludeScopes)
                {
                    scopeInfo.Scopes.Add(scope);
                }
            }, writer);

            return scopeInfo;
        }

        private bool ShouldWriteAttributesSection<TState>(ScopeInfo scopeInfo, Exception exception, TState state)
        {
            var customAttributesFromScopeExists = (scopeInfo.CustomAttributes?.Any() ?? false) || (scopeInfo.Scopes?.Any() ?? false);
            var exceptionExists = exception != null;
            var stateExists = state is CustomLogAttributeState;

            if (customAttributesFromScopeExists || exceptionExists || stateExists)
            {
                return true;
            }

            return false;
        }

        private static int GetLogLevelSeverity(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => 1,
                LogLevel.Debug => 5,
                LogLevel.Information => 9,
                LogLevel.Warning => 13,
                LogLevel.Error => 17,
                LogLevel.Critical => 21,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };
        }

        private void ReloadLoggerOptions(OtelJsonConsoleFormatterOptions options)
        {
            _formatterOptions = options;
        }

        public void Dispose()
        {
            _optionsReloadToken?.Dispose();
        }
    }
}
