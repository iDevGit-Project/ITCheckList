namespace ITCheckList.Helpers
{
    using System.Globalization;

    public static class PersianDateConverter
    {
        public static DateTime ToGregorian(string persianDate)
        {
            var pc = new PersianCalendar();
            var parts = persianDate.Split('/');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            return pc.ToDateTime(year, month, day, 0, 0, 0, 0);
        }
    }

}
