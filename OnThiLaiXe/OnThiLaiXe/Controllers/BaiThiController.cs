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

            return View("KetQuaBaiThi", ketQuaList);
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
       

        //[HttpPost]
        //public IActionResult TaoDeThi(int loaiBangLaiId, Dictionary<int, int> soLuongMoiChuDe)
        //{
        //    if (soLuongMoiChuDe == null || !soLuongMoiChuDe.Any())
        //    {
        //        TempData["Error"] = "Số lượng câu hỏi theo chủ đề không hợp lệ.";
        //        return RedirectToAction("ChonDeThi");
        //    }

        //    if (!_context.LoaiBangLais.Any(lb => lb.Id == loaiBangLaiId))
        //    {
        //        TempData["Error"] = "Loại bằng lái không hợp lệ.";
        //        return RedirectToAction("ChonDeThi");
        //    }

        //    var danhSachCauHoi = new List<CauHoi>();

        //    foreach (var chuDe in soLuongMoiChuDe)
        //    {
        //        int chuDeId = chuDe.Key;
        //        int soLuong = chuDe.Value;

        //        // Lấy câu hỏi theo chủ đề và loại bằng lái
        //        var cauHoiTheoChuDe = _context.CauHois
        //            .Where(c => c.LoaiBangLaiId == loaiBangLaiId && c.ChuDeId == chuDeId)
        //            .OrderBy(r => Guid.NewGuid()) // Lấy ngẫu nhiên
        //            .Take(soLuong)
        //            .ToList();

        //        danhSachCauHoi.AddRange(cauHoiTheoChuDe);
        //    }

        //    if (danhSachCauHoi.Count == 0)
        //    {
        //        TempData["Error"] = "Không đủ câu hỏi để tạo đề thi.";
        //        return RedirectToAction("ChonDeThi");
        //    }

        //    try
        //    {
        //        var deThi = new BaiThi
        //        {
        //            NgayThi = DateTime.Now,
        //            TenBaiThi = $"Đề thi chính thức - {DateTime.Now:dd/MM/yyyy HH:mm}",
        //            LoaiBaiThi = "Đề thi chính thức",
        //            ChiTietBaiThis = danhSachCauHoi.Select(c => new ChiTietBaiThi { CauHoiId = c.Id }).ToList()
        //        };

        //        // Nếu người dùng đăng nhập, lưu thông tin người dùng
        //        if (User.Identity.IsAuthenticated)
        //        {
        //            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //            deThi.UserId = int.Parse(currentUserId);
        //        }

        //        _context.BaiThis.Add(deThi);
        //        _context.SaveChanges();

        //        return RedirectToAction("ChiTietBaiThi", new { id = deThi.Id });
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = "Đã xảy ra lỗi khi tạo đề thi: " + ex.Message;
        //        return RedirectToAction("ChonDeThi");
        //    }
        //}


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
                TempData["Error"] = "Không có câu hỏi nào cho loại bằng lái này.";
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

        [HttpPost]
        public IActionResult LuuKetQuaLuyenLai(List<KetQuaLuyenTapViewModel> ketQua)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.TryParse(currentUserId, out var uid) ? uid : (Guid.TryParse(currentUserId, out var guid) ? guid.GetHashCode() & int.MaxValue : -1);
            if (userId == -1) return Unauthorized();

            foreach (var item in ketQua)
            {
                var cauHoiSai = _context.CauHoiSais
                    .FirstOrDefault(c => c.UserId == userId && c.CauHoiId == item.CauHoiId);

                // Nếu người dùng làm đúng lại -> xóa khỏi danh sách sai
                if (item.Dung)
                {
                    if (cauHoiSai != null)
                        _context.CauHoiSais.Remove(cauHoiSai);
                }
                else
                {
                    // Nếu vẫn sai thì cập nhật lại NgaySai
                    if (cauHoiSai != null)
                        cauHoiSai.NgaySai = DateTime.Now;
                    else
                    {
                        // Nếu chưa có thì thêm mới
                        _context.CauHoiSais.Add(new CauHoiSai
                        {
                            UserId = userId,
                            CauHoiId = item.CauHoiId,
                            NgaySai = DateTime.Now
                        });
                    }
                }
            }

            _context.SaveChanges();
            return Json(new { success = true });
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





        [HttpGet]
        public async Task<IActionResult> Share(string? sortOrder, string? searchString)
        {
            var sharesQuery = _context.Shares.AsQueryable();


            if (!string.IsNullOrEmpty(searchString))
            {

                var matchingShares = _context.Shares.Where(s =>
                    s.Content.Contains(searchString) ||
                    (s.Topic != null && s.Topic.Contains(searchString)));


                var matchingReplyShareIds = _context.ShareReplies
                    .Where(r => r.Content.Contains(searchString))
                    .Select(r => r.ShareId)
                    .Distinct();
                sharesQuery = matchingShares
                          .Union(_context.Shares.Where(s => matchingReplyShareIds.Contains(s.Id)));
            }


            ViewBag.CurrentSort = sortOrder;
            sharesQuery = sortOrder == "oldest"
                ? sharesQuery.OrderBy(s => s.CreatedAt)
                : sharesQuery.OrderByDescending(s => s.CreatedAt);

            var shares = await sharesQuery.ToListAsync();


            var replies = await _context.ShareReplies.ToListAsync();
            ViewBag.AllReplies = replies;
            ViewBag.ChuDeList = new List<string> { "GPLX", "Lý Thuyết", "Mô Phỏng", "Khác" };
            ViewBag.SearchString = searchString;

            return View(shares);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateShare(string Content, string? Topic)
        {

            if (!User.Identity.IsAuthenticated)
            {
                TempData["RequireLogin"] = "Bạn cần đăng nhập để chia sẻ.";
                return RedirectToAction(nameof(Share));
            }
            if (string.IsNullOrEmpty(Content))
            {
                TempData["Error"] = "Nội dung không được để trống.";
                return RedirectToAction(nameof(Share));
            }

            if (string.IsNullOrEmpty(Topic))
            {
                TempData["Error"] = "Vui lòng chọn chủ đề trước khi chia sẻ.";
                return RedirectToAction(nameof(Share));
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReply(int shareId, string content, int? parentReplyId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["RequireLogin"] = "Bạn cần đăng nhập để trả lời.";
                return RedirectToAction(nameof(Share));
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Nội dung trả lời không được để trống.";
                return RedirectToAction(nameof(Share));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.Identity.Name;
            var userName = email.Split('@')[0];

            var reply = new ShareReply
            {
                ShareId = shareId,
                Content = content,
                ParentReplyId = parentReplyId,
                CreatedAt = DateTime.Now,
                UserId = userId,
                UserName = userName
            };

            _context.ShareReplies.Add(reply);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Share));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReply(int id)
        {
            var reply = await _context.ShareReplies.FindAsync(id);
            if (reply == null)
            {
                return NotFound();
            }
            _context.ShareReplies.Remove(reply);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Share));

        }
    }
}