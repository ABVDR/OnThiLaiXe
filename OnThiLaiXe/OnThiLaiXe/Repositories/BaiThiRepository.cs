using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public class BaiThiRepository : IBaiThiRepository
    {
        private readonly ApplicationDbContext _context;

        public BaiThiRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public BaiThi GetById(int id)
        {
            return _context.BaiThis.Find(id);
        }

        public IEnumerable<BaiThi> GetAll()
        {
            return _context.BaiThis.ToList();
        }

        public void Add(BaiThi baiThi)
        {
            _context.BaiThis.Add(baiThi);
            _context.SaveChanges();
        }

        public void Update(BaiThi baiThi)
        {
            _context.BaiThis.Update(baiThi);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var baiThi = _context.BaiThis.Find(id);
            if (baiThi != null)
            {
                _context.BaiThis.Remove(baiThi);
                _context.SaveChanges();
            }
        }
    }
}