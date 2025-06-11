using ITCheckList.Services;
using Microsoft.AspNetCore.Mvc;

namespace ITCheckList.Controllers
{
    public class CacheController : Controller
    {
        private readonly ICacheService _cacheService;

        public CacheController(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpGet]
        public IActionResult Report()
        {
            var items = CacheService.GetTrackedItems()
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
            return View(items);
        }

        [HttpPost]
        public IActionResult ClearAll()
        {
            foreach (var item in CacheService.GetTrackedItems())
            {
                _cacheService.Remove(item.Key);
            }

            return Content("همه کش‌ها با موفقیت پاک شدند.");
        }
    }
}
