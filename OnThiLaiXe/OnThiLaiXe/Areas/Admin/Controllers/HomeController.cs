using Microsoft.AspNetCore.Mvc;
using OnThiLaiXe.Models;
using OnThiLaiXe.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnThiLaiXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IGiaoDichRepository _giaoDichRepository;
        private readonly IVisitLogRepository _visitLogRepository;

        public HomeController(
            IUserRepository userRepository,
            IGiaoDichRepository giaoDichRepository,
            IVisitLogRepository visitLogRepository)
        {
            _userRepository = userRepository;
            _giaoDichRepository = giaoDichRepository;
            _visitLogRepository = visitLogRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy giờ Việt Nam (UTC+7)
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var currentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);  // Sử dụng giờ UTC và chuyển về giờ VN
            var currentMonth = currentDate.Month;
            var currentYear = currentDate.Year;

            // Lấy số người dùng đăng ký trong tháng
            var newUserCount = await _giaoDichRepository.GetNewUserCountInMonthAsync(currentMonth, currentYear);

            // Lấy tổng doanh thu trong tháng
            var monthlyRevenue = await _giaoDichRepository.GetTotalAmountInMonthAsync(currentMonth, currentYear);

            // Lấy doanh thu theo tháng trong năm hiện tại
            var revenueByMonth = await _giaoDichRepository.GetRevenueByMonthAsync(currentYear);

            // Lấy 10 giao dịch gần nhất
            var recentTransactions = await _giaoDichRepository.GetRecentTransactionsAsync(10);

            // Lấy số người dùng truy cập trong 5 phút gần đây
            var currentVisitors = await _visitLogRepository.GetCurrentVisitorsCountAsync();

            // Lấy số lượt truy cập trong tháng
            var monthlyVisitors = await _visitLogRepository.GetMonthVisitorsCountAsync();

            // Lấy tổng số lượt truy cập trong năm
            var totalVisitors = await _visitLogRepository.GetYearVisitorsCountAsync();

            // Lấy số lượt truy cập theo tháng trong năm hiện tại
            var visitsByMonth = await _visitLogRepository.GetVisitsByMonthAsync(currentYear);

            ViewBag.NewUserCount = newUserCount;
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.RevenueByMonth = revenueByMonth;
            ViewBag.RecentTransactions = recentTransactions;
            ViewBag.CurrentMonth = currentDate.ToString("MM/yyyy");

            ViewBag.CurrentVisitors = currentVisitors;
            ViewBag.TotalVisitors = totalVisitors;
            ViewBag.MonthlyVisitors = monthlyVisitors;
            ViewBag.VisitsByMonth = visitsByMonth;

            return View();
        }
    }
}