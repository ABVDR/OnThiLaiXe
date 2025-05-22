using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult GetVisitCounts()
        {
            var now = DateTime.UtcNow;

            var todayCount = _context.VisitLogs.Count(v => v.VisitTime.Date == now.Date);
            var monthCount = _context.VisitLogs.Count(v => v.VisitTime.Month == now.Month && v.VisitTime.Year == now.Year);
            var yearCount = _context.VisitLogs.Count(v => v.VisitTime.Year == now.Year);
            var yesterdayCount = _context.VisitLogs.Count(v => v.VisitTime.Date == now.Date.AddDays(-1));
            var lastWeekCount = _context.VisitLogs.Count(v => v.VisitTime >= now.AddDays(-7) && v.VisitTime < now);
            var lastMonthCount = _context.VisitLogs.Count(v => v.VisitTime.Month == now.Month - 1 && v.VisitTime.Year == now.Year);
            var currentVisitors = _context.VisitLogs.Count(v => (now - v.VisitTime).TotalMinutes < 10); // Ví dụ: khách đang truy cập trong 10 phút qua

            var stats = new
            {
                TodayCount = todayCount,
                MonthCount = monthCount,
                YearCount = yearCount,
                YesterdayCount = yesterdayCount,
                LastWeekCount = lastWeekCount,
                LastMonthCount = lastMonthCount,
                CurrentVisitors = currentVisitors
            };

            return Json(stats);
        }
    }
}
