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
            if (IsDevelopment(context)) return "محیط توسعه  (لوکال)";
            if (IsStaging(context)) return "محیط تست (Staging)";
            if (IsProduction(context)) return "محیط اصلی (هاست)";
            return IsLocal(context) ? "سیستم محلی" : "هاست آنلاین";
        }

        // خروجی رنگ‌بندی مناسب برای وضعیت محیط
        public static string GetEnvironmentBadgeClass(HttpContext context)
        {
            if (IsDevelopment(context)) return "d-inline-flex fw-semibold text-warning-emphasis bg-warning-subtle border border-warning-subtle p-2 ";
            if (IsStaging(context)) return "d-inline-flex fw-semibold text-primary-emphasis bg-primary-subtle border border-primary-subtle p-2 ";
            if (IsProduction(context)) return "d-inline-flex fw-semibold text-success-emphasis bg-success-subtle border border-success-subtle p-2 ";
            return IsLocal(context) ? "d-inline-flex fw-semibold text-warning-emphasis bg-warning-subtle border border-warning-subtle p-2 " : "bg-secondary p-2 ";
        }

        public static string GetEnvironmentBadgeClass(string env)
        {
            return env switch
            {
                "Development" => "d-inline-flex fw-semibold text-warning-emphasis bg-warning-subtle border border-warning-subtle",
                "Staging" => "d-inline-flex fw-semibold text-primary-emphasis bg-primary-subtle border border-primary-subtle",
                "Production" => "d-inline-flex fw-semibold text-success-emphasis bg-success-subtle border border-success-subtle",
                _ => "d-inline-flex fw-semibold text-primary-emphasis bg-primary-subtle border border-primary-subtle"
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
