﻿using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;
using OnThiLaiXe.Repositories;

namespace OnThiLaiXe.Controllers
{
    public class BaiThiController : Controller
    {
        private readonly ICauHoiRepository _cauHoiRepo;
        private readonly IBaiThiRepository _baiThiRepo;
        private readonly ApplicationDbContext _context;

        public BaiThiController(ICauHoiRepository cauHoiRepo, IBaiThiRepository baiThiRepo, ApplicationDbContext context)
        {
            _cauHoiRepo = cauHoiRepo;
            _baiThiRepo = baiThiRepo;
            _context = context;
        }

        // FEATURE 4: Xem kết quả bài thi (Đã nâng cấp phương thức NopBaiThi)
        [HttpPost]
        public IActionResult NopBaiThi(int baiThiId, string dapAnJson)
        {
            Console.WriteLine($"Nộp bài với ID: {baiThiId}");

            if (baiThiId == 0)
                return BadRequest("baiThiId không hợp lệ.");

            // Kiểm tra nếu người dùng đăng nhập
            bool isLoggedIn = User.Identity != null && User.Identity.IsAuthenticated;
            string currentUserId = isLoggedIn ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

            var ketQuaList = _baiThiRepo.NopBaiThi(baiThiId, dapAnJson, currentUserId, isLoggedIn);

            if (ketQuaList == null)
                return NotFound("Không tìm thấy bài thi.");

            // Lấy thông tin bài thi để truyền cho view
            var baiThi = _baiThiRepo.GetBaiThiById(baiThiId);

            // Truyền thông tin tổng hợp sang View
            ViewBag.TongSoCau = baiThi.ChiTietBaiThis.Count;
            ViewBag.SoCauDung = baiThi.SoCauDung;
            ViewBag.SoCauSai = baiThi.SoCauSai;
            ViewBag.SoCauChuaTraLoi = baiThi.SoCauChuaTraLoi;
            ViewBag.PhanTramDung = baiThi.PhanTramDung;
            ViewBag.MacLoiNghiemTrong = baiThi.MacLoiNghiemTrong;
            ViewBag.Diem = baiThi.Diem;
            ViewBag.KetQua = baiThi.KetQua;
            ViewBag.BaiThiId = baiThiId;

            // Lấy điểm tối thiểu từ loại bằng
            var loaiBang = baiThi.ChiTietBaiThis.FirstOrDefault()?.CauHoi?.LoaiBangLai;
            ViewBag.DiemToiThieu = loaiBang?.DiemToiThieu ?? 21;

            // Xử lý userId từ currentUserId
            int userId;
            if (int.TryParse(currentUserId, out userId))
            {
                // Tiếp tục với userId đã parse thành int
            }
            else if (Guid.TryParse(currentUserId, out var userGuid))
            {
                // Chuyển Guid thành int bằng GetHashCode()
                userId = userGuid.GetHashCode() & int.MaxValue;  // Đảm bảo tránh overflow khi chuyển thành int
            }
            else
            {
                // Nếu không thể chuyển currentUserId thành int hoặc Guid, xử lý lỗi (không cần lưu lịch sử thi)
                return BadRequest("User ID không hợp lệ.");
            }

            // Tính toán kết quả thi
            var correctCount = baiThi.SoCauDung;  // Giả sử đã có số câu đúng
            var tongDiem = baiThi.Diem;  // Giả sử đã có điểm tổng
            var saiDiemLiet = baiThi.MacLoiNghiemTrong;  // Giả sử là flag khi sai điểm liệt

            // Lưu vào bảng LichSuThi
            var lichSuThi = new LichSuThi
            {
                BaiThiId = baiThi.Id,
                TenBaiThi = baiThi.TenBaiThi,
                NgayThi = DateTime.Now,
                LoaiBaiThi = baiThi.LoaiBaiThi,
                TongSoCau = baiThi.ChiTietBaiThis.Count,
                SoCauDung = correctCount,
                PhanTramDung = baiThi.PhanTramDung,
                Diem = tongDiem ?? 0,
                KetQua = baiThi.KetQua,
                MacLoiNghiemTrong = saiDiemLiet,
                // Lưu UserId dưới dạng int
                UserId = userId  // Lưu trực tiếp giá trị đã parse thành int
            };

            // Lưu lịch sử thi vào cơ sở dữ liệu
            _context.LichSuThis.Add(lichSuThi);
            _context.SaveChanges();



            // Tạo danh sách chi tiết lịch sử thi
            var chiTietLichSuList = baiThi.ChiTietBaiThis.Select(ct => new ChiTietLichSuThi
            {
                LichSuThiId = lichSuThi.Id,
                CauHoiId = ct.CauHoiId,
                CauTraLoi = ct.CauTraLoi?.ToString(), // Convert char? to string
                DungSai = ct.DungSai
            }).ToList();

            // Lưu chi tiết lịch sử
            _context.ChiTietLichSuThis.AddRange(chiTietLichSuList);
            _context.SaveChanges();

            return View("KetQuaBaiThi", ketQuaList);
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

            int userId;
            if (int.TryParse(currentUserId, out userId))
            {
                Console.WriteLine($"UserId: {userId}");
            }
            else if (Guid.TryParse(currentUserId, out var userGuid))
            {
                userId = userGuid.GetHashCode() & int.MaxValue;
                Console.WriteLine($"UserId (from Guid): {userId}");
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

            var lichSuThi = _context.LichSuThis
                                    .Where(c => c.UserId == userId)
                                    .OrderByDescending(c => c.NgayThi)
                                    .ToList();

            Console.WriteLine($"Total records found: {lichSuThi.Count}"); // Kiểm tra số lượng bản ghi tìm được

            return View(lichSuThi);
        }




        public IActionResult ChonDeThi()
        {
            ViewBag.DanhSachChuDe = _baiThiRepo.GetDanhSachChuDe();
            ViewBag.DanhSachLoaiBangLai = _baiThiRepo.GetDanhSachLoaiBangLai();

            return View();
        }


        public IActionResult ChiTietBaiThi(int id)
        {
            var baiThi = _baiThiRepo.GetChiTietBaiThi(id);

            if (baiThi == null)
            {
                return NotFound();
            }

            return View(baiThi);
        }

        public IActionResult DanhSachBaiThi()
        {
            var baiThis = _baiThiRepo.GetDanhSachBaiThi();
            return View(baiThis);
        }

        // Action để làm bài thi
        public IActionResult LamBaiThi(int id)
        {
            var baiThi = _baiThiRepo.GetBaiThiById(id);

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
                bool result = _baiThiRepo.LuuDapAnTamThoi(request);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        public IActionResult DanhSachDeThi(string loaiXe)
        {
            var danhSachDeThi = _baiThiRepo.GetDanhSachDeThi(loaiXe);
            return View(danhSachDeThi);
        }

        public IActionResult LoaiBangLaiXeMay()
        {
            var danhSachLoaiBangLaiXeMay = _baiThiRepo.GetLoaiBangLaiXeMay();

            if (danhSachLoaiBangLaiXeMay == null || !danhSachLoaiBangLaiXeMay.Any())
            {
                ViewBag.ErrorMessage = "Không tìm thấy loại bằng lái xe máy!";
                return View();
            }

            return View(danhSachLoaiBangLaiXeMay);
        }

        public IActionResult LoaiBangLaiOTo()
        {
            var danhSachLoaiBangLaiOTo = _baiThiRepo.GetLoaiBangLaiOTo();
            return View(danhSachLoaiBangLaiOTo);
        }


        public IActionResult ThiDe(int loaiBangLaiId)
        {
            var loaiBangLai = _baiThiRepo.GetLoaiBangLaiById(loaiBangLaiId);
            if (loaiBangLai == null)
            {
                return NotFound();
            }

            var danhSachDeThi = _baiThiRepo.GetDeThiByLoaiBangLai(loaiBangLaiId);

            ViewBag.LoaiBangLai = loaiBangLai;
            return View(danhSachDeThi);
        }

        public IActionResult OnTap(int loaiBangLaiId)
        {
            var cauHoiList = _baiThiRepo.GetCauHoiOnTap(loaiBangLaiId);

            if (!cauHoiList.Any())
            {
              
                return RedirectToAction("DanhSachLoaiBangLai");
            }

            ViewBag.LoaiBangLai = _baiThiRepo.GetLoaiBangLaiById(loaiBangLaiId);
            return View(cauHoiList);
        }

        public IActionResult OnTapChuDe(int loaiBangLaiId, int chuDeId)
        {
            var cauHoiList = _baiThiRepo.GetCauHoiTheoChuDe(loaiBangLaiId, chuDeId);

            if (!cauHoiList.Any())
            {
                TempData["Error"] = "Không có câu hỏi nào cho chủ đề này.";
                return RedirectToAction("OnTap", new { loaiBangLaiId });
            }

            ViewBag.LoaiBangLai = _baiThiRepo.GetLoaiBangLaiById(loaiBangLaiId);
            ViewBag.TenChuDe = _baiThiRepo.GetTenChuDeById(chuDeId);

            return View("OnTapChuDe", cauHoiList); // dùng lại View hiện có
        }


        public IActionResult ChonChuDe(int loaiBangLaiId)
        {
            var loai = _context.LoaiBangLais.FirstOrDefault(l => l.Id == loaiBangLaiId);
            if (loai == null)
            {
                return NotFound();
            }

            var chuDeList = _context.ChuDes
                .Include(cd => cd.CauHois)
                .Where(cd => cd.CauHois.Any(ch => ch.LoaiBangLaiId == loaiBangLaiId))
                .Distinct()
                .ToList();

            ViewBag.TenLoai = loai.TenLoai;
            ViewBag.LoaiBangLaiId = loaiBangLaiId;

            return View("ChonChuDe", chuDeList);
        }



        public IActionResult DanhSachLoaiBangLai()
        {
            var danhSachLoaiBangLai = _baiThiRepo.GetDanhSachLoaiBangLai();
            return View(danhSachLoaiBangLai);
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
            int userId = int.TryParse(currentUserId, out var uid) ? uid : (Guid.TryParse(currentUserId, out var guid) ? guid.GetHashCode() & int.MaxValue : -1);
            if (userId == -1) return Unauthorized();

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




    }
}