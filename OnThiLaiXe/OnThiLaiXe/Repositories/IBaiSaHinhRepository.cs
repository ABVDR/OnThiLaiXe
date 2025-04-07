using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public interface IBaiSaHinhRepository
    {
        Task<IEnumerable<BaiSaHinh>> GetAllBaiSaHinhAsync();
        Task<BaiSaHinh> GetBaiSaHinhByIdAsync(int id);
        Task AddBaiSaHinhAsync(BaiSaHinh baiSaHinh);
        Task UpdateBaiSaHinhAsync(BaiSaHinh baiSaHinh);
        Task DeleteBaiSaHinhAsync(int id);
        Task<IEnumerable<BaiSaHinh>> GetBaiSaHinhByLoaiBangLaiIdAsync(int loaiBangLaiId);
    }
}