using ITCheckList.Models;
using ITCheckList.Models.Context;
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

        private const string CacheKey = "ChecklistItemsCache";

        public ChecklistController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
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
        public IActionResult Create(TBL_CheckItem item)
        {
            if (item == null)
            {
                return Json(new { success = false, message = "اطلاعات ارسالی نامعتبر است.", type = "error" });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = string.Join(" - ", errors), type = "warning" });
            }

            _context.TBLCheckItems.Add(item);
            _context.SaveChanges();

            // حذف کش بعد از افزودن آیتم جدید
            _cache.Remove(CacheKey);

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

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TBL_CheckItem model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();

                // حذف کش بعد از ویرایش
                _cache.Remove(CacheKey);

                TempData["SuccessMessage"] = "اطلاعات با موفقیت ویرایش شد.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.TBLCheckItems.Any(e => e.Id == model.Id))
                    return NotFound();

                throw;
            }
        }

        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var item = await _context.TBLCheckItems.FindAsync(id);
        //    if (item == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(item);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, TBL_CheckItem model)
        //{
        //    if (id != model.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(model);
        //            await _context.SaveChangesAsync();

        //            // حذف کش بعد از ویرایش
        //            _cache.Remove(CacheKey);

        //            TempData["SuccessMessage"] = "اطلاعات با موفقیت ویرایش شد.";

        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!_context.TBLCheckItems.Any(e => e.Id == model.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //    }
        //    return View(model);
        //}
        #endregion

        #region عملیات حذف بررسی های جاری
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TBLCheckItems.FindAsync(id);
            if (item == null)
                return Json(new { success = false, message = "مورد یافت نشد." });

            _context.TBLCheckItems.Remove(item);
            await _context.SaveChangesAsync();  // حتماً باید باشه

            // حذف کش بعد از حذف
            _cache.Remove(CacheKey);

            return Json(new { success = true, message = "آیتم با موفقیت حذف شد." });
        }
        #endregion

        #region متد های مربوط به عملیات آرشیو کردن اطلاعات
        [HttpPost]
        public async Task<IActionResult> ArchiveToday(bool deleteAfterArchive = false)
        {
            try
            {
                var today = DateTime.Today;

                var todayItems = await _context.TBLCheckItems
                    .Where(x => x.CreatedAt.Date == today && x.Status == true)
                    .ToListAsync();

                if (!todayItems.Any())
                {
                    return BadRequest("موردی برای بایگانی یافت نشد.");
                }

                var archivedItemsToday = await _context.TBLCheckItemArchives
                    .Where(x => x.CreatedAt.Date == today)
                    .ToListAsync();

                var itemsToArchive = todayItems
                    .Where(item => !archivedItemsToday.Any(a =>
                        a.Section == item.Section &&
                        a.Description == item.Description &&
                        a.Note == item.Note &&
                        a.Status == item.Status))
                    .ToList();

                if (!itemsToArchive.Any())
                {
                    return BadRequest("تمام موارد امروز قبلاً در بایگانی ثبت شده‌اند.");
                }

                var archiveItems = itemsToArchive.Select(x => new TBL_CheckItemArchive
                {
                    Section = x.Section,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt,
                    Note = x.Note,
                    Status = x.Status
                }).ToList();

                await _context.TBLCheckItemArchives.AddRangeAsync(archiveItems);

                if (deleteAfterArchive)
                {
                    _context.TBLCheckItems.RemoveRange(itemsToArchive);
                }

                await _context.SaveChangesAsync();

                // حذف کش بعد از آرشیو (که احتمالا داده‌ها تغییر کرده‌اند)
                _cache.Remove(CacheKey);

                return Ok("بایگانی با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "خطا در بایگانی: " + ex.Message);
            }
        }

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
            _cache.Remove(CacheKey);

            return Json(new { success = true, message = "بایگانی با موفقیت انجام شد." });
        }
        #endregion

        #region عملیات مربوط به بررسی وضعیت روز جاری
        [HttpGet]
        public IActionResult CheckTodayData()
        {
            var today = DateTime.Today;
            var hasTodayData = _context.TBLCheckItems.Any(x => x.CreatedAt.Date == today);

            // بررسی وضعیت مواردی که در وضعیت "در دست اقدام" هستند
            var allPending = _context.TBLCheckItems
                                .Where(x => x.CreatedAt.Date == today)
                                .All(x => !x.Status);

            return Json(new { hasTodayData, allPending });
        }
        #endregion

        #region عملیات تأیید کردن مستقیم درخواست روز جاری بدون ورود به قسمت ویرایش
        [HttpPost]
        public JsonResult ConfirmFinal(int id)
        {
            var item = _context.TBLCheckItems.FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                return Json(new { success = false, message = "آیتم مورد نظر یافت نشد." });
            }

            item.Status = true;
            _context.SaveChanges();

            return Json(new { success = true });
        }

        #endregion

    }
}
