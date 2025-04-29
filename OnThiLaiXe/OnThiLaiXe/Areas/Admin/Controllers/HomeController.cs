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
            var currentDate = DateTime.Now;
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

            var currentVisitors = await _visitLogRepository.GetCurrentVisitorsCountAsync();
            var monthlyVisitors = await _visitLogRepository.GetMonthVisitorsCountAsync();
            var totalVisitors = await _visitLogRepository.GetYearVisitorsCountAsync();
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