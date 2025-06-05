using System.Net;

namespace ITCheckList.Helpers
{
    public static class EnvironmentHelper
    {
        // بررسی اینکه آیا پروژه روی لوکال اجرا می‌شود یا نه
        public static bool IsLocal(HttpContext context)
        {
            if (context == null)
                return false;

            var connection = context.Connection;
            var remoteIp = connection?.RemoteIpAddress;
            var localIp = connection?.LocalIpAddress;

            // سناریوهای مختلف برای لوکال:
            return remoteIp == null ||
                   remoteIp.Equals(IPAddress.Loopback) ||          // 127.0.0.1
                   remoteIp.Equals(IPAddress.IPv6Loopback) ||      // ::1
                   (localIp != null && remoteIp.Equals(localIp)) || // اتصال داخلی
                   remoteIp.ToString().StartsWith("192.168.") ||   // شبکه داخلی
                   remoteIp.ToString().StartsWith("10.") ||        // Private network
                   remoteIp.ToString().StartsWith("172.16.");      // Private network
        }

        // پیام نمایشی محیط اجرا
        public static string GetEnvironmentMessage(HttpContext context)
        {
            if (IsDevelopment(context)) return "محیط توسعه (لوکال)";
            if (IsStaging(context)) return "محیط تست (Staging)";
            if (IsProduction(context)) return "محیط اصلی (هاست)";
            return IsLocal(context) ? "سیستم محلی" : "هاست آنلاین";
        }

        // خروجی رنگ‌بندی مناسب برای وضعیت محیط
        public static string GetEnvironmentBadgeClass(HttpContext context)
        {
            if (IsDevelopment(context)) return "bg-warning text-dark";
            if (IsStaging(context)) return "bg-info text-dark";
            if (IsProduction(context)) return "bg-success";
            return IsLocal(context) ? "bg-warning text-dark" : "bg-secondary";
        }

        public static string GetEnvironmentBadgeClass(string env)
        {
            return env switch
            {
                "Development" => "bg-warning text-dark",
                "Staging" => "bg-primary text-dark",
                "Production" => "bg-success text-dark",
                _ => "bg-secondary text-dark"
            };
        }

        // محیط فعلی پروژه از طریق IWebHostEnvironment
        public static string GetEnvironmentName(HttpContext context)
        {
            var env = context?.RequestServices?.GetService<IWebHostEnvironment>();
            return env?.EnvironmentName ?? "Unknown";
        }

        public static bool IsDevelopment(HttpContext context)
        {
            return GetEnvironmentName(context).Equals("Development", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsStaging(HttpContext context)
        {
            return GetEnvironmentName(context).Equals("Staging", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsProduction(HttpContext context)
        {
            return GetEnvironmentName(context).Equals("Production", StringComparison.OrdinalIgnoreCase);
        }
    }
}
