using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public interface ICauHoiRepository
    {
        Task<IEnumerable<CauHoi>> GetAllAsync();
        Task<CauHoi> GetByIdAsync(int id);
        Task AddAsync(CauHoi cauhoi);
        Task UpdateAsync(CauHoi cauhoi);
        Task DeleteAsync(int id);
        List<CauHoi> LayCauHoiTheoChuDe(int chuDeId, int soLuong);

    }
}
