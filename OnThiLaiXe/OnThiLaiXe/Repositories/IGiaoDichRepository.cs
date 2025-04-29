using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public interface IGiaoDichRepository
    {
        Task<IEnumerable<GiaoDich>> GetAllAsync();
        Task<GiaoDich> GetByIdAsync(int id);
        Task<IEnumerable<GiaoDich>> GetGiaoDichByUserIdAsync(string userId);
        Task<decimal> GetTotalAmountInMonthAsync(int month, int year);
        Task<Dictionary<string, decimal>> GetRevenueByMonthAsync(int year);
        Task<int> GetNewUserCountInMonthAsync(int month, int year);
        Task<IEnumerable<GiaoDich>> GetRecentTransactionsAsync(int count = 10);
        Task AddAsync(GiaoDich giaoDich);
        Task UpdateAsync(GiaoDich giaoDich);
        Task DeleteAsync(int id);
    }
}