using MD.PersianDateTime.Standard;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITCheckList.Helpers
{
    public static class PersianDateHelpers
    {
        public static IHtmlContent ToPersianDate(this IHtmlHelper htmlHelper, DateTime dateTime, string format = "yyyy/MM/dd")
        {
            var persian = new PersianDateTime(dateTime).ToString(format);
            return new HtmlString(persian);
        }
    }
}
