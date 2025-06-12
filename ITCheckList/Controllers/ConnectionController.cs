using ITCheckList.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ITCheckList.Controllers
{
    [Route("Connection")]
    [ApiController]
    public class ConnectionController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly string _cacheKey = "CurrentConnectionString";

        public ConnectionController(IConfiguration configuration, IMemoryCache cache)
        {
            _configuration = configuration;
            _cache = cache;
        }

        [HttpPost("ResetCache")]
        public IActionResult ResetConnectionCache()
        {
            _cache.Remove(_cacheKey);
            return Ok(".کش رشته اتصال با موفقیت پاک شد.");
        }

        [HttpGet("Check")]
        public IActionResult Check()
        {
            try
            {
                var connectionString = GetCachedConnectionString();
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return Ok("✅ اتصال برقرار است.");
                }
            }
            catch
            {
                return StatusCode(500, "❌ اتصال به پایگاه داده برقرار نیست.");
            }
        }

        [HttpGet("GetCurrentConnectionString")]
        public IActionResult GetCurrentConnectionString()
        {
            try
            {
                var connectionString = GetCachedConnectionString();
                return Ok(connectionString ?? string.Empty);
            }
            catch
            {
                return StatusCode(500, "❌ خطا در دریافت رشته اتصال.");
            }
        }

        [HttpPost("TestLoginConnection")]
        public IActionResult TestLoginConnection([FromBody] ConnectionInput input)
        {
            if (string.IsNullOrWhiteSpace(input.Server))
                return BadRequest("فیلد Server الزامی است.");

            if (string.IsNullOrWhiteSpace(input.Database))
                return BadRequest("فیلد Database الزامی است.");

            if (string.IsNullOrWhiteSpace(input.AuthenticationType))
                return BadRequest("فیلد AuthenticationType الزامی است.");

            if (input.AuthenticationType == "sql")
            {
                if (string.IsNullOrWhiteSpace(input.Username))
                    return BadRequest("برای SQL Authentication، فیلد Username الزامی است.");

                if (string.IsNullOrWhiteSpace(input.Password))
                    return BadRequest("برای SQL Authentication، فیلد Password الزامی است.");
            }

            try
            {
                var connectionString = BuildConnectionString(input);
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                }

                UpdateConnectionStringInAppSettings(connectionString);

                // حذف کش پس از بروزرسانی رشته اتصال
                _cache.Remove(_cacheKey);

                return Ok("✅ اتصال برقرار شد و اطلاعات ذخیره شد.");
            }
            catch (SqlException ex)
            {
                return BadRequest($"❌ خطای SQL: {ex.Message}");
            }
            catch (Exception ex)
            {
                return BadRequest($"❌ خطای عمومی: {ex.Message}");
            }
        }

        private string GetCachedConnectionString()
        {
            if (!_cache.TryGetValue(_cacheKey, out string cachedConnectionString))
            {
                // به جای استفاده از IConfiguration، فایل را مستقیماً بخوان
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                var json = JObject.Parse(System.IO.File.ReadAllText(filePath));
                cachedConnectionString = json["ConnectionStrings"]?["StrConDb"]?.ToString() ?? string.Empty;

                _cache.Set(_cacheKey, cachedConnectionString);
            }
            return cachedConnectionString;
        }

        private string BuildConnectionString(ConnectionInput input)
        {
            if (input.AuthenticationType == "windows")
            {
                return $"Server={input.Server};Database={input.Database};Trusted_Connection=True;Trust Server Certificate=true;MultipleActiveResultSets=true";
            }
            else
            {
                return $"Server={input.Server};Database={input.Database};User ID={input.Username};Password={input.Password};Trusted_Connection=False;Trust Server Certificate=true;MultipleActiveResultSets=true";
            }
        }

        private void UpdateConnectionStringInAppSettings(string newConnectionString)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var json = JObject.Parse(System.IO.File.ReadAllText(filePath));

            if (json["ConnectionStrings"] == null)
                json["ConnectionStrings"] = new JObject();

            json["ConnectionStrings"]["StrConDb"] = newConnectionString;

            System.IO.File.WriteAllText(filePath, json.ToString());
        }

        public class ConnectionInput
        {
            public string Server { get; set; }
            public string Database { get; set; }
            public string AuthenticationType { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
