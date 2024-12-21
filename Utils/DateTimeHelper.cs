using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Utils
{
    public static class DateTimeHelper
    {
        public static DateTime UnixTimeToDateTime(long unixtime)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixtime);

            return dateTimeOffset.LocalDateTime;
        }

        public static string FromTimeSpanTo2DigitsString(TimeSpan ts)
        {
            return $"{string.Format("{0:00}", ts.Hours)}:{string.Format("{0:00}", ts.Minutes)}:{string.Format("{0:00}", ts.Seconds)}";
        }

        public static DateTime? GetNullableDateTime(object objectToConvert)
        {
            if (objectToConvert == null || string.IsNullOrWhiteSpace(objectToConvert.ToString()))
                return null;

            if (!DateTime.TryParse(objectToConvert.ToString(), out DateTime result))
                return null;

            return result < new DateTime(1900, 1, 1) ? (DateTime?)null : result;
        }


    }
}
