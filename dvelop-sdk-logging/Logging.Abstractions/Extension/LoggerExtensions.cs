using System;
using Dvelop.Sdk.Logging.Abstractions.State;
using Microsoft.Extensions.Logging;

namespace Dvelop.Sdk.Logging.Abstractions.Extension
{
    public static class LoggerExtensions
    {
        private static readonly Func<CustomLogAttributeState, Exception, string> _messageFormatter = MessageFormatter;

        public static void LogWithState(this ILogger logger, LogLevel level, EventId eventId, Exception exception, string message, CustomLogAttributeState state)
        {
            state.Message = message;
            logger.Log(level, eventId, state, exception, _messageFormatter);
        }

        public static void LogWithState(this ILogger logger, LogLevel level, EventId eventId, string message, CustomLogAttributeState state)
        {
            logger.LogWithState(level, eventId, null, message, state);
        }

        public static void LogWithState(this ILogger logger, LogLevel level, Exception exception, string message, CustomLogAttributeState state)
        {
            logger.LogWithState(level, 0, exception, message, state);
        }

        public static void LogWithState(this ILogger logger, LogLevel level, string message, CustomLogAttributeState state)
        {
            logger.LogWithState(level, 0, null, message, state);
        }

        private static string MessageFormatter(CustomLogAttributeState state, Exception error)
        {
            return state.ToString();
        }
    }
}
