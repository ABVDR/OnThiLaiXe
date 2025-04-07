using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnThiLaiXe.Repositories;

namespace OnThiLaiXe.Controllers
{
    public class BaiSaHinhController : Controller
    {
        private readonly IBaiSaHinhRepository _baiSaHinhRepository;
        private readonly ILoaiBangLaiRepository _loaiBangLaiRepository;
        private readonly IConfiguration _configuration;

        public BaiSaHinhController(IBaiSaHinhRepository baiSaHinhRepository, ILoaiBangLaiRepository loaiBangLaiRepository, IConfiguration configuration)
        {
            _baiSaHinhRepository = baiSaHinhRepository;
            _loaiBangLaiRepository = loaiBangLaiRepository;
            _configuration = configuration;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var loaiBangLais = await _loaiBangLaiRepository.GetAllWithBaiHocAsync();
            return View(loaiBangLais);
        }


        // Hiển thị danh sách bài học với nội dung bên phải
        public async Task<IActionResult> DanhSachBaiHoc(int loaiBangLaiId)
        {
            var baiSaHinhs = await _baiSaHinhRepository.GetBaiSaHinhByLoaiBangLaiIdAsync(loaiBangLaiId);
            ViewBag.TenLoaiBangLai = (await _loaiBangLaiRepository.GetByIdAsync(loaiBangLaiId))?.TenLoai;
            ViewBag.TinyMCEApiKey = _configuration["TinyMCE:ApiKey"];
            return View("DanhSachVaChiTiet", baiSaHinhs);
        }

        // API lấy nội dung bài học (sử dụng AJAX)
        public async Task<IActionResult> GetNoiDungBaiHoc(int id)
        {
            var baiSaHinh = await _baiSaHinhRepository.GetBaiSaHinhByIdAsync(id);
            if (baiSaHinh == null)
            {
                return NotFound();
            }
            return Json(new { tenBai = baiSaHinh.TenBai, noiDung = baiSaHinh.NoiDung });
        }
    }
}
