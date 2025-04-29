namespace OnThiLaiXe.Repositories
{
    public interface IVisitLogRepository
    {
        Task<int> GetTodayVisitorsCountAsync();
        Task<int> GetMonthVisitorsCountAsync();
        Task<int> GetYearVisitorsCountAsync();
        Task<int> GetYesterdayVisitorsCountAsync();
        Task<int> GetLastWeekVisitorsCountAsync();
        Task<int> GetLastMonthVisitorsCountAsync();
        Task<int> GetCurrentVisitorsCountAsync();
        Task<Dictionary<string, int>> GetVisitsByMonthAsync(int year);
    }
}
