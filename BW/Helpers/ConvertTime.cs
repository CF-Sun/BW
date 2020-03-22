using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BW.Helpers
{
    public class ConvertTime
    {
        public DateTime UStoTW(DateTime time)
        {
            TimeZoneInfo US_TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            TimeZoneInfo TW_TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
            DateTime TW_DateTime = TimeZoneInfo.ConvertTime(time, TW_TimeZoneInfo);
            return TW_DateTime;
        }
    }
}