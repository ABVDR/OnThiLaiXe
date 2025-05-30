using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;
using OnThiLaiXe.Repositories;

namespace OnThiLaiXe.Controllers
{
    public class LichSuThiController : Controller
    {
       
        private readonly ApplicationDbContext _context;

        public LichSuThiController( ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ChiTiet(int id)
        {
            // Lấy chi tiết lịch sử thi với các câu hỏi và chi tiết câu trả lời
            var lichSuThi = _context.LichSuThis

                                    .Include(ls => ls.ChiTietLichSuThis)  // Bao gồm Chi tiết lịch sử thi
                                    .ThenInclude(ct => ct.CauHoi)  // Bao gồm Câu hỏi trong chi tiết
                                    .FirstOrDefault(ls => ls.Id == id);  // Sử dụng Id của lịch sử thi, không phải BaiThiId

            // Nếu không tìm thấy lịch sử thi theo Id
            if (lichSuThi == null)
            {
                return NotFound();  // Trả về lỗi nếu không tìm thấy
            }

            // Trả về View với model là lichSuThi
            return View(lichSuThi);
        }


        public IActionResult LichSuThi()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            string userId = currentUserId;
            var lichSuThi = _context.LichSuThis
                                     .Where(c => c.UserId == userId)
                                     .OrderByDescending(c => c.NgayThi)
                                     .ToList();

            Console.WriteLine($"Total records found: {lichSuThi.Count}"); // Kiểm tra số lượng bản ghi tìm được

            return View(lichSuThi);
        }





        // FEATURE 5: Xem lại các câu đã sai
        public IActionResult DanhSachCauHoiSai()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }


            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            string userId = currentUserId;


            var cauHoiSaiList = _context.CauHoiSais
                .Where(c => c.UserId == userId)
                .Include(c => c.CauHoi)
                .ThenInclude(ch => ch.ChuDe)
                .GroupBy(c => c.CauHoiId)
                .Select(g => new CauHoiSaiViewModel
                {
                    CauHoi = g.First().CauHoi,
                    SoLanSai = g.Count(),
                    LanSaiGanNhat = g.Max(c => c.NgaySai)
                })
                .OrderByDescending(c => c.LanSaiGanNhat)
                .ToList();

            return View(cauHoiSaiList);
        }

        // Action để luyện lại câu sai cho người dùng đã đăng nhập
        public IActionResult LuyenLaiCauSai()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            string userId = currentUserId;


            // ✅ THÊM ĐOẠN KIỂM TRA ĐÃ THANH TOÁN HAY CHƯA
            var daThanhToan = _context.GiaoDichs
                                .Any(g => g.UserId == currentUserId && g.DaThanhToan == true);

            if (!daThanhToan)
            {
                return RedirectToAction("ThanhToanTruoc", "GiaoDich");
            }

            // ✅ LẤY CÂU HỎI SAI SAU KHI ĐÃ XÁC NHẬN THANH TOÁN
            var cauHois = _context.CauHoiSais
                          .Where(c => c.UserId == userId)
                          .Select(c => c.CauHoi)
                          .ToList();

            var uniqueCauHois = cauHois
                        .GroupBy(c => c.Id)
                        .Select(g => g.First())
                        .ToList();

            return View(uniqueCauHois);
        }


        [HttpPost]
        public IActionResult LuuKetQuaLuyenLai(Dictionary<string, string> cauHoiAnswers)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string userId = currentUserId;

            var results = new List<KetQuaLuyenTapViewModel>();  // Danh sách lưu kết quả luyện lại

            foreach (var item in cauHoiAnswers)
            {
                var cauHoiId = item.Key.Replace("cauHoi_", "");
                var dapAn = item.Value;

                // Cập nhật dữ liệu vào database
                var cauHoiSai = _context.CauHoiSais
                    .Where(c => c.UserId == userId && c.CauHoiId == int.Parse(cauHoiId))
                    .ToList();  // Lấy tất cả câu sai có cùng ID

                bool isCorrect = false;
                var cauHoi = _context.CauHois.FirstOrDefault(c => c.Id == int.Parse(cauHoiId));
                if (cauHoi != null)
                {
                    if (dapAn == cauHoi.DapAnDung.ToString())
                        isCorrect = true;
                }

                // Thêm kết quả vào danh sách
                results.Add(new KetQuaLuyenTapViewModel
                {
                    CauHoiId = int.Parse(cauHoiId),
                    NoiDung = cauHoi?.NoiDung,
                    LuaChonA = cauHoi?.LuaChonA,
                    LuaChonB = cauHoi?.LuaChonB,
                    LuaChonC = cauHoi?.LuaChonC,
                    LuaChonD = cauHoi?.LuaChonD,
                    DapAnDung = cauHoi?.DapAnDung.ToString(),
                    DapAnNguoiDung = dapAn,
                    IsCorrect = isCorrect
                });

                if (isCorrect)
                {
                    // Xóa tất cả câu sai có cùng ID
                    if (cauHoiSai.Any())
                    {
                        _context.CauHoiSais.RemoveRange(cauHoiSai);
                    }
                }
                else
                {
                    // Cập nhật ngày sai hoặc thêm mới
                    if (cauHoiSai.Any())
                    {
                        foreach (var itemSai in cauHoiSai)
                        {
                            itemSai.NgaySai = DateTime.Now;
                        }
                    }
                    else
                    {
                        _context.CauHoiSais.Add(new CauHoiSai
                        {
                            UserId = userId,
                            CauHoiId = int.Parse(cauHoiId),
                            NgaySai = DateTime.Now
                        });
                    }
                }
            }

            _context.SaveChanges();

            // Trả về kết quả luyện lại
            return View("KetQuaLuyenLai", results);
        }






        private void SaveCauHoiSai(bool isLoggedIn, string currentUserId, int cauHoiId)
        {
            if (!isLoggedIn || string.IsNullOrEmpty(currentUserId)) return;

            try
            {
                string userId = currentUserId;

                _context.CauHoiSais.Add(new CauHoiSai
                {
                    UserId = userId,
                    CauHoiId = cauHoiId,
                    NgaySai = DateTime.Now
                });


            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw exception để tiếp tục xử lý
                Console.WriteLine($"Lỗi khi lưu câu hỏi sai: {ex.Message}");
            }
        }



      


    }
}
