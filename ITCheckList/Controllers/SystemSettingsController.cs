using ITCheckList.Models;
using ITCheckList.Models.Context;
using ITCheckList.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Data.Common;

namespace ITCheckList.Controllers
{
    public class SystemSettingsController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly AppDbContext _context;
        private const string CacheKeysList = "ChecklistItemsCache";

        public SystemSettingsController(IMemoryCache cache, AppDbContext context)
        {
            _cache = cache;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ClearCache()
        {
            try
            {
                // فقط کش‌هایی که می‌دونی وجود دارن، حذف کن. (مثال: "SiteStats", "UserRoles", ...)
                string[] knownKeys = new[] { "SiteStats", "UserRoles", "SomethingElse" }; // بسته به پروژه‌ی شما

                foreach (var key in knownKeys)
                {
                    _cache.Remove(key);
                }

                return Content("پاک‌سازی کش انجام شد.");
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Content("خطا در پاک‌سازی کش: " + ex.Message);
            }
        }

        private async Task<long> GetRowCount(DbConnection connection, string tableName)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM [{tableName}]";
            return Convert.ToInt64(await cmd.ExecuteScalarAsync());
        }

        private async Task<long> GetTableSizeKB(DbConnection connection, string tableName)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $@"
        SELECT SUM(reserved_page_count) * 8
        FROM sys.dm_db_partition_stats
        WHERE object_id = OBJECT_ID(@tableName)";
            var param = cmd.CreateParameter();
            param.ParameterName = "@tableName";
            param.Value = tableName;
            cmd.Parameters.Add(param);

            var result = await cmd.ExecuteScalarAsync();
            return result != DBNull.Value ? Convert.ToInt64(result) : 0;
        }

        [HttpPost]
        public async Task<IActionResult> OptimizeDatabase()
        {
            var report = new List<TableOptimizationReport>();

            try
            {
                using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                var tableNames = new List<string>();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                        tableNames.Add(reader.GetString(0));
                }

                foreach (var table in tableNames)
                {
                    var item = new TableOptimizationReport
                    {
                        TableName = table,
                        RowCountBefore = await GetRowCount(connection, table),
                        SizeBeforeKB = await GetTableSizeKB(connection, table)
                    };

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = $"ALTER INDEX ALL ON [{table}] REBUILD";
                        await cmd.ExecuteNonQueryAsync();
                    }

                    item.RowCountAfter = await GetRowCount(connection, table);
                    item.SizeAfterKB = await GetTableSizeKB(connection, table);
                    item.WasOptimized = true;
                    report.Add(item);
                }

                await connection.CloseAsync();
                return Json(report); // ✅ برگرداندن JSON به جای View
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Content("خطا در بهینه‌سازی پایگاه داده: " + ex.Message);
            }
        }
    }
}
