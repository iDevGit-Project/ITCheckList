using ITCheckList.Models;
using ITCheckList.Models.Context;
using ITCheckList.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;

namespace ITCheckList.Controllers
{
    public class ChecklistController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogService _logService;

        private const string CacheKey = "ChecklistItemsCache";

        public ChecklistController(AppDbContext context, IMemoryCache cache, ILogService logService)
        {
            _context = context;
            _cache = cache;
            _logService = logService;
        }
        public IActionResult Index()
        {
            DateTime today = DateTime.Now.Date;
            DateTime threeDaysAgo = today.AddDays(-3);

            // بررسی وجود داده‌های قدیمی که بایگانی نشده‌اند
            var unarchivedOldData = _context.TBLCheckItems
                .Where(c => c.CreatedAt.Date >= threeDaysAgo && c.CreatedAt.Date < today)
                .OrderBy(c => c.CreatedAt)
                .ToList();

            ViewBag.HasUnarchivedOldData = unarchivedOldData.Any();

            if (unarchivedOldData.Any())
            {
                // نمایش داده‌های قدیمی به جای داده‌های روز جاری
                ViewBag.Items = unarchivedOldData;
            }
            else
            {
                // نمایش داده‌های امروز از کش یا دیتابیس
                if (!_cache.TryGetValue(CacheKey, out List<TBL_CheckItem> items))
                {
                    items = _context.TBLCheckItems
                        .Where(c => c.CreatedAt.Date == today)
                        .OrderByDescending(c => c.CreatedAt)
                        .ToList();

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                    _cache.Set(CacheKey, items, cacheOptions);
                }

                ViewBag.Items = items;
            }

            return View(ViewBag.Items);
        }

