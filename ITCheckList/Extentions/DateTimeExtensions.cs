using System;
using System.Globalization;

namespace ITCheckList.Extentions
{
    public static class DateTimeExtensions
    {
        public static string ToShamsi(this DateTime dateTime)
        {
            PersianCalendar pc = new PersianCalendar();
            string year = pc.GetYear(dateTime).ToString("0000");
            string month = pc.GetMonth(dateTime).ToString("00");
            string day = pc.GetDayOfMonth(dateTime).ToString("00");

            string hour = pc.GetHour(dateTime).ToString("00");
            string minute = pc.GetMinute(dateTime).ToString("00");

            return $"{year}/{month}/{day} - {hour}:{minute}";
        }
    }
}
