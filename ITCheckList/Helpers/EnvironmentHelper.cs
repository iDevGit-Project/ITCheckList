namespace ITCheckList.Helpers
{
    public static class EnvironmentHelper
    {
        public static string GetEnvironmentMessage(HttpContext httpContext)
        {
            var ip = httpContext.Connection?.RemoteIpAddress?.ToString();
            var host = httpContext.Request?.Host.Host?.ToLower();

            if (string.IsNullOrEmpty(host))
                return "محیط اجرای سیستم مشخص نیست.";

            if (host.Contains("localhost") || ip == "::1" || ip == "127.0.0.1")
            {
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    return "شما در محیط لوکال با دسترسی به اینترنت هستید.";

                return "شما در محیط لوکال آفلاین هستید.";
            }

            return "شما در حال استفاده از نسخه اصلی وب‌سایت هستید.";
        }
    }

}
