using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OnThiLaiXe.Models;
using OnThiLaiXe.Repositories;

namespace OnThiLaiXe.Controllers
{
    public class BaiThiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICauHoiRepository _cauHoiRepo;

        public BaiThiController(ApplicationDbContext context, ICauHoiRepository cauHoiRepo)
        {
            _context = context;
            _cauHoiRepo = cauHoiRepo;
            
        }

        // FEATURE 3: Làm bài ôn tập theo chủ đề
        public IActionResult OnTapTheoChuDe(int chuDeId, int loaiBangLaiId)
        {
            var cauHoiList = _context.CauHois
                .Where(c => c.ChuDeId == chuDeId && c.LoaiBangLaiId == loaiBangLaiId)
                .OrderBy(r => Guid.NewGuid())
                .ToList();

            if (!cauHoiList.Any())
            {
                TempData["Error"] = "Không có câu hỏi nào cho chủ đề và loại bằng lái này.";
                return RedirectToAction("OnTapTheoChuDeVaLoaiBangLai", new { loaiBangLaiId = loaiBangLaiId });
            }

            ViewBag.ChuDe = _context.ChuDes.FirstOrDefault(c => c.Id == chuDeId);
            ViewBag.LoaiBangLai = _context.LoaiBangLais.FirstOrDefault(l => l.Id == loaiBangLaiId);
            return View(cauHoiList);
        }

        // FEATURE 3: Lọc câu hỏi theo loại (biển báo, sa hình, ...)
        //public IActionResult OnTapTheoLoai(string loaiCauHoi, int loaiBangLaiId)
        //{
        //    // Lấy danh sách câu hỏi theo loại và loại bằng lái
        //    var cauHoiList = _context.CauHois
        //        .Where(c => c.LoaiCauHoi == loaiCauHoi && c.LoaiBangLaiId == loaiBangLaiId)
        //        .OrderBy(r => Guid.NewGuid())
        //        .ToList();

        //    if (!cauHoiList.Any())
        //    {
        //        TempData["Error"] = $"Không có câu hỏi loại '{loaiCauHoi}' cho loại bằng lái này.";
        //        return RedirectToAction("OnTapTheoChuDeVaLoaiBangLai", new { loaiBangLaiId = loaiBangLaiId });
        //    }

        //    ViewBag.LoaiCauHoi = loaiCauHoi;
        //    ViewBag.LoaiBangLai = _context.LoaiBangLais.FirstOrDefault(l => l.Id == loaiBangLaiId);
        //    return View("OnTapTheoLoai", cauHoiList);
        //}

