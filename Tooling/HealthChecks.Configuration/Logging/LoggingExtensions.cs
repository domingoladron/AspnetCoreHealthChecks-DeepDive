using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HealthChecks.Configuration.Logging
{
    /// <summary>
    /// Extension methods to log in our standard JSON format
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Log a message in JSON format
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message">Message to log</param>
        /// <param name="data">Data to log with message</param>
        /// <param name="withMetaData">Log with date/level or raw data</param>
        public static void LogInformationJson(this ILogger logger, string message, dynamic data = null, bool withMetaData = true)
        {
            var formattedMessage = GetFormattedMessage(
                logger,
                LogLevel.Information,
                withMetaData,
                message,
                data
            );

            if(formattedMessage != null)
            {
                logger.LogInformation(message = formattedMessage);
            }
        }

        /// <summary>
        /// Log a message in JSON format
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message">Message to log</param>
        /// <param name="data">Data to log with message</param>
        /// <param name="withMetaData">Log with date/level or raw data</param>
        public static void LogDebugJson(this ILogger logger, string message, object data = null, bool withMetaData = true)
        {
            var formattedMessage = GetFormattedMessage(
                logger,
                LogLevel.Debug,
                withMetaData,
                message,
                data
            );

            if (formattedMessage != null)
            {
                logger.LogDebug(message = formattedMessage);
            }
        }

        /// <summary>
        /// Log a message in JSON format
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message">Message to log</param>
        /// <param name="data">Data to log with message</param>
        /// <param name="withMetaData">Log with date/level or raw data</param>
        public static void LogWarningJson(this ILogger logger, string message, dynamic data = null, bool withMetaData = true)
        {
            var formattedMessage = GetFormattedMessage(
                logger,
                LogLevel.Warning,
                withMetaData,
                message,
                data
            );

            if (formattedMessage != null)
            {
                logger.LogWarning(message = formattedMessage);
            }
        }

        /// <summary>
        /// Log a message in JSON format
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message">Message to log</param>
        /// <param name="data">Data to log with message</param>
        /// <param name="withMetaData">Log with date/level or raw data</param>
        public static void LogTraceJson(this ILogger logger, string message, dynamic data = null, bool withMetaData = true)
        {
            var formattedMessage = GetFormattedMessage(
                logger,
                LogLevel.Trace,
                withMetaData,
                message,
                data
            );

            if (formattedMessage != null)
            {
                logger.LogTrace(message = formattedMessage);
            }
        }

        /// <summary>
        /// Log a message in JSON format
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message">Message to log</param>
        /// <param name="data">Data to log with message</param>
        /// <param name="withMetaData">Log with date/level or raw data</param>
        public static void LogCriticalJson(this ILogger logger, string message, dynamic data = null, bool withMetaData = true)
        {
            var formattedMessage = GetFormattedMessage(
                logger,
                LogLevel.Critical,
                withMetaData,
                message,
                data
            );

            if (formattedMessage != null)
            {
                logger.LogCritical(message = formattedMessage);
            }
        }

        /// <summary>
        /// Log a message in JSON format
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message">Message to log</param>
        /// <param name="data">Data to log with message</param>
        /// <param name="withMetaData">Log with date/level or raw data</param>
        public static void LogErrorJson(this ILogger logger, string message, dynamic data = null, bool withMetaData = true)
        {
            var formattedMessage = GetFormattedMessage(
                logger,
                LogLevel.Error,
                withMetaData,
                message,
                data
            );

            if (formattedMessage != null)
            {
                logger.LogError(message = formattedMessage);
            }
        }

        private static string GetFormattedMessage(
            ILogger logger, 
            LogLevel level, 
            bool withMetaData, 
            string message, 
            dynamic data = null)
        {
            if (withMetaData)
            {
                data = new
                {
                    LongDate = DateTime.Now,
                    LongDateUtc = DateTime.UtcNow,
                    Level = level.ToString(),
                    Message = message,
                    Data = data
                };
            }
            else
            {
                data = new
                {
                    Message = message,
                    Data = data
                };
            }

            return SerializeMessage(logger, data);
        }

        private static string SerializeMessage(ILogger logger, dynamic message)
        {
            var serializationErrors = new List<string>();

            var serializedMessage = JsonConvert.SerializeObject(
                message,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                    Error = delegate (object sender, ErrorEventArgs args)
                    {
                        serializationErrors.Add(args.ErrorContext.Error.Message);
                        // Mark the serialization error as handled so it does not throw an exception up the call stack
                        args.ErrorContext.Handled = true;
                    }
                });

            if (serializationErrors.Count > 0)
            {
                logger.LogWarning(JsonConvert.SerializeObject(serializationErrors));
            }

            return serializedMessage;
        }
    }
}
