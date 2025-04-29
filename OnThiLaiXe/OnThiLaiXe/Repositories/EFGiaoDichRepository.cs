using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public class EFGiaoDichRepository : IGiaoDichRepository
    {
        private readonly ApplicationDbContext _context;

        public EFGiaoDichRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(GiaoDich giaoDich)
        {
            await _context.GiaoDichs.AddAsync(giaoDich);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var giaoDich = await _context.GiaoDichs.FindAsync(id);
            if (giaoDich != null)
            {
                _context.GiaoDichs.Remove(giaoDich);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<GiaoDich>> GetAllAsync()
        {
            return await _context.GiaoDichs.Include(g => g.User).ToListAsync();
        }

        public async Task<GiaoDich> GetByIdAsync(int id)
        {
            return await _context.GiaoDichs
                .Include(g => g.User)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<IEnumerable<GiaoDich>> GetGiaoDichByUserIdAsync(string userId)
        {
            return await _context.GiaoDichs
                .Where(g => g.UserId == userId)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountInMonthAsync(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await _context.GiaoDichs
                .Where(g => g.NgayThanhToan >= startDate &&
                            g.NgayThanhToan <= endDate &&
                            g.DaThanhToan)
                .SumAsync(g => g.SoTien);
        }

        public async Task<Dictionary<string, decimal>> GetRevenueByMonthAsync(int year)
        {
            var result = new Dictionary<string, decimal>();
            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var amount = await _context.GiaoDichs
                    .Where(g => g.NgayThanhToan >= startDate &&
                                g.NgayThanhToan <= endDate &&
                                g.DaThanhToan)
                    .SumAsync(g => g.SoTien);

                result.Add($"{month}/{year}", amount);
            }
            return result;
        }

        public async Task<int> GetNewUserCountInMonthAsync(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await _context.Users
                .Where(u => u.NgayTao >= startDate && u.NgayTao <= endDate)
                .CountAsync();
        }

        public async Task<IEnumerable<GiaoDich>> GetRecentTransactionsAsync(int count = 10)
        {
            return await _context.GiaoDichs
                .Include(g => g.User)
                .OrderByDescending(g => g.NgayThanhToan)
                .Take(count)
                .ToListAsync();
        }

        public async Task UpdateAsync(GiaoDich giaoDich)
        {
            _context.GiaoDichs.Update(giaoDich);
            await _context.SaveChangesAsync();
        }
    }
}