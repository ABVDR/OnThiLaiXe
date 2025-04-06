using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public interface IBaiThiRepository
    {
        BaiThi GetById(int id);
        IEnumerable<BaiThi> GetAll();
        void Add(BaiThi baiThi);
        void Update(BaiThi baiThi);
        void Delete(int id);
    }
}
