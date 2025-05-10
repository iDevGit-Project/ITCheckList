using ITCheckList.Models;
using ITCheckList.Models.Context;
using Microsoft.AspNetCore.Mvc;

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

        // GET - بارگذاری فرم ویرایش در Modal
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var item = _context.TBLCheckItems.Find(id);
            if (item == null)
                return NotFound();

            return PartialView("_Edit", item);
        }

        [HttpPost]
        public JsonResult Edit([FromForm] TBL_CheckItem model)
        {
            if (!ModelState.IsValid)
            {
                // اگر ModelState نامعتبر باشد، خطاها را چاپ کن
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors)
                                                                 .Select(e => e.ErrorMessage));
                Console.WriteLine($"ModelState Errors: {errors}");

                return Json(new { success = false, message = "ورودی نامعتبر!" });
            }

            var item = _context.TBLCheckItems.Find(model.Id);
            if (item == null)
                return Json(new { success = false, message = "مورد یافت نشد!" });

            // بروزرسانی مقادیر
            item.Section = model.Section;
            item.Description = model.Description;
            item.Status = model.Status; // توجه کن که مقدار Status به درستی به روز می‌شود
            item.Note = model.Note;

            _context.SaveChanges();

            return Json(new { success = true, message = "با موفقیت ویرایش شد." });
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

    }
}
