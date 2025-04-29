using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;
using OnThiLaiXe.Repositories;

namespace OnThiLaiXe.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class QlbaiThiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICauHoiRepository _cauHoiRepo;
        private readonly ILogger<QlbaiThiController> _logger;

        public QlbaiThiController(ApplicationDbContext context, ICauHoiRepository cauHoiRepo, ILogger<QlbaiThiController> logger)
        {
            _context = context;
            _cauHoiRepo = cauHoiRepo;
            _logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> TaoDeThi(int loaiBangLaiId, IFormCollection form)
        {
            // Parse số lượng câu hỏi theo từng chủ đề từ form
            var soLuongMoiChuDe = new Dictionary<int, int>();

            foreach (var key in form.Keys)
            {
                if (key.StartsWith("soLuongMoiChuDe["))
                {
                    var chuDeIdStr = key.Replace("soLuongMoiChuDe[", "").Replace("]", "");
                    if (int.TryParse(chuDeIdStr, out int chuDeId) &&
                        int.TryParse(form[key], out int soLuong) &&
                        soLuong > 0)
                    {
                        soLuongMoiChuDe[chuDeId] = soLuong;
                    }
                }
            }

            if (soLuongMoiChuDe == null || !soLuongMoiChuDe.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một chủ đề với số lượng câu hỏi hợp lệ.";
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

                var cauHoiTheoChuDe = _context.CauHois
                    .Where(c => c.LoaiBangLaiId == loaiBangLaiId && c.ChuDeId == chuDeId)
                    .OrderBy(c => Guid.NewGuid())
                    .Take(soLuong)
                    .ToList();

                if (cauHoiTheoChuDe.Count < soLuong)
                {
                    TempData["Error"] = $"Không đủ câu hỏi cho chủ đề ID {chuDeId}. Yêu cầu: {soLuong}, Có: {cauHoiTheoChuDe.Count}.";
                    return RedirectToAction("ChonDeThi");
                }

                danhSachCauHoi.AddRange(cauHoiTheoChuDe);
            }

            if (!danhSachCauHoi.Any())
            {
                TempData["Error"] = "Không đủ câu hỏi để tạo đề thi.";
                return RedirectToAction("ChonDeThi");
            }

            try
            {

                var deThi = new BaiThi
                {
                    TenBaiThi = "Đề thi mới",
                    ChiTietBaiThis = new List<ChiTietBaiThi>() // Đảm bảo không null
                };

                _context.BaiThis.Add(deThi);
                await _context.SaveChangesAsync();

                var chiTietList = danhSachCauHoi.Select(c => new ChiTietBaiThi
                {
                    BaiThiId = deThi.Id,
                    CauHoiId = c.Id,
                    CauTraLoi = null,
                    DungSai = null
                }).ToList();

                _context.ChiTietBaiThis.AddRange(chiTietList);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Tạo đề thi thành công.";
                return RedirectToAction("ChiTietBaiThi", new { id = deThi.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đề thi: {Message}", ex.InnerException?.Message ?? ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi khi tạo đề thi: " + (ex.InnerException?.Message ?? ex.Message);
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

                .ToList();

            return View(baiThis);
        }


        public IActionResult DanhSachDeThi()
        {
            var danhSachDeThi = _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                    .ThenInclude(ct => ct.CauHoi)
                        .ThenInclude(c => c.LoaiBangLai) // Đảm bảo nạp dữ liệu loại bằng lái
                .ToList();

            return View(danhSachDeThi);
        }


        public IActionResult DanhSachLoaiBangLai()
        {
            var danhSachLoaiBangLai = _context.LoaiBangLais.ToList();
            return View(danhSachLoaiBangLai); // Truyền model thay vì ViewBag
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var baiThi = await _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                .FirstOrDefaultAsync(bt => bt.Id == id);

            if (baiThi == null)
            {
                return NotFound();
            }

            return View(baiThi);
        }

        // Xử lý xác nhận xóa bài thi
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var baiThi = await _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                .FirstOrDefaultAsync(bt => bt.Id == id);

            if (baiThi == null)
            {
                return NotFound();
            }

            _context.BaiThis.Remove(baiThi);
            await _context.SaveChangesAsync();

            return RedirectToAction("DanhSachBaiThi");
        }
        public IActionResult OnTapTheoChuDeVaLoaiBangLai(int? loaiBangLaiId)
        {
            var danhSachChuDe = _context.ChuDes.ToList();
            var danhSachLoaiBangLai = _context.LoaiBangLais.ToList();
            ViewBag.DanhSachLoaiBangLai = danhSachLoaiBangLai;
            ViewBag.SelectedLoaiBangLaiId = loaiBangLaiId;
            return View(danhSachChuDe);
        }





        public IActionResult Search(string keywords, string topic)
        {
            // Fetch the list of ChuDe (topics) from the database to pass to the View for dropdown selection
            var chuDes = _context.ChuDes.ToList();
            ViewBag.ChuDes = chuDes;

            // Initialize the query to filter BaiThi based on keywords and topic
            var questions = _context.BaiThis
                .Include(b => b.ChiTietBaiThis)
                .ThenInclude(ct => ct.CauHoi)
                .ThenInclude(c => c.ChuDe)  // Ensure that we include ChuDe for filtering by topic
                .Where(b => (string.IsNullOrEmpty(keywords) || b.ChiTietBaiThis.Any(c => c.CauHoi.NoiDung.Contains(keywords))) &&
                            (string.IsNullOrEmpty(topic) || b.ChiTietBaiThis.Any(c => c.CauHoi.ChuDe.TenChuDe == topic)))
                .ToList();

            // Return the filtered list in the same view (or a different view if you prefer)
            return View("DanhSachDeThi", questions);
        }

    }
}
