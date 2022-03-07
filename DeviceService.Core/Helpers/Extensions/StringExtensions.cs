using DeviceService.Core.Dtos.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DeviceService.Core.Helpers.Common.Utils;

namespace DeviceService.Core.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string ToDictionaryString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TKey : class where TValue : class
        {
            return $"{{ {string.Join(", ", dictionary.Select(kv => kv.Key + " = " + kv.Value).ToArray())} }}";
        }

        public static List<string> SplitCsv(this string csvList, bool nullOrWhitespaceInputReturnsNull = false)
        {
            if (string.IsNullOrWhiteSpace(csvList))
                return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

            return csvList
                .TrimEnd(',')
                .Split(',')
                .AsEnumerable<string>()
                .Select(s => s.Trim())
                .ToList();
        }

        public static void AddToLogs(this string messageLog, ref List<Log> logs, LogType logType = LogType.LOG_DEBUG, Exception exceptionLog = null)
        {
            logs.Add(new Log()
            {
                MessageLog = messageLog,
                LogType = (int)logType,
                ExceptionLog = exceptionLog
            });
        }
    }
}
