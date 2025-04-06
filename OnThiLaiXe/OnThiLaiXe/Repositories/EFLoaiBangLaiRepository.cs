using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public class EFLoaiBangLaiRepository : ILoaiBangLaiRepository
    {
        private readonly ApplicationDbContext _context;
        public EFLoaiBangLaiRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(LoaiBangLai loaibanglai)
        {
            _context.LoaiBangLais.Add(loaibanglai);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var loaibanglai = await _context.LoaiBangLais.FindAsync(id);
            _context.LoaiBangLais.Remove(loaibanglai);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<LoaiBangLai>> GetAllAsync()
        {
            return await _context.LoaiBangLais.ToListAsync();
        }

        public async Task<LoaiBangLai> GetByIdAsync(int id)
        {
            return await _context.LoaiBangLais.FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task UpdateAsync(LoaiBangLai loaibanglai)
        {
            _context.LoaiBangLais.Update(loaibanglai);
            await _context.SaveChangesAsync();
        }
    }
}
