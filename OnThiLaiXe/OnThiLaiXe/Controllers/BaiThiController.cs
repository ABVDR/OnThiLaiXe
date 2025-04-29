using System.Security.Claims;
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
        // Action để làm bài thi
        public IActionResult LamBaiThi(int id, Dictionary<int, string> answers)
        {
            var baiThi = _context.BaiThis
                                 .Include(b => b.ChiTietBaiThis)
                                 .ThenInclude(ct => ct.CauHoi)
                                 .ThenInclude(b => b.LoaiBangLai)
                                 .FirstOrDefault(b => b.Id == id);

            if (baiThi == null)
            {
                return NotFound();
            }

            int totalCorrectAnswers = 0;
            List<KetQuaBaiThi> ketQuaList = new List<KetQuaBaiThi>();

            // Kiểm tra từng câu hỏi
            foreach (var chiTiet in baiThi.ChiTietBaiThis)
            {
                var userAnswer = answers.ContainsKey(chiTiet.Id) ? answers[chiTiet.Id] : null;

                // Nếu userAnswer không rỗng, so sánh ký tự đầu tiên của userAnswer với DapAnDung
                var isCorrect = userAnswer != null && userAnswer.Length > 0 && chiTiet.CauHoi.DapAnDung.Equals(userAnswer[0]);

                ketQuaList.Add(new KetQuaBaiThi
                {
                    CauHoiId = chiTiet.CauHoiId,
                    CauTraLoi = userAnswer != null && userAnswer.Length > 0 ? userAnswer[0] : ' ', // Lấy ký tự đầu tiên hoặc gán một giá trị mặc định
                    DungSai = isCorrect
                });

                if (isCorrect)
                {
                    totalCorrectAnswers++;
                }
            }

            var diem = (totalCorrectAnswers / (float)baiThi.ChiTietBaiThis.Count) * 10; // Ví dụ tính điểm theo tỷ lệ

            ViewBag.KetQuaList = ketQuaList;
            ViewBag.TongSoCau = baiThi.ChiTietBaiThis.Count;
            ViewBag.DiemToiThieu = baiThi.ChiTietBaiThis
                                 .FirstOrDefault()?.CauHoi?.LoaiBangLai?.DiemToiThieu ?? 0;
            ViewBag.Diem = diem;

            return View(baiThi);
        }



        [HttpPost]
        public IActionResult NopBaiThiAjax([FromBody] SubmitBaiThiRequest request)
        {
            if (request == null || request.BaiThiId == 0 || request.Answers == null)
            {
                return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ!" });
            }

            var baiThi = _context.BaiThis
                                 .Include(b => b.ChiTietBaiThis)
                                 .ThenInclude(ct => ct.CauHoi)
                                 .ThenInclude(ch => ch.LoaiBangLai)
                                 .FirstOrDefault(b => b.Id == request.BaiThiId);

            if (baiThi == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài thi." });
            }

            int totalCorrectAnswers = 0;
            List<KetQuaBaiThi> ketQuaList = new List<KetQuaBaiThi>();

            foreach (var chiTiet in baiThi.ChiTietBaiThis)
            {
                var userAnswer = request.Answers.ContainsKey(chiTiet.CauHoiId)
                                   ? request.Answers[chiTiet.CauHoiId]
                                   : null;

                var isCorrect = false;

                // Kiểm tra nếu người dùng có câu trả lời
                if (!string.IsNullOrEmpty(userAnswer))
                {
                    isCorrect = chiTiet.CauHoi.DapAnDung.Equals(userAnswer[0]);
                }
                else
                {
                    // Nếu không có câu trả lời, thì là sai
                    isCorrect = false;
                }

                ketQuaList.Add(new KetQuaBaiThi
                {
                    CauHoiId = chiTiet.CauHoiId,
                    CauTraLoi = userAnswer != null ? userAnswer[0] : (char?)null,  // Nếu không có câu trả lời, để giá trị null
                    DapAnDung = chiTiet.CauHoi.DapAnDung,
                    DungSai = isCorrect == false,  // Đánh dấu sai nếu isCorrect là false
                });

                if (isCorrect)
                {
                    totalCorrectAnswers++;
                }
            }

            var tongSoCau = baiThi.ChiTietBaiThis.Count;
            var phanTramDung = (double)totalCorrectAnswers / tongSoCau * 100;
            int diem = (int)Math.Round(phanTramDung / 10);
            var diemToiThieu = baiThi.ChiTietBaiThis.FirstOrDefault()?.CauHoi?.LoaiBangLai?.DiemToiThieu ?? 0;
            var ketQua = diem >= diemToiThieu ? "Đậu" : "Rớt";
            bool macLoiNghiemTrong = ketQuaList
    .Any(kq => kq.DungSai && baiThi.ChiTietBaiThis
        .First(ct => ct.CauHoiId == kq.CauHoiId).CauHoi.DiemLiet);

            // Đếm số câu mắc lỗi nghiêm trọng
            int soCauLoiNghiemTrong = ketQuaList
                .Count(kq => kq.DungSai && baiThi.ChiTietBaiThis
                    .First(ct => ct.CauHoiId == kq.CauHoiId).CauHoi.DiemLiet);


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                var lichSuThi = new LichSuThi
                {
                    UserId = userId,
                    BaiThiId = baiThi.Id,
                    TenBaiThi = baiThi.TenBaiThi.Length > 100 ? baiThi.TenBaiThi.Substring(0, 100) : baiThi.TenBaiThi,
                    NgayThi = DateTime.Now,
                    TongSoCau = tongSoCau,
                    SoCauDung = totalCorrectAnswers,
                    PhanTramDung = phanTramDung,
                    Diem = diem,
                    KetQua = ketQua.Length > 20 ? ketQua.Substring(0, 20) : ketQua,
                    MacLoiNghiemTrong = macLoiNghiemTrong,
                };

                try
                {
                    _context.LichSuThis.Add(lichSuThi);
                    _context.SaveChanges();

                    foreach (var kq in ketQuaList)
                    {
                        _context.ChiTietLichSuThis.Add(new ChiTietLichSuThi
                        {
                            LichSuThiId = lichSuThi.Id,
                            CauHoiId = kq.CauHoiId,
                            CauTraLoi = kq.CauTraLoi,
                            DungSai = kq.DungSai
                        });

                        if (kq.DungSai)
                        {
                            var existingSai = _context.CauHoiSais
                                .FirstOrDefault(c => c.UserId == userId && c.CauHoiId == kq.CauHoiId);

                            if (existingSai != null)
                            {
                                existingSai.NgaySai = DateTime.Now;
                            }
                            else
                            {
                                _context.CauHoiSais.Add(new CauHoiSai
                                {
                                    UserId = userId,
                                    CauHoiId = kq.CauHoiId,
                                    NgaySai = DateTime.Now
                                });
                            }
                        }
                    }

                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Lỗi khi lưu lịch sử thi.",
                        error = ex.Message,
                        inner = ex.InnerException?.Message,
                        full = ex.ToString()
                    });
                }
            }

            return Json(new
            {
                success = true,
                baiThiId = baiThi.Id,
                ketQuaList = ketQuaList.Select(kq => new
                {
                    kq.CauHoiId,
                    kq.CauTraLoi,
                    kq.DapAnDung,
                    kq.DungSai,
                }),
                tongSoCau = tongSoCau,
                tongDiem = diem,
                ketQua = ketQua,
                macLoiNghiemTrong = macLoiNghiemTrong,
                soCauLoiNghiemTrong = soCauLoiNghiemTrong
            });

        }





        public class SubmitBaiThiRequest
        {
            public int BaiThiId { get; set; }
            public Dictionary<int, string> Answers { get; set; }
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




    }
}