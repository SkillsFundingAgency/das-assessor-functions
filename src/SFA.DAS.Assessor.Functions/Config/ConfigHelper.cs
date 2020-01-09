using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFA.DAS.Assessor.Functions.Config
{
    public class ConfigHelper
    {
        public static List<T> ConvertCsvValueToList<T>(string csvValue)
        {
            return csvValue.Split(',').ToList().ConvertAll(p => (T)Convert.ChangeType(p, typeof(T)));
        }
    }
}