        #region عملیات ثبت بررسی جدید
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_Create", new TBL_CheckItem());
        }

        [HttpPost]
        public async Task<IActionResult> Create(TBL_CheckItem item)
        {
            if (item == null)
            {
                await _logService.LogAsync("ChecklistController", "Create", "ورودی خالی بود.");
                return Json(new { success = false, message = "اطلاعات ارسالی نامعتبر است.", type = "error" });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                var errorMessage = string.Join(" - ", errors);

                await _logService.LogAsync("ChecklistController", "Create", $"مدل نامعتبر: {errorMessage}");
                return Json(new { success = false, message = errorMessage, type = "warning" });
            }

            _context.TBLCheckItems.Add(item);
            await _context.SaveChangesAsync();

            _cache.Remove("ChecklistItemsCache");

            await _logService.LogAsync("ChecklistController", "Create", "آیتم با موفقیت ثبت شد.", entityId: item.Id.ToString());

            return Json(new { success = true, message = "ثبت با موفقیت انجام شد.", type = "success" });
        }
        #endregion

        #region عملیات ویرایش بررسی های جاری
        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var item = await _context.TBLCheckItems.FindAsync(id);
            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TBL_CheckItem model)
        {
            if (id != model.Id)
            {
                await _logService.LogAsync("ChecklistController", "Edit", "شناسه ارسالی با مدل مطابقت ندارد.");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await _logService.LogAsync("ChecklistController", "Edit", "مدل نامعتبر بود.");
                return View(model);
            }

            try
            {
                var existingItem = await _context.TBLCheckItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (existingItem == null)
                {
                    return NotFound();
                }

                // مقدار CreatedAt و DurationInSeconds را حفظ می‌کنیم
                model.CreatedAt = existingItem.CreatedAt;
                //model.DurationInSeconds = existingItem.DurationInSeconds;

                _context.Update(model);
                await _context.SaveChangesAsync();

                _cache.Remove("ChecklistItemsCache");

                await _logService.LogAsync("ChecklistController", "Edit", "آیتم با موفقیت ویرایش شد.", entityId: model.Id.ToString());

                TempData["SuccessMessage"] = "اطلاعات با موفقیت ویرایش شد.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.TBLCheckItems.Any(e => e.Id == model.Id))
                {
                    await _logService.LogAsync("ChecklistController", "Edit", "آیتم برای ویرایش یافت نشد.", entityId: model.Id.ToString());
                    return NotFound();
                }

                throw;
            }
        }



        // POST: Edit

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, TBL_CheckItem model)
        //{
        //    if (id != model.Id)
        //        return NotFound();

        //    if (!ModelState.IsValid)
        //        return View(model);

        //    try
        //    {
        //        _context.Update(model);
        //        await _context.SaveChangesAsync();

        //        // پاک کردن کش پس از تغییر وضعیت
        //        _cache.Remove("ChecklistItemsCache");

        //        TempData["SuccessMessage"] = "اطلاعات با موفقیت ویرایش شد.";
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!_context.TBLCheckItems.Any(e => e.Id == model.Id))
        //            return NotFound();

        //        throw;
        //    }
        //}
        #endregion

        #region عملیات حذف بررسی های جاری
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                await _logService.LogAsync("ChecklistController", "Delete", "شناسه حذف نامعتبر بود.", "N/A");
                return Json(new { success = false, message = "شناسه معتبر نیست.", type = "error" });
            }

            var item = await _context.TBLCheckItems.FindAsync(id);
            if (item == null)
            {
                await _logService.LogAsync("ChecklistController", "Delete", $"آیتم با شناسه {id} یافت نشد.", id.ToString());
                return Json(new { success = false, message = "مورد مورد نظر یافت نشد.", type = "warning" });
            }

            try
            {
                _context.TBLCheckItems.Remove(item);
                await _context.SaveChangesAsync();

                // پاک کردن کش پس از حذف
                _cache.Remove("ChecklistItemsCache");

                await _logService.LogAsync("ChecklistController", "Delete", "آیتم با موفقیت حذف شد.", id.ToString());

                return Json(new { success = true, message = "آیتم با موفقیت حذف شد.", type = "success" });
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("ChecklistController", "Delete", $"خطا در حذف آیتم: {ex.Message}", id.ToString());
                return Json(new { success = false, message = "خطایی در حذف اطلاعات رخ داد.", type = "error" });
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var item = await _context.TBLCheckItems.FindAsync(id);
        //    if (item == null)
        //        return Json(new { success = false, message = "مورد یافت نشد." });

        //    _context.TBLCheckItems.Remove(item);
        //    await _context.SaveChangesAsync();  // حتماً باید باشه

        //    // پاک کردن کش پس از تغییر وضعیت
        //    _cache.Remove("ChecklistItemsCache");

        //    return Json(new { success = true, message = "آیتم با موفقیت حذف شد." });
        //}
        #endregion

        #region متد های مربوط به عملیات آرشیو کردن اطلاعات
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
        //public async Task<IActionResult> ArchiveToday(bool deleteAfterArchive = false)
        //{
        //    try
        //    {
        //        var today = DateTime.Today;

        //        var todayItems = await _context.TBLCheckItems
        //            .Where(x => x.CreatedAt.Date == today && x.Status == true)
        //            .ToListAsync();

        //        if (!todayItems.Any())
        //        {
        //            return BadRequest("موردی برای بایگانی یافت نشد.");
        //        }

        //        var archivedItemsToday = await _context.TBLCheckItemArchives
        //            .Where(x => x.CreatedAt.Date == today)
        //            .ToListAsync();

        //        var itemsToArchive = todayItems
        //            .Where(item => !archivedItemsToday.Any(a =>
        //                a.Section == item.Section &&
        //                a.Description == item.Description &&
        //                a.Note == item.Note &&
        //                a.Status == item.Status))
        //            .ToList();

        //        if (!itemsToArchive.Any())
        //        {
        //            return BadRequest("تمام موارد امروز قبلاً در بایگانی ثبت شده‌اند.");
        //        }

        //        var archiveItems = itemsToArchive.Select(x => new TBL_CheckItemArchive
        //        {
        //            Section = x.Section,
        //            Description = x.Description,
        //            CreatedAt = x.CreatedAt,
        //            Note = x.Note,
        //            Status = x.Status
        //        }).ToList();

        //        await _context.TBLCheckItemArchives.AddRangeAsync(archiveItems);

        //        if (deleteAfterArchive)
        //        {
        //            _context.TBLCheckItems.RemoveRange(itemsToArchive);
        //        }

        //        await _context.SaveChangesAsync();

        //        // حذف کش بعد از آرشیو (که احتمالا داده‌ها تغییر کرده‌اند)
        //        _cache.Remove(CacheKey);

        //        return Ok("بایگانی با موفقیت انجام شد.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "خطا در بایگانی: " + ex.Message);
        //    }
        //}

        [HttpGet]
        public async Task<JsonResult> CheckPreviousDayData()
        {
            var today = DateTime.Today;

            var hasOldData = await _context.TBLCheckItems
                .AnyAsync(x => x.CreatedAt.Date < today);

            return Json(new { hasOldData });
        }

        [HttpGet]
        public async Task<IActionResult> HasPendingItems()
        {
            var hasPending = await _context.TBLCheckItems
                .AnyAsync(x => x.Status == false);

            return Json(new { hasPending });
        }
        #endregion

        #region بررسی وجود اطلاعات در جدول تسک های روزانه
        [HttpGet]
        public async Task<JsonResult> IsCheckItemTableEmpty()
        {
            bool isEmpty = !await _context.TBLCheckItems.AnyAsync();
            return Json(new { isEmpty });
        }
        #endregion

        #region عملیات مربوط به دریافت گزارش از جدول بایگانی
        public IActionResult ArchiveIndex()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ArchiveIndex(string selectedDate)
        {
            if (string.IsNullOrWhiteSpace(selectedDate))
            {
                ViewBag.ErrorMessage = "لطفاً یک تاریخ انتخاب کنید.";
                return View(new List<TBL_CheckItemArchive>());
            }

            try
            {
                selectedDate = ConvertPersianToEnglishNumbers(selectedDate);

                var parts = selectedDate.Split('/');
                if (parts.Length != 3 ||
                    !int.TryParse(parts[0], out int year) ||
                    !int.TryParse(parts[1], out int month) ||
                    !int.TryParse(parts[2], out int day))
                {
                    ViewBag.ErrorMessage = "فرمت تاریخ نادرست است.";
                    return View(new List<TBL_CheckItemArchive>());
                }

                var persianCalendar = new System.Globalization.PersianCalendar();
                var selectedMiladiDate = persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);
                var nextDay = selectedMiladiDate.AddDays(1);

                var archives = _context.TBLCheckItemArchives
                    .Where(x => x.CreatedAt >= selectedMiladiDate && x.CreatedAt < nextDay)
                    .OrderBy(x => x.CreatedAt)
                    .ToList();

                if (!archives.Any())
                {
                    ViewBag.NoData = "برای تاریخ انتخاب‌شده داده‌ای در بایگانی موجود نیست.";
                }

                ViewBag.SelectedDate = selectedDate;
                return View(archives);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "خطا در پردازش تاریخ: " + ex.Message;
                return View(new List<TBL_CheckItemArchive>());
            }
        }

        private string ConvertPersianToEnglishNumbers(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string[] persianDigits = { "۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹" };
            string[] englishDigits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            for (int i = 0; i < persianDigits.Length; i++)
            {
                input = input.Replace(persianDigits[i], englishDigits[i]);
            }

            return input;
        }
        #endregion

        #region عملیات مربوط به بررسی اطلاعات بایگانی نشده طی 1 تا 3 روز گذشته
        [HttpPost]
        public IActionResult ArchivePreviousDayData()
        {
            DateTime today = DateTime.Now.Date;
            DateTime threeDaysAgo = today.AddDays(-3);

            var unarchivedOldData = _context.TBLCheckItems
                .Where(c => c.CreatedAt.Date >= threeDaysAgo && c.CreatedAt.Date < today)
                .ToList();

            if (unarchivedOldData == null || unarchivedOldData.Count == 0)
            {
                return Json(new { success = false, message = "داده‌ای جهت بایگانی یافت نشد." });
            }

            foreach (var item in unarchivedOldData)
            {
                var existing = _context.TBLCheckItemArchives
                    .FirstOrDefault(a =>
                        a.Section == item.Section &&
                        a.Description == item.Description &&
                        a.CreatedAt.Date == item.CreatedAt.Date);

                if (existing != null)
                {
                    existing.Note = item.Note;
                    existing.Status = true; // وضعیت به "انجام شد" تغییر داده می‌شود
                    existing.ArchivedAt = DateTime.Now;
                }
                else
                {
                    _context.TBLCheckItemArchives.Add(new TBL_CheckItemArchive
                    {
                        Section = item.Section,
                        Description = item.Description,
                        CreatedAt = item.CreatedAt,
                        Note = item.Note,
                        Status = true, // وضعیت به "انجام شد" تغییر داده می‌شود
                        ArchivedAt = DateTime.Now
                    });
                }
            }
            _context.TBLCheckItems.RemoveRange(unarchivedOldData);
            _context.SaveChanges();
            // پاک کردن کش پس از تغییر وضعیت
            _cache.Remove("ChecklistItemsCache");

            return Json(new { success = true, message = "بایگانی با موفقیت انجام شد." });
        }
        #endregion

        #region عملیات مربوط به بررسی وضعیت روز جاری
        [HttpGet]
        public IActionResult CheckTodayData()
        {
            var today = DateTime.Today;

            // گرفتن همه آیتم‌های امروز
            var todayItems = _context.TBLCheckItems
                .Where(x => x.CreatedAt.Date == today)
                .ToList();

            bool hasTodayData = todayItems.Any();

            // بررسی اینکه همه آیتم‌ها هنوز انجام نشده‌اند (در دست بررسی هستند)
            bool allPending = hasTodayData && todayItems.All(x => x.Status == false);

            return Json(new { hasTodayData, allPending });
        }

        #endregion

        #region عملیات تأیید کردن مستقیم درخواست روز جاری بدون ورود به قسمت ویرایش
        [HttpPost]
        public async Task<JsonResult> ConfirmFinal(int id)
        {
            var item = _context.TBLCheckItems.FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                return Json(new { success = false, message = "آیتم مورد نظر یافت نشد." });
            }

            // محاسبه مدت زمان سپری شده از زمان شروع تا الان
            TimeSpan duration = DateTime.Now - item.CreatedAt;
            string durationFormatted = $"{duration.Minutes:D2}:{duration.Seconds:D2}";

            // ذخیره مدت زمان در مدل
            item.Status = true;
            item.Duration = durationFormatted;

            await _context.SaveChangesAsync();

            // ثبت لاگ با زمان سپری شده
            await _logService.LogAsync(
                controller: "Checklist",
                action: "ConfirmFinal",
                description: $"آیتم با موفقیت تأیید نهایی شد. مدت زمان سپری شده: {durationFormatted}",
                entityId: item.Id.ToString()
            );

            // پاک کردن کش پس از تغییر وضعیت
            _cache.Remove("ChecklistItemsCache");

            return Json(new { success = true });
        }

        //[HttpPost]
        //public async Task<JsonResult> ConfirmFinal(int id)
        //{
        //    var item = await _context.TBLCheckItems.FirstOrDefaultAsync(x => x.Id == id);
        //    if (item == null)
        //    {
        //        return Json(new { success = false, message = "آیتم مورد نظر یافت نشد." });
        //    }

        //    if (!item.Status)
        //    {
        //        var duration = DateTime.Now - item.CreatedAt;
        //        var formatted = $"{(int)duration.TotalMinutes:D2}:{duration.Seconds:D2}";

        //        item.Status = true;
        //        item.Duration = formatted;

        //        await _context.SaveChangesAsync();

        //        _cache.Remove("ChecklistItemsCache");

        //        // ثبت در لاگ
        //        await _logService.LogAsync(
        //            controller: "Checklist",
        //            action: "ConfirmFinal",
        //            description: $"آیتم با شناسه {id} نهایی شد. مدت زمان: {formatted}",
        //            entityId: id.ToString()
        //        );

        //        // ثبت در آرشیو (در صورت وجود متد مرتبط ارسال کن تا اون رو هم اصلاح کنیم)
        //    }

        //    return Json(new { success = true });
        //}

        //[HttpPost]
        //public JsonResult ConfirmFinal(int id)
        //{
        //    var item = _context.TBLCheckItems.FirstOrDefault(x => x.Id == id);
        //    if (item == null)
        //    {
        //        return Json(new { success = false, message = "آیتم مورد نظر یافت نشد." });
        //    }

        //    item.Status = true;

        //    // محاسبه و ثبت زمان سپری شده به ثانیه
        //    var duration = (DateTime.Now - item.CreatedAt).TotalSeconds;
        //    item.DurationInSeconds = (int)duration;

        //    _context.SaveChanges();

        //    // پاک کردن کش پس از تغییر وضعیت
        //    _cache.Remove("ChecklistItemsCache");

        //    return Json(new { success = true });
        //}

        //[HttpPost]
        //public JsonResult ConfirmFinal(int id)
        //{
        //    var item = _context.TBLCheckItems.FirstOrDefault(x => x.Id == id);
        //    if (item == null)
        //    {
        //        return Json(new { success = false, message = "آیتم مورد نظر یافت نشد." });
        //    }

        //    item.Status = true;
        //    _context.SaveChanges();
        //    // پاک کردن کش پس از تغییر وضعیت
        //    _cache.Remove("ChecklistItemsCache");

        //    return Json(new { success = true });
        //}

        #endregion

    }
}
