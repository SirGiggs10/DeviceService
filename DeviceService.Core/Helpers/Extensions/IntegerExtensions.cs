using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Helpers.Extensions
{
    public static class IntegerExtensions
    {
        /// <summary>
        /// Formats Date Variables and Adds Zero In Front of Variables Less than 10
        /// </summary>
        /// <param name="dateVariable"></param>
        /// <returns>Date Variable With/Without Zero in Front</returns>
        public static string FormatDateVariables(this int dateVariable)
        {
            return dateVariable < 10 ? $"0{dateVariable}" : $"{dateVariable}";
        }
    }
}
