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

        // POST: Checklist/ArchiveTodayTasks
        [HttpPost]
        public async Task<IActionResult> ArchiveTodayTasks()
        {
            // گرفتن تاریخ امروز
            var today = DateTime.Now.Date;

            // پیدا کردن کارهایی که وضعیت "انجام شد" دارند و تاریخ آنها برابر امروز است
            var todayTasks = _context.TBLCheckItems.Where(x => x.Status == true && x.CreatedAt.Date == today).ToList();

            // اگر کارهایی برای بایگانی وجود داشته باشد
            if (todayTasks.Any())
            {
                // ایجاد آرایه‌ای از کارها برای بایگانی
                var archiveItems = todayTasks.Select(task => new TBL_CheckItemArchive
                {
                    Section = task.Section,
                    Description = task.Description,
                    CreatedAt = task.CreatedAt,
                    Note = task.Note,
                    Status = task.Status
                }).ToList();

                // اضافه کردن کارها به جدول بایگانی
                await _context.TBLCheckItemArchives.AddRangeAsync(archiveItems);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

    }
}
