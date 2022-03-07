using DeviceService.Core.Dtos.Global;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static DeviceService.Core.Helpers.Common.Utils;

namespace DeviceService.Core.Helpers.Logging.Logger
{
    /// <summary>
    /// Log Writer Class
    /// </summary>
    /// <remarks>Author: MAC</remarks>
    public static class LogWriter
    {
        private static ILogger _Logger;
        
        public static ILogger Logger
        {
            set
            {
                _Logger = value;
            }
        }

        /// <summary>
        /// Write Log to the Configured Sink
        /// </summary>
        /// <param name="logs">List of Log Objects to write to Sink.</param>
        /// <returns>Initiated Token Details</returns>
        /// <response code="200">Returns the Initiated Token Details</response>
        public static void WriteLog(List<Log> logs)
        {
            foreach(var log in logs)
            {
                MainLogWriter(log.MessageLog, (LogType)log.LogType, log.ExceptionLog);
            }
        }

        /// <summary>
        /// Write Log to the Configured Sink
        /// </summary>
        /// <param name="messageLog">Message Log as Text</param>
        /// <param name="logType">Logging Type to Use</param>
        /// <param name="exceptionLog">Exception Object if any.</param>
        /// <returns>Initiated Token Details</returns>
        /// <response code="200">Returns the Initiated Token Details</response>
        public static void WriteLog(string messageLog, LogType logType, Exception exceptionLog = null)
        {
            MainLogWriter(messageLog, logType, exceptionLog);
        }

        private static void MainLogWriter(string messageLog, LogType logType, Exception exceptionLog = null)
        {
            try
            {
                switch (logType)
                {
                    case LogType.LOG_DEBUG:

                        _Logger.LogDebug(exceptionLog, messageLog);

                        break;
                    case LogType.LOG_INFORMATION:

                        _Logger.LogInformation(messageLog);

                        break;
                    case LogType.LOG_ERROR:

                        _Logger.LogError(messageLog);

                        break;
                    default:
                        //DO NOTHING
                        break;
                }
            }
            catch(Exception ex)
            {
                var eventLog = new EventLog();
                eventLog.Source = "DeviceService";
                //eventLog.WriteEntry("Error in: " + MyHttpContextAccessor.GetHttpContext()?.Request?.GetEncodedUrl());
                eventLog.WriteEntry(ex.Message, EventLogEntryType.Information);
            }           
        }

        //[Obsolete]
        public static void AddLogAndClearLogBuilderOnException(ref StringBuilder logBuilder, LogType logType, ref List<Log> logs, Exception exception, string exceptionMessage = null)
        {
            logs.Add(new Log()
            {
                LogType = (int)logType,
                MessageLog = logBuilder.ToString()
            });
            logBuilder.Clear();

            logs.Add(new Log()
            {
                LogType = (int)LogType.LOG_DEBUG,
                MessageLog = exceptionMessage,
                ExceptionLog = exception
            });
        }
    }
}
