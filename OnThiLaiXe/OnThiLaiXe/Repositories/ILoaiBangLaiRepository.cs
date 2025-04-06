using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public interface ILoaiBangLaiRepository
    {
        Task<IEnumerable<LoaiBangLai>> GetAllAsync();
        Task<LoaiBangLai> GetByIdAsync(int id);
        Task AddAsync(LoaiBangLai loaibanglai);
        Task UpdateAsync(LoaiBangLai loaibanglai);
        Task DeleteAsync(int id);

    }
}
