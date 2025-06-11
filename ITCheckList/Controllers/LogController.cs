using ITCheckList.Models.Context;
using Microsoft.AspNetCore.Mvc;

namespace ITCheckList.Controllers
{
    public class LogController : Controller
    {
        private readonly AppDbContext _context;
        public LogController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var logs = _context.TBLLogEntries
                .OrderByDescending(x => x.Timestamp)
                .Take(100)
                .ToList();

            return View(logs);
        }
    }
}
