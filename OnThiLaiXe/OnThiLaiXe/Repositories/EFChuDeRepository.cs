using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public class EFChuDeRepository : IChuDeRepository
    {
        private readonly ApplicationDbContext _context;

        public EFChuDeRepository(ApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<IEnumerable<ChuDe>> GetAllAsync()
        {
            // return await _context.Products.ToListAsync(); 
            return await _context.ChuDes.ToListAsync();
        }

        public async Task<ChuDe> GetByIdAsync(int id)
        {
            // return await _context.Products.FindAsync(id); 
            // lấy thông tin kèm theo category 
            return await _context.ChuDes.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(ChuDe chude)
        {
            _context.ChuDes.Add(chude);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ChuDe chude)
        {
            _context.ChuDes.Update(chude);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var chude = await _context.ChuDes.FindAsync(id);
            _context.ChuDes.Remove(chude);
            await _context.SaveChangesAsync();
        }
    }
}
