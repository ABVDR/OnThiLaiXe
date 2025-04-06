using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public class EFCauHoiRepository : ICauHoiRepository
    {
        private readonly ApplicationDbContext _context;
        public EFCauHoiRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<CauHoi>> GetAllAsync()
        {
            // return await _context.Products.ToListAsync(); 
            return await _context.CauHois.Include(p => p.ChuDe).Include(p => p.LoaiBangLai).ToListAsync();
        }
        public async Task<CauHoi> GetByIdAsync(int id)
        {
            return await _context.CauHois
                .Include(p => p.ChuDe)
                .Include(p => p.LoaiBangLai)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task AddAsync(CauHoi cauhoi)
        {
            _context.CauHois.Add(cauhoi);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(CauHoi cauhoi)
        {
            _context.CauHois.Update(cauhoi);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var cauHoi = await _context.CauHois.FindAsync(id);
            _context.CauHois.Remove(cauHoi);
            await _context.SaveChangesAsync();
        }
        public List<CauHoi> LayCauHoiTheoChuDe(int chuDeId, int soLuong)
        {
            return _context.CauHois
                           .Where(c => c.ChuDeId == chuDeId)
                           .OrderBy(r => Guid.NewGuid()) // Lấy ngẫu nhiên
                           .Take(soLuong)
                           .ToList();
        }
    }
}
