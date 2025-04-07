using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;
using OnThiLaiXe.Repositories;

namespace OnThiLaiXe.Controllers
{
    [Authorize]
    public class BaiThiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICauHoiRepository _cauHoiRepo;

        public BaiThiController(ApplicationDbContext context, ICauHoiRepository cauHoiRepo)
        {
            _context = context;
            _cauHoiRepo = cauHoiRepo;
        }

    
      

        // FEATURE 4: Xem kết quả bài thi (Đã nâng cấp phương thức NopBaiThi)
        [HttpPost]
        public IActionResult NopBaiThi(int baiThiId, string dapAnJson)
        {
            Console.WriteLine($"Nộp bài với ID: {baiThiId}");

            if (baiThiId == 0)
                return BadRequest("baiThiId không hợp lệ.");

            var baiThi = _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                    .ThenInclude(ct => ct.CauHoi)
                .FirstOrDefault(bt => bt.Id == baiThiId);

            if (baiThi == null)
                return NotFound("Không tìm thấy bài thi.");

            // Kiểm tra nếu người dùng đăng nhập
            bool isLoggedIn = User.Identity != null && User.Identity.IsAuthenticated;
            string currentUserId = isLoggedIn ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

            // Parse JSON đáp án
            Dictionary<string, string> dapAnDict = new();
            try
            {
                if (!string.IsNullOrEmpty(dapAnJson))
                    dapAnDict = JsonSerializer.Deserialize<Dictionary<string, string>>(dapAnJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi parse JSON: {ex.Message}");
            }

            var chiTietList = baiThi.ChiTietBaiThis.ToList();
            int correctCount = 0;
            int wrongCount = 0;
            int unansweredCount = 0;
            bool saiDiemLiet = false;

            // Xử lý từng câu trả lời
            for (int i = 0; i < chiTietList.Count; i++)
            {
                string key = $"dapAn_{i}";
                var chiTiet = chiTietList[i];

                if (dapAnDict.ContainsKey(key) && !string.IsNullOrEmpty(dapAnDict[key]))
                {
                    char dapAn = dapAnDict[key][0];
                    chiTiet.CauTraLoi = dapAn;
                    chiTiet.DungSai = dapAn == chiTiet.CauHoi.DapAnDung;

                    if (chiTiet.DungSai == true)
                        correctCount++;
                    else
                    {
                        wrongCount++;
                        if (isLoggedIn)
                            SaveCauHoiSai(true, currentUserId, chiTiet.CauHoi.Id);
                    }

                    if (chiTiet.CauHoi.DiemLiet && dapAn != chiTiet.CauHoi.DapAnDung)
                        saiDiemLiet = true;
                }
                else
                {
                    chiTiet.CauTraLoi = '\0';
                    chiTiet.DungSai = false;
                    unansweredCount++;

                    if (chiTiet.CauHoi.DiemLiet)
                        saiDiemLiet = true;

                    if (isLoggedIn)
                        SaveCauHoiSai(true, currentUserId, chiTiet.CauHoi.Id);
                }
            }

            // 🔽 Lấy loại bằng để kiểm tra điều kiện đậu
            var loaiBang = baiThi.ChiTietBaiThis.FirstOrDefault()?.CauHoi?.LoaiBangLai;
            int diemToiThieu = loaiBang?.DiemToiThieu ?? 21;

            // 🔽 Mỗi câu đúng = 1 điểm, không tính điểm nếu sai câu điểm liệt
            int tongDiem = correctCount;

            baiThi.Diem = tongDiem;
            baiThi.MacLoiNghiemTrong = saiDiemLiet;
            baiThi.SoCauDung = correctCount;
            baiThi.SoCauSai = wrongCount;
            baiThi.SoCauChuaTraLoi = unansweredCount;
            baiThi.PhanTramDung = chiTietList.Count > 0 ? (double)correctCount / chiTietList.Count * 100 : 0;
            baiThi.KetQua = (tongDiem >= diemToiThieu && !saiDiemLiet) ? "Đạt" : "Không đạt";
            baiThi.DaHoanThanh = true;

            _context.SaveChanges();

            // Chuẩn bị dữ liệu hiển thị kết quả
            var ketQuaList = chiTietList.Select(ct => new KetQuaBaiThi
            {
                BaiThiId = baiThiId,
                CauHoiId = ct.CauHoi.Id,
                CauHoi = ct.CauHoi,
                CauTraLoi = ct.CauTraLoi ?? '\0',
                DungSai = ct.DungSai ?? false
            }).ToList();

            // Truyền thông tin tổng hợp sang View
            ViewBag.TongSoCau = chiTietList.Count;
            ViewBag.SoCauDung = correctCount;
            ViewBag.SoCauSai = wrongCount;
            ViewBag.SoCauChuaTraLoi = unansweredCount;
            ViewBag.PhanTramDung = baiThi.PhanTramDung;
            ViewBag.MacLoiNghiemTrong = saiDiemLiet;
            ViewBag.Diem = tongDiem;
            ViewBag.KetQua = baiThi.KetQua;
            ViewBag.BaiThiId = baiThiId;
            ViewBag.DiemToiThieu = diemToiThieu;

            return View("KetQuaBaiThi", ketQuaList);
        }

        private void SaveCauHoiSai(bool isLoggedIn, string currentUserId, int cauHoiId)
        {
            if (!isLoggedIn || string.IsNullOrEmpty(currentUserId)) return;

            try
            {
                int userId;
                if (int.TryParse(currentUserId, out userId))
                {
                    _context.CauHoiSais.Add(new CauHoiSai
                    {
                        UserId = userId,
                        CauHoiId = cauHoiId,
                        NgaySai = DateTime.Now
                    });
                }
                else if (Guid.TryParse(currentUserId, out var userGuid))
                {
                    // Sử dụng int.MaxValue & operation để tránh overflow 
                    // nếu hashcode là số âm
                    int hashUserId = userGuid.GetHashCode() & int.MaxValue;
                    _context.CauHoiSais.Add(new CauHoiSai
                    {
                        UserId = hashUserId,
                        CauHoiId = cauHoiId,
                        NgaySai = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw exception để tiếp tục xử lý
                Console.WriteLine($"Lỗi khi lưu câu hỏi sai: {ex.Message}");
            }
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

                int userId;
                // Thử chuyển đổi trực tiếp thành int
                if (int.TryParse(currentUserId, out userId))
                {
                    // Tiếp tục với userId đã parse
                }
                // Nếu thất bại, thử parse thành Guid rồi chuyển thành int
                else if (Guid.TryParse(currentUserId, out var userGuid))
                {
                    userId = userGuid.GetHashCode() & int.MaxValue;
                }
                else
                {
                    // Nếu không phải int hoặc Guid, chuyển hướng về trang login
                    return RedirectToAction("Login", "Account");
                }

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

                int userId;
                if (!int.TryParse(currentUserId, out userId) && Guid.TryParse(currentUserId, out var userGuid))
                {
                    userId = userGuid.GetHashCode() & int.MaxValue;
                }

                var lichSuThiList = _context.BaiThis
                    .Where(bt => bt.UserId == userId && bt.DaHoanThanh)
                    .Include(bt => bt.ChiTietBaiThis)
                    .OrderByDescending(bt => bt.NgayThi)
                    .Select(bt => new LichSuThiViewModel
                    {
                        BaiThiId = bt.Id,
                        TenBaiThi = bt.TenBaiThi,
                        NgayThi = bt.NgayThi,
                        LoaiBaiThi = bt.LoaiBaiThi,
                        TongSoCau = bt.ChiTietBaiThis.Count,
                        SoCauDung = bt.SoCauDung,
                        PhanTramDung = bt.PhanTramDung,
                        Diem = bt.Diem ?? 0,
                        KetQua = bt.KetQua,
                        MacLoiNghiemTrong = bt.MacLoiNghiemTrong
                    })
                    .ToList();

                return View(lichSuThiList);
            
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

            int userId;
            if (int.TryParse(currentUserId, out userId))
            {
                // OK
            }
            else if (Guid.TryParse(currentUserId, out var userGuid))
            {
                userId = userGuid.GetHashCode() & int.MaxValue;
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

            var cauHoiIds = _context.CauHoiSais
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.NgaySai)
                .Select(c => c.CauHoiId)
                .Distinct()
                .Take(20)
                .ToList();

            var cauHois = _context.CauHois
                .Where(c => cauHoiIds.Contains(c.Id))
                .ToList();

            return View("LuyenLaiCauSai", cauHois);
        }

        public IActionResult ChonDeThi()
        {
            ViewBag.DanhSachChuDe = _context.ChuDes.ToList();
            ViewBag.DanhSachLoaiBangLai = _context.LoaiBangLais.ToList();

            return View();
        }

        public IActionResult ChiTietBaiThi(int id)
        {
            var baiThi = _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                .ThenInclude(ct => ct.CauHoi)
                .FirstOrDefault(bt => bt.Id == id);

            if (baiThi == null)
            {
                return NotFound();
            }

            return View(baiThi);
        }

        public IActionResult DanhSachBaiThi()
        {
            var baiThis = _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis) // Load số câu hỏi trong bài thi
                .OrderByDescending(bt => bt.NgayThi)
                .ToList();

            return View(baiThis);
        }

        // Action để làm bài thi
        public IActionResult LamBaiThi(int id)
        {
            var baiThi = _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                .ThenInclude(ct => ct.CauHoi)
                  .ThenInclude(c => c.LoaiBangLai)
                .FirstOrDefault(bt => bt.Id == id);

            if (baiThi == null)
            {
                return NotFound();
            }

            return View(baiThi);
        }

        // API endpoint để nhận đáp án từ client thông qua AJAX
        [HttpPost]
        public IActionResult LuuDapAnTamThoi([FromBody] DapAnTamThoi request)
        {
            if (request == null || request.BaiThiId == 0)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                // Lưu đáp án tạm thời vào session hoặc cache nếu cần
                // Có thể sử dụng Redis, session, hoặc cache của ứng dụng

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        

        public IActionResult DanhSachDeThi(string loaiXe)
        {
            // Lấy các đề thi có loại xe tương ứng
            var danhSachDeThi = _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                    .ThenInclude(ct => ct.CauHoi)
                        .ThenInclude(c => c.LoaiBangLai) // Đảm bảo nạp dữ liệu loại bằng lái
                                                         // Lọc theo loại xe
                .ToList();

            return View(danhSachDeThi);
        }

        public IActionResult LoaiBangLaiXeMay()
        {
            // Lấy danh sách các loại bằng lái cho xe máy
            var danhSachLoaiBangLaiXeMay = _context.LoaiBangLais.Where(l => l.LoaiXe == "Xe máy").ToList();

            // Kiểm tra xem danh sách có null không
            if (danhSachLoaiBangLaiXeMay == null || !danhSachLoaiBangLaiXeMay.Any())
            {
                // Xử lý khi không có dữ liệu
                ViewBag.ErrorMessage = "Không tìm thấy loại bằng lái xe máy!";
                return View();
            }

            return View(danhSachLoaiBangLaiXeMay);
        }

        public IActionResult LoaiBangLaiOTo()
        {
            // Lấy danh sách các loại bằng lái cho ô tô
            var danhSachLoaiBangLaiOTo = _context.LoaiBangLais.Where(l => l.LoaiXe == "Xe oto").ToList();
            return View(danhSachLoaiBangLaiOTo);
        }

        public IActionResult ThiDe(int loaiBangLaiId)
        {
            // Lấy thông tin loại bằng lái
            var loaiBangLai = _context.LoaiBangLais.FirstOrDefault(l => l.Id == loaiBangLaiId);
            if (loaiBangLai == null)
            {
                return NotFound();
            }

            // Lấy các đề thi tương ứng với loại bằng lái
            var danhSachDeThi = _context.BaiThis
                .Where(bt => bt.ChiTietBaiThis.Any(ct => ct.CauHoi.LoaiBangLaiId == loaiBangLaiId))
                .Include(bt => bt.ChiTietBaiThis)
                .ThenInclude(ct => ct.CauHoi)
                .ToList();

            ViewBag.LoaiBangLai = loaiBangLai;
            return View(danhSachDeThi);
        }

        public IActionResult OnTap(int loaiBangLaiId)
        {
            var cauHoiList = _context.CauHois
                .Where(c => c.LoaiBangLaiId == loaiBangLaiId)
                .OrderBy(c => c.ChuDeId)
                .ToList();

            if (!cauHoiList.Any())
            {
                TempData["Error"] = "Không có câu hỏi nào cho loại bằng lái này.";
                return RedirectToAction("DanhSachLoaiBangLai");
            }

            ViewBag.LoaiBangLai = _context.LoaiBangLais.FirstOrDefault(l => l.Id == loaiBangLaiId);
            return View(cauHoiList);
        }

        public IActionResult DanhSachLoaiBangLai()
        {
            var danhSachLoaiBangLai = _context.LoaiBangLais.ToList();
            return View(danhSachLoaiBangLai); // Truyền model thay vì ViewBag
        }
     

    }
}