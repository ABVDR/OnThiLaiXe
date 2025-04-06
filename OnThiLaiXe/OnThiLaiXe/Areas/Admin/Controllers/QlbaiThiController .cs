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

        public QlbaiThiController(ApplicationDbContext context, ICauHoiRepository cauHoiRepo)
        {
            _context = context;
            _cauHoiRepo = cauHoiRepo;
        }
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
                    ChiTietBaiThis = danhSachCauHoi.Select(c => new ChiTietBaiThi { CauHoiId = c.Id }).ToList()
                };

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
        public IActionResult OnTapTheoChuDeVaLoaiBangLai(int? loaiBangLaiId)
        {
            var danhSachChuDe = _context.ChuDes.ToList();
            var danhSachLoaiBangLai = _context.LoaiBangLais.ToList();
            ViewBag.DanhSachLoaiBangLai = danhSachLoaiBangLai;
            ViewBag.SelectedLoaiBangLaiId = loaiBangLaiId;
            return View(danhSachChuDe);
        }



    }
}