        // FEATURE 4: Xem kết quả bài thi (Đã nâng cấp phương thức NopBaiThi)
        [HttpPost]
        public IActionResult NopBaiThi(int baiThiId, string dapAnJson)
        {


            Console.WriteLine($"Nộp bài với ID: {baiThiId}");

            if (baiThiId == 0)
            {
                return BadRequest("baiThiId không hợp lệ.");
            }

            var baiThi = _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                .ThenInclude(ct => ct.CauHoi)
                .FirstOrDefault(bt => bt.Id == baiThiId);

            if (baiThi == null)
            {
                return NotFound("Không tìm thấy bài thi.");
            }

            // Kiểm tra nếu người dùng đang đăng nhập
            bool isLoggedIn = User.Identity != null && User.Identity.IsAuthenticated;
            string currentUserId = isLoggedIn ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

            // Parse JSON dap an
            Dictionary<string, string> dapAnDict = new Dictionary<string, string>();
            try
            {
                if (!string.IsNullOrEmpty(dapAnJson))
                {
                    dapAnDict = JsonSerializer.Deserialize<Dictionary<string, string>>(dapAnJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi parse JSON: {ex.Message}");
                // Tiếp tục với dictionary trống
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
                    {
                        correctCount++;
                    }
                    else
                    {
                        wrongCount++;
                        // Nếu trả lời sai và người dùng đã đăng nhập thì lưu vào bảng CauHoiSai
                        SaveCauHoiSai(isLoggedIn, currentUserId, chiTiet.CauHoi.Id);
                    }

                    if (chiTiet.CauHoi.DiemLiet && dapAn != chiTiet.CauHoi.DapAnDung)
                    {
                        saiDiemLiet = true;
                    }
                }
                else
                {
                    chiTiet.CauTraLoi = '\0';
                    chiTiet.DungSai = false;
                    unansweredCount++;

                    if (chiTiet.CauHoi.DiemLiet)
                    {
                        saiDiemLiet = true;
                    }

                    // Lưu câu hỏi chưa trả lời
                    SaveCauHoiSai(isLoggedIn, currentUserId, chiTiet.CauHoi.Id);
                }
            }

            // Tính điểm và cập nhật kết quả bài thi
            double diemMoiCau = 10.0 / chiTietList.Count;
            int tongDiem = saiDiemLiet ? 0 : (int)Math.Round(correctCount * diemMoiCau);

            baiThi.Diem = tongDiem;
            baiThi.MacLoiNghiemTrong = saiDiemLiet;
            baiThi.SoCauDung = correctCount;
            baiThi.SoCauSai = wrongCount;
            baiThi.SoCauChuaTraLoi = unansweredCount;
            baiThi.PhanTramDung = chiTietList.Count > 0 ? (double)correctCount / chiTietList.Count * 100 : 0;
            baiThi.KetQua = tongDiem >= 8 && !saiDiemLiet ? "Đạt" : "Không đạt";
            baiThi.DaHoanThanh = true;

            _context.SaveChanges();

            // Chuẩn bị dữ liệu cho view kết quả
            var ketQuaList = chiTietList.Select(ct => new KetQuaBaiThi
            {
                BaiThiId = baiThiId,
                CauHoiId = ct.CauHoi.Id,
                CauHoi = ct.CauHoi,
                CauTraLoi = ct.CauTraLoi ?? '\0',
                DungSai = ct.DungSai ?? false
            }).ToList();

            // Thêm thông tin tổng hợp kết quả
            ViewBag.TongSoCau = chiTietList.Count;
            ViewBag.SoCauDung = correctCount;
            ViewBag.SoCauSai = wrongCount;
            ViewBag.SoCauChuaTraLoi = unansweredCount;
            ViewBag.PhanTramDung = chiTietList.Count > 0 ? (double)correctCount / chiTietList.Count * 100 : 0;
            ViewBag.MacLoiNghiemTrong = saiDiemLiet;
            ViewBag.Diem = tongDiem;
            ViewBag.KetQua = tongDiem >= 8 && !saiDiemLiet ? "Đạt" : "Không đạt";
            ViewBag.BaiThiId = baiThiId;

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



        // FEATURE 6: Xem lịch sử thi
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



        // Các phương thức hiện có của bạn
        [HttpPost]
        public IActionResult TaoDeThi(int loaiBangLaiId, Dictionary<int, int> soLuongMoiChuDe)
        {
            if (soLuongMoiChuDe == null || !soLuongMoiChuDe.Any())
            {
                TempData["Error"] = "Số lượng câu hỏi theo chủ đề không hợp lệ.";
                return RedirectToAction("ChonDeThi");
            }

            if (!_context.LoaiBangLais.Any(lb => lb.Id == loaiBangLaiId))
            {
                TempData["Error"] = "Loại bằng lái không hợp lệ.";
                return RedirectToAction("ChonDeThi");
            }

            var danhSachCauHoi = new List<CauHoi>();

            foreach (var chuDe in soLuongMoiChuDe)
            {
                int chuDeId = chuDe.Key;
                int soLuong = chuDe.Value;

                // Lấy câu hỏi theo chủ đề và loại bằng lái
                var cauHoiTheoChuDe = _context.CauHois
                    .Where(c => c.LoaiBangLaiId == loaiBangLaiId && c.ChuDeId == chuDeId)
                    .OrderBy(r => Guid.NewGuid()) // Lấy ngẫu nhiên
                    .Take(soLuong)
                    .ToList();

                danhSachCauHoi.AddRange(cauHoiTheoChuDe);
            }

            if (danhSachCauHoi.Count == 0)
            {
                TempData["Error"] = "Không đủ câu hỏi để tạo đề thi.";
                return RedirectToAction("ChonDeThi");
            }

            try
            {
                var deThi = new BaiThi
                {
                    NgayThi = DateTime.Now,
                    TenBaiThi = $"Đề thi chính thức - {DateTime.Now:dd/MM/yyyy HH:mm}",
                    LoaiBaiThi = "Đề thi chính thức",
                    ChiTietBaiThis = danhSachCauHoi.Select(c => new ChiTietBaiThi { CauHoiId = c.Id }).ToList()
                };

                // Nếu người dùng đăng nhập, lưu thông tin người dùng
                if (User.Identity.IsAuthenticated)
                {
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    deThi.UserId = int.Parse(currentUserId);
                }

                _context.BaiThis.Add(deThi);
                _context.SaveChanges();

                return RedirectToAction("ChiTietBaiThi", new { id = deThi.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tạo đề thi: " + ex.Message;
                return RedirectToAction("ChonDeThi");
            }
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

        // Action để luyện lại câu sai cho người dùng đã đăng nhập
        public IActionResult LuyenLaiCauSai()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return RedirectToAction("Login", "Account");

            var cauHoiIds = _context.CauHoiSais
                .Where(c => c.UserId == int.Parse(currentUserId))
                .OrderByDescending(c => c.NgaySai)
                .Select(c => c.CauHoiId)
                .Distinct()
                .Take(20)
                .ToList();

            var cauHois = _context.CauHois
                .Where(c => cauHoiIds.Contains(c.Id))
                .ToList();

            return View("LamLaiCauSai", cauHois);
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

        [HttpPost]
        public IActionResult XoaBaiThi(int id)
        {
            var baiThi = _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                .FirstOrDefault(bt => bt.Id == id);

            if (baiThi == null)
            {
                return NotFound();
            }

            _context.BaiThis.Remove(baiThi);
            _context.SaveChanges();

            return RedirectToAction("DanhSachBaiThi");
        }

        //public IActionResult OnTapTheoChuDeVaLoaiBangLai(int? loaiBangLaiId)
        //{
        //    var danhSachChuDe = _context.ChuDes.ToList();
        //    var danhSachLoaiBangLai = _context.LoaiBangLais.ToList();

        //    // Thêm thông tin về các loại câu hỏi (biển báo, sa hình, ...)
        //    var loaiCauHoiList = _context.CauHois
        //        .Where(c => loaiBangLaiId == null || c.LoaiBangLaiId == loaiBangLaiId)
        //        .Select(c => c.LoaiCauHoi)
        //        .Distinct()
        //        .Where(l => !string.IsNullOrEmpty(l))
        //        .ToList();

        //    ViewBag.DanhSachLoaiBangLai = danhSachLoaiBangLai;
        //    ViewBag.SelectedLoaiBangLaiId = loaiBangLaiId;
        //    ViewBag.LoaiCauHoiList = loaiCauHoiList;

        //    return View(danhSachChuDe);
        //}
        // Hiển thị danh sách chia sẻ của tất cả người dùng
        // Hiển thị danh sách chia sẻ của tất cả người dùng
        [HttpGet]
        public async Task<IActionResult> Share(string? sortOrder, string? searchString)
        {
            var sharesQuery = _context.Shares.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                sharesQuery = sharesQuery.Where(s =>
                    s.Content.Contains(searchString) ||
                    (s.Topic != null && s.Topic.Contains(searchString)));
            }
            ViewBag.CurrentSort = sortOrder;
            sharesQuery = sortOrder == "oldest"
                ? sharesQuery.OrderBy(s => s.CreatedAt)
                : sharesQuery.OrderByDescending(s => s.CreatedAt);

            var shares = await sharesQuery.ToListAsync();
            ViewBag.ChuDeList = new List<string> { "GPLX", "Lý Thuyết", "Mô Phỏng", "Khác" };
            ViewBag.SearchString = searchString;

            return View(shares);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateShare(string Content, string? Topic)
        {
            if (string.IsNullOrEmpty(Content))
            {
                TempData["Error"] = "Nội dung không được để trống.";
                return RedirectToAction(nameof(Share));
            }

            if (!User.Identity.IsAuthenticated)
            {
                TempData["Error"] = "Bạn cần đăng nhập để chia sẻ.";
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.Identity.Name;
            var userName = email.Split('@')[0];

            var share = new Share
            {
                Content = Content,
                Topic = Topic,
                UserId = userId,
                UserName = userName,
                CreatedAt = DateTime.Now
            };

            _context.Add(share);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Share));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteShare(int id)
        {
            var share = await _context.Shares.FindAsync(id);
            if (share == null)
            {
                return NotFound();
            }

            _context.Shares.Remove(share);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Share));
        }
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile upload)
        {
            if (upload != null && upload.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(upload.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }
                var imageUrl = Url.Content("~/uploads/" + fileName);
                return Json(new { uploaded = true, url = imageUrl });
            }

            return BadRequest("No file uploaded.");
        }
    }
}
