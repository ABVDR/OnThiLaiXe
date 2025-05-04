using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public class EFVisitLogRepository : IVisitLogRepository
    {
        private readonly ApplicationDbContext _context;

        public EFVisitLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        private DateTime GetVietnamTime()
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone); // Chuyển đổi UTC về giờ Việt Nam
        }
        public async Task<int> GetTodayVisitorsCountAsync()
        {
            var now = GetVietnamTime();
            return await _context.VisitLogs.CountAsync(v => v.VisitTime.Date == now.Date);
        }

        public async Task<int> GetMonthVisitorsCountAsync()
        {
            var now = GetVietnamTime();
            return await _context.VisitLogs.CountAsync(v => v.VisitTime.Month == now.Month && v.VisitTime.Year == now.Year);
        }

        public async Task<int> GetYearVisitorsCountAsync()
        {
            var now = GetVietnamTime();
            return await _context.VisitLogs.CountAsync(v => v.VisitTime.Year == now.Year);
        }

        public async Task<int> GetYesterdayVisitorsCountAsync()
        {
            var now = GetVietnamTime();
            return await _context.VisitLogs.CountAsync(v => v.VisitTime.Date == now.Date.AddDays(-1));
        }

        public async Task<int> GetLastWeekVisitorsCountAsync()
        {
            var now = GetVietnamTime();
            return await _context.VisitLogs.CountAsync(v => v.VisitTime >= now.AddDays(-7) && v.VisitTime < now);
        }

        public async Task<int> GetLastMonthVisitorsCountAsync()
        {
            var now = GetVietnamTime();
            return await _context.VisitLogs.CountAsync(v => v.VisitTime.Month == now.Month - 1 && v.VisitTime.Year == now.Year);
        }

        public async Task<int> GetCurrentVisitorsCountAsync()
        {
            var now = GetVietnamTime();
            // Sử dụng thời gian tuyệt đối thay vì phép trừ để EF có thể dịch
            var fiveMinutesAgo = now.AddMinutes(-5);
            return await _context.VisitLogs.CountAsync(v => v.VisitTime >= fiveMinutesAgo);
        }

        public async Task<Dictionary<string, int>> GetVisitsByMonthAsync(int year)
        {
            var result = new Dictionary<string, int>();

            // Group visits by month and count
            var visitsByMonth = await _context.VisitLogs
                .Where(v => v.VisitTime.Year == year)
                .GroupBy(v => v.VisitTime.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            // Convert to dictionary with month names
            for (int month = 1; month <= 12; month++)
            {
                var monthData = visitsByMonth.FirstOrDefault(m => m.Month == month);
                var count = monthData != null ? monthData.Count : 0;
                var monthName = new DateTime(year, month, 1).ToString("MM/yyyy");
                result.Add(monthName, count);
            }

            return result;
        }
    }
}
