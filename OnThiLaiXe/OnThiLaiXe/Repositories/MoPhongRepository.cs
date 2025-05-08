using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public class MoPhongRepository : IMoPhongRepository
    {
        private readonly ApplicationDbContext _context;

        public MoPhongRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MoPhong>> GetAllAsync()
        {
            return await _context.MoPhongs.ToListAsync();
        }

        public async Task<MoPhong> GetByIdAsync(int id)
        {
            return await _context.MoPhongs.FindAsync(id);
        }

        public async Task<MoPhong> AddAsync(MoPhong moPhong)
        {
            _context.MoPhongs.Add(moPhong);
            await _context.SaveChangesAsync();
            return moPhong;
        }

        public async Task UpdateAsync(MoPhong moPhong)
        {
            _context.Entry(moPhong).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var moPhong = await _context.MoPhongs.FindAsync(id);
            if (moPhong != null)
            {
                _context.MoPhongs.Remove(moPhong);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.MoPhongs.AnyAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<MoPhong>> GetOrderedAsync()
        {
            return await _context.MoPhongs.OrderBy(m => m.Order).ToListAsync();
        }
    }
}
