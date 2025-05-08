using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public interface IMoPhongRepository
    {
        Task<IEnumerable<MoPhong>> GetAllAsync();
        Task<MoPhong> GetByIdAsync(int id);
        Task<MoPhong> AddAsync(MoPhong moPhong);
        Task UpdateAsync(MoPhong moPhong);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<MoPhong>> GetOrderedAsync();
    }
}
