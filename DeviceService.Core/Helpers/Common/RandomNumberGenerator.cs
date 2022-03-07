using CBN_eNaira.Core.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Helpers.Common
{
    public static class RandomNumberGenerator
    {
        public static string GenerateUniqueReference()
        {
            var randomNumber = new Random();
            var currentDateTime = DateTime.Now;
            var uniqueReference = $"{currentDateTime.Year.FormatDateVariables()}{currentDateTime.Day.FormatDateVariables()}{currentDateTime.Hour.FormatDateVariables()}{currentDateTime.Second.FormatDateVariables()}{currentDateTime.Month.FormatDateVariables()}{currentDateTime.Minute.FormatDateVariables()}{randomNumber.Next(10000, 99999)}";

            return uniqueReference;
        }
    }
}
