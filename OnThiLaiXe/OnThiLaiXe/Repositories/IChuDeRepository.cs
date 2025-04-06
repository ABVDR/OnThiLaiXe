using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public interface IChuDeRepository
    {
        Task<IEnumerable<ChuDe>> GetAllAsync();
        Task<ChuDe> GetByIdAsync(int id);
        Task AddAsync(ChuDe chude);
        Task UpdateAsync(ChuDe chude);
        Task DeleteAsync(int id);
    }
}
