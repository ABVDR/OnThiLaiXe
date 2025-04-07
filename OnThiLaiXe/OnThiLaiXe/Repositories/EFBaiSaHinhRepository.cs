using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public class EFBaiSaHinhRepository : IBaiSaHinhRepository
    {
        private readonly ApplicationDbContext _context;

        public EFBaiSaHinhRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BaiSaHinh>> GetAllBaiSaHinhAsync()
        {
            return await _context.BaiSaHinhs
                .Include(b => b.LoaiBangLai)
                .OrderBy(b => b.Order)
                .ToListAsync();
        }

        public async Task<BaiSaHinh> GetBaiSaHinhByIdAsync(int id)
        {
            return await _context.BaiSaHinhs
                .Include(b => b.LoaiBangLai)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddBaiSaHinhAsync(BaiSaHinh baiSaHinh)
        {
            var maxOrder = await _context.BaiSaHinhs
                .Where(b => b.LoaiBangLaiId == baiSaHinh.LoaiBangLaiId)
                .MaxAsync(b => (int?)b.Order) ?? 0;

            if (baiSaHinh.Order > maxOrder + 1 || baiSaHinh.Order < 1) baiSaHinh.Order = maxOrder + 1;

            var lessonsToShift = await _context.BaiSaHinhs
                .Where(b => b.LoaiBangLaiId == baiSaHinh.LoaiBangLaiId && b.Order >= baiSaHinh.Order)
                .ToListAsync();

            foreach (var lesson in lessonsToShift)
            {
                lesson.Order++;
            }

            _context.BaiSaHinhs.Add(baiSaHinh);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBaiSaHinhAsync(BaiSaHinh baiSaHinh)
        {
            var oldBai = await _context.BaiSaHinhs.AsNoTracking().FirstOrDefaultAsync(b => b.Id == baiSaHinh.Id);
            var maxOrder = await _context.BaiSaHinhs
                .Where(b => b.LoaiBangLaiId == baiSaHinh.LoaiBangLaiId)
                .MaxAsync(b => (int?)b.Order) ?? 0;

            if (baiSaHinh.Order > maxOrder || baiSaHinh.Order < 1) baiSaHinh.Order = maxOrder;

            if (oldBai.Order != baiSaHinh.Order)
            {
                if (baiSaHinh.Order > oldBai.Order)
                {
                    var lessonsToShiftDown = await _context.BaiSaHinhs
                        .Where(b => b.LoaiBangLaiId == baiSaHinh.LoaiBangLaiId && b.Order > oldBai.Order && b.Order <= baiSaHinh.Order)
                        .ToListAsync();

                    foreach (var lesson in lessonsToShiftDown)
                    {
                        lesson.Order--;
                    }
                }
                else
                {
                    var lessonsToShiftUp = await _context.BaiSaHinhs
                        .Where(b => b.LoaiBangLaiId == baiSaHinh.LoaiBangLaiId && b.Order >= baiSaHinh.Order && b.Order < oldBai.Order)
                        .ToListAsync();

                    foreach (var lesson in lessonsToShiftUp)
                    {
                        lesson.Order++;
                    }
                }
            }

            // Không thêm "Bài {Order}: " vào TenBai
            _context.BaiSaHinhs.Update(baiSaHinh);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBaiSaHinhAsync(int id)
        {
            var baiSaHinh = await _context.BaiSaHinhs.FindAsync(id);
            if (baiSaHinh != null)
            {
                int loaiBangLaiId = baiSaHinh.LoaiBangLaiId;
                int deletedOrder = baiSaHinh.Order;

                _context.BaiSaHinhs.Remove(baiSaHinh);

                var lessonsToShift = await _context.BaiSaHinhs
                    .Where(b => b.LoaiBangLaiId == loaiBangLaiId && b.Order > deletedOrder)
                    .ToListAsync();

                foreach (var lesson in lessonsToShift)
                {
                    lesson.Order--;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<BaiSaHinh>> GetBaiSaHinhByLoaiBangLaiIdAsync(int loaiBangLaiId)
        {
            return await _context.BaiSaHinhs
                .Include(b => b.LoaiBangLai)
                .Where(b => b.LoaiBangLaiId == loaiBangLaiId)
                .OrderBy(b => b.Order)
                .ToListAsync();
        }
    }
}