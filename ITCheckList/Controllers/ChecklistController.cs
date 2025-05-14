using ITCheckList.Models;
using ITCheckList.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITCheckList.Controllers
{
    public class ChecklistController : Controller
    {
        private readonly AppDbContext _context;

        public ChecklistController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var items = _context.TBLCheckItems.OrderByDescending(c => c.CreatedAt).ToList();
            return View(items);
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
            if (ModelState.IsValid)
            {
                _context.TBLCheckItems.Add(item);
                _context.SaveChanges();
                return Json(new { success = true, message = "ثبت با موفقیت انجام شد." });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = string.Join(" - ", errors) });
        }
        #endregion

        #region عملیات ویرایش بررسی های جاری
        // GET: Checklist/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.TBLCheckItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Checklist/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TBL_CheckItem model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    // ارسال پیام موفقیت به صفحه با استفاده از TempData
                    TempData["SuccessMessage"] = "اطلاعات با موفقیت ویرایش شد.";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TBLCheckItems.Any(e => e.Id == model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(model);
        }
        #endregion

        #region عملیات حذف بررسی های جاری
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var item = _context.TBLCheckItems.Find(id);
            if (item == null)
                return Json(new { success = false, message = "مورد یافت نشد." });

            _context.TBLCheckItems.Remove(item);
            _context.SaveChanges();

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

                // دریافت آیتم‌های امروز با وضعیت تایید شده
                var todayItems = await _context.TBLCheckItems
                    .Where(x => x.CreatedAt.Date == today && x.Status == true)
                    .ToListAsync();

                if (!todayItems.Any())
                {
                    return BadRequest("موردی برای بایگانی یافت نشد.");
                }

                // دریافت همه آرشیوهای امروز برای بررسی تکراری بودن
                var archivedItemsToday = await _context.TBLCheckItemArchives
                    .Where(x => x.CreatedAt.Date == today)
                    .ToListAsync();

                // ایجاد لیست نهایی آیتم‌هایی که در آرشیو موجود نیستند
                var archiveItems = todayItems
                    .Where(item => !archivedItemsToday.Any(a =>
                        a.Section == item.Section &&
                        a.Description == item.Description &&
                        a.Note == item.Note &&
                        a.Status == item.Status))
                    .Select(x => new TBL_CheckItemArchive
                    {
                        Section = x.Section,
                        Description = x.Description,
                        CreatedAt = x.CreatedAt,
                        Note = x.Note,
                        Status = x.Status
                    }).ToList();

                if (!archiveItems.Any())
                {
                    return BadRequest("تمام موارد امروز قبلاً در بایگانی ثبت شده‌اند.");
                }

                // ذخیره در جدول بایگانی
                await _context.TBLCheckItemArchives.AddRangeAsync(archiveItems);

                // در صورت انتخاب، حذف از جدول اصلی
                if (deleteAfterArchive)
                {
                    _context.TBLCheckItems.RemoveRange(archiveItems.Select(x => new TBL_CheckItem
                    {
                        Section = x.Section,
                        Description = x.Description,
                        CreatedAt = x.CreatedAt,
                        Note = x.Note,
                        Status = x.Status
                    }));
                }

                await _context.SaveChangesAsync();

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
                .AnyAsync(x => x.Status == false); // یعنی هنوز "انجام نشده"

            return Json(new { hasPending });
        }
        #endregion

        #region بررسی وجود اطلاعات در جدول به جهت دریافت خروجی
        [HttpGet]
        public async Task<JsonResult> IsCheckItemTableEmpty()
        {
            bool isEmpty = !await _context.TBLCheckItems.AnyAsync();
            return Json(new { isEmpty });
        }
        #endregion
    }
}
