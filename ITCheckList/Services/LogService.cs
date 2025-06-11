using ITCheckList.Models;
using ITCheckList.Models.Context;
using System.Net;

namespace ITCheckList.Services
{
    public class LogService : ILogService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;

        public LogService(AppDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        public async Task LogAsync(string controller, string action, string description, string userName = null, string entityId = "")
        {
            var ip = GetClientIp();

            // اگر userName صراحتاً پاس داده نشده بود، از HttpContext بگیر
            var username = !string.IsNullOrWhiteSpace(userName)
                ? userName
                : _http.HttpContext?.User?.Identity?.Name ?? "ناشناس";

            // اطمینان حاصل شود که فقط string معتبر وارد شود
            if (string.IsNullOrWhiteSpace(username) || username.All(char.IsDigit))
            {
                username = "ناشناس";
            }

            var log = new TBL_LogEntry
            {
                Action = action,
                EntityName = controller,
                EntityId = entityId,
                Description = description,
                Username = username,
                IP = ip
            };

            _context.TBLLogEntries.Add(log);
            await _context.SaveChangesAsync();
        }

        private string GetClientIp()
        {
            var context = _http.HttpContext;

            var forwardedFor = context?.Request?.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').FirstOrDefault()?.Trim();
            }

            var ipAddress = context?.Connection?.RemoteIpAddress;
            if (ipAddress != null && IPAddress.IsLoopback(ipAddress))
            {
                return "127.0.0.1";
            }

            return ipAddress?.ToString() ?? "نامشخص";
        }
    }

    //public class LogService : ILogService
    //{
    //    private readonly AppDbContext _context;
    //    private readonly IHttpContextAccessor _http;

    //    public LogService(AppDbContext context, IHttpContextAccessor http)
    //    {
    //        _context = context;
    //        _http = http;
    //    }

    //    public async Task LogAsync(string controller, string action, string description, string userName = "ناشناس", string entityId = "")
    //    {
    //        var ip = GetClientIp();
    //        var username = !string.IsNullOrEmpty(userName)
    //            ? userName
    //            : _http.HttpContext?.User?.Identity?.Name ?? "ناشناس";

    //        var log = new TBL_LogEntry
    //        {
    //            Action = action,
    //            EntityName = controller,
    //            EntityId = entityId,
    //            Description = description,
    //            Username = username,
    //            IP = ip
    //        };

    //        _context.TBLLogEntries.Add(log);
    //        await _context.SaveChangesAsync();
    //    }

    //    private string GetClientIp()
    //    {
    //        var context = _http.HttpContext;

    //        // بررسی هدر X-Forwarded-For برای حالت‌های پشت CDN/Proxy یا هاست‌های آنلاین
    //        var forwardedFor = context?.Request?.Headers["X-Forwarded-For"].FirstOrDefault();
    //        if (!string.IsNullOrEmpty(forwardedFor))
    //        {
    //            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
    //        }

    //        var ipAddress = context?.Connection?.RemoteIpAddress;

    //        if (ipAddress != null && IPAddress.IsLoopback(ipAddress))
    //        {
    //            return "127.0.0.1";
    //        }

    //        return ipAddress?.ToString() ?? "نامشخص";
    //    }
    //}
}
