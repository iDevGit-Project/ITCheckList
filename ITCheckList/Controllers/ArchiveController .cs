using ITCheckList.Models;
using ITCheckList.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ITCheckList.Controllers
{
    public class ArchiveController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "ChecklistItemsCache";
        public ArchiveController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // نمایش آرشیو
        public async Task<IActionResult> Index()
        {
            var data = await _context.TBLCheckItemArchives
                .OrderByDescending(x => x.ArchivedAt)
                .ToListAsync();
            return View(data);
        }

        // بایگانی اطلاعات امروز
        [HttpPost]
        public async Task<IActionResult> ArchiveToday(bool deleteAfterArchive = false)
        {
            try
            {
                var today = DateTime.Today;

                // گرفتن آیتم‌های امروز که وضعیت آنها "انجام شده" است
                var todayItems = await _context.TBLCheckItems
                    .Where(x => x.CreatedAt.Date == today && x.Status == true)
                    .ToListAsync();

                if (!todayItems.Any())
                {
                    return BadRequest("موردی برای بایگانی یافت نشد.");
                }

                // گرفتن آیتم‌های بایگانی شده امروز
                var archivedItemsToday = await _context.TBLCheckItemArchives
                    .Where(x => x.CreatedAt.Date == today)
                    .ToListAsync();

                // انتخاب آیتم‌هایی که هنوز بایگانی نشده‌اند (مقایسه با آرشیو امروز)
                var itemsToArchive = todayItems
                    .Where(item => !archivedItemsToday.Any(a =>
                        a.Section == item.Section &&
                        a.Description == item.Description &&
                        a.Note == item.Note &&
                        a.Status == item.Status &&
                        a.Duration == item.Duration)) // اضافه کردن Duration به شرط مقایسه
                    .ToList();

                if (!itemsToArchive.Any())
                {
                    return BadRequest("تمام موارد امروز قبلاً در بایگانی ثبت شده‌اند.");
                }

                // آماده‌سازی داده‌های آرشیو با اضافه کردن Duration و زمان آرشیو
                var archiveItems = itemsToArchive.Select(x => new TBL_CheckItemArchive
                {
                    Section = x.Section,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt,
                    Note = x.Note,
                    Status = x.Status,
                    Duration = x.Duration,         // اضافه شده
                    ArchivedAt = DateTime.Now      // زمان آرشیو
                }).ToList();

                await _context.TBLCheckItemArchives.AddRangeAsync(archiveItems);

                if (deleteAfterArchive)
                {
                    _context.TBLCheckItems.RemoveRange(itemsToArchive);
                }

                await _context.SaveChangesAsync();

                // حذف کش بعد از آرشیو
                _cache.Remove(CacheKey);

                return Ok("بایگانی با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "خطا در بایگانی: " + ex.Message);
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> ArchiveToday()
        //{
        //    var today = DateTime.Today;
        //    var todayItems = await _context.TBLCheckItems
        //        .Where(x => x.CreatedAt.Date == today && x.Status == true)
        //        .ToListAsync();

        //    var pendingItemsExist = await _context.TBLCheckItems
        //        .AnyAsync(x => x.Status == false);

        //    if (pendingItemsExist)
        //    {
        //        TempData["ErrorMessage"] = "برخی از آیتم‌ها هنوز انجام نشده‌اند و نمی‌توان عملیات بایگانی را انجام داد.";
        //        return RedirectToAction(nameof(Index));
        //    }

        //    foreach (var item in todayItems)
        //    {
        //        var duration = DateTime.Now - item.CreatedAt;
        //        var durationFormatted = $"{duration.Minutes:D2}:{duration.Seconds:D2}";

        //        var archive = new TBL_CheckItemArchive
        //        {
        //            Section = item.Section,
        //            Description = item.Description,
        //            CreatedAt = item.CreatedAt,
        //            Note = item.Note,
        //            Status = item.Status,
        //            ArchivedAt = DateTime.Now,
        //            Duration = durationFormatted // ⬅ ثبت مدت زمان
        //        };

        //        _context.TBLCheckItemArchives.Add(archive);
        //    }

        //    await _context.SaveChangesAsync();

        //    TempData["Success"] = "کارهای امروز با موفقیت بایگانی شدند.";
        //    return RedirectToAction("Index", "Checklist");
        //}

        //[HttpPost]
        //public async Task<IActionResult> ArchiveToday()
        //{
        //    var today = DateTime.Today;
        //    var todayItems = await _context.TBLCheckItems
        //        .Where(x => x.CreatedAt.Date == today && x.Status == true)
        //        .ToListAsync();

        //    // بررسی وجود آیتم‌هایی که هنوز کامل نشده‌اند
        //    var pendingItemsExist = await _context.TBLCheckItems
        //        .AnyAsync(x => x.Status == false);

        //    if (pendingItemsExist)
        //    {
        //        // اگر هنوز آیتم ناقص وجود داره، نمایش صفحه خطا یا ریدایرکت
        //        TempData["ErrorMessage"] = "برخی از آیتم‌ها هنوز انجام نشده‌اند و نمی‌توان عملیات بایگانی را انجام داد.";
        //        return RedirectToAction(nameof(Index));
        //    }

        //    //if (!todayItems.Any())
        //    //{
        //    //    TempData["Warning"] = "هیچ گزارشی با تاریخ امروز برای بایگانی وجود ندارد.";
        //    //    return RedirectToAction("Index");
        //    //}

        //    foreach (var item in todayItems)
        //    {
        //        var archive = new TBL_CheckItemArchive
        //        {
        //            Section = item.Section,
        //            Description = item.Description,
        //            CreatedAt = item.CreatedAt,
        //            Note = item.Note,
        //            Status = item.Status,
        //            ArchivedAt = DateTime.Now
        //        };

        //        _context.TBLCheckItemArchives.Add(archive);
        //    }

        //    // در صورت نیاز حذف از جدول اصلی:
        //    // _context.TBLCheckItems.RemoveRange(todayItems);

        //    await _context.SaveChangesAsync();

        //    TempData["Success"] = "کارهای امروز با موفقیت بایگانی شدند.";
        //    return RedirectToAction("Index", "Checklist");
        //}
    }
}
