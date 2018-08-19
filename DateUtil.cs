using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class DateUtil
    {
        public static bool DateWithinSameDay(DateTime dt, DateTime dt2)
        {
            if (dt.Year != dt2.Year ||
                dt.Month != dt2.Month ||
                dt.Day != dt2.Day) return false;
            return true;
        }
        public static DateTime ParseChineseDate(String dt)
        {
            System.Globalization.CultureInfo tc = new System.Globalization.CultureInfo("zh-TW");
            tc.DateTimeFormat.Calendar = new System.Globalization.TaiwanCalendar();
            return DateTime.Parse(dt, tc).Date;
        }
    }
}
