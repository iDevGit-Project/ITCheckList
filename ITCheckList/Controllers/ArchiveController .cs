using ITCheckList.Models;
using ITCheckList.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITCheckList.Controllers
{
    public class ArchiveController : Controller
    {
        private readonly AppDbContext _context;

        public ArchiveController(AppDbContext context)
        {
            _context = context;
        }

        // بایگانی اطلاعات امروز
        [HttpPost]
        public async Task<IActionResult> ArchiveToday()
        {
            var today = DateTime.Today;
            var todayItems = await _context.TBLCheckItems
                .Where(x => x.CreatedAt.Date == today && x.Status == true)
                .ToListAsync();

            // بررسی وجود آیتم‌هایی که هنوز کامل نشده‌اند
            var pendingItemsExist = await _context.TBLCheckItems
                .AnyAsync(x => x.Status == false);

            if (pendingItemsExist)
            {
                // اگر هنوز آیتم ناقص وجود داره، نمایش صفحه خطا یا ریدایرکت
                TempData["ErrorMessage"] = "برخی از آیتم‌ها هنوز انجام نشده‌اند و نمی‌توان عملیات بایگانی را انجام داد.";
                return RedirectToAction(nameof(Index));
            }

            //if (!todayItems.Any())
            //{
            //    TempData["Warning"] = "هیچ گزارشی با تاریخ امروز برای بایگانی وجود ندارد.";
            //    return RedirectToAction("Index");
            //}

            foreach (var item in todayItems)
            {
                var archive = new TBL_CheckItemArchive
                {
                    Section = item.Section,
                    Description = item.Description,
                    CreatedAt = item.CreatedAt,
                    Note = item.Note,
                    Status = item.Status,
                    ArchivedAt = DateTime.Now
                };

                _context.TBLCheckItemArchives.Add(archive);
            }

            // در صورت نیاز حذف از جدول اصلی:
            // _context.TBLCheckItems.RemoveRange(todayItems);

            await _context.SaveChangesAsync();

            TempData["Success"] = "کارهای امروز با موفقیت بایگانی شدند.";
            return RedirectToAction("Index", "Checklist");
        }

        // نمایش آرشیو
        public async Task<IActionResult> Index()
        {
            var data = await _context.TBLCheckItemArchives
                .OrderByDescending(x => x.ArchivedAt)
                .ToListAsync();
            return View(data);
        }
    }
}
