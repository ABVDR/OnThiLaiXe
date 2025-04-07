using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using OnThiLaiXe.Repositories;

namespace OnThiLaiXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class BaiSaHinhController : Controller
    {
        private readonly IBaiSaHinhRepository _baiSaHinhRepository;
        private readonly ILoaiBangLaiRepository _loaiBangLaiRepository;

        public BaiSaHinhController(IBaiSaHinhRepository baiSaHinhRepository, ILoaiBangLaiRepository loaiBangLaiRepository)
        {
            _baiSaHinhRepository = baiSaHinhRepository;
            _loaiBangLaiRepository = loaiBangLaiRepository;
        }

        public async Task<IActionResult> Index(int? loaiBangLaiId)
        {
            var loaiBangLais = await _loaiBangLaiRepository.GetAllAsync();
            ViewBag.LoaiBangLais = loaiBangLais;

            IEnumerable<BaiSaHinh> baiSaHinhs;
            if (loaiBangLaiId.HasValue)
            {
                baiSaHinhs = await _baiSaHinhRepository.GetBaiSaHinhByLoaiBangLaiIdAsync(loaiBangLaiId.Value);
            }
            else
            {
                baiSaHinhs = await _baiSaHinhRepository.GetAllBaiSaHinhAsync();
            }

            ViewBag.SelectedLoaiBangLaiId = loaiBangLaiId;
            return View(baiSaHinhs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var baiSaHinh = await _baiSaHinhRepository.GetBaiSaHinhByIdAsync(id);
            if (baiSaHinh == null) return NotFound();
            return View(baiSaHinh);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.LoaiBangLais = await _loaiBangLaiRepository.GetAllAsync();
            return View(new BaiSaHinh());
        }

        [HttpPost]
        public async Task<IActionResult> Create(BaiSaHinh baiSaHinh)
        {
            if (ModelState.IsValid)
            {
                await _baiSaHinhRepository.AddBaiSaHinhAsync(baiSaHinh);
                return RedirectToAction("Index", new { loaiBangLaiId = baiSaHinh.LoaiBangLaiId });
            }

            ViewBag.LoaiBangLais = await _loaiBangLaiRepository.GetAllAsync();
            return View(baiSaHinh);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var baiSaHinh = await _baiSaHinhRepository.GetBaiSaHinhByIdAsync(id);
            if (baiSaHinh == null) return NotFound();

            ViewBag.LoaiBangLais = await _loaiBangLaiRepository.GetAllAsync();
            return View(baiSaHinh);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BaiSaHinh baiSaHinh)
        {
            if (ModelState.IsValid)
            {
                await _baiSaHinhRepository.UpdateBaiSaHinhAsync(baiSaHinh);
                return RedirectToAction("Index", new { loaiBangLaiId = baiSaHinh.LoaiBangLaiId });
            }

            ViewBag.LoaiBangLais = await _loaiBangLaiRepository.GetAllAsync();
            return View(baiSaHinh);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var baiSaHinh = await _baiSaHinhRepository.GetBaiSaHinhByIdAsync(id);
            if (baiSaHinh == null) return NotFound();

            await _baiSaHinhRepository.DeleteBaiSaHinhAsync(id);
            return RedirectToAction("Index", new { loaiBangLaiId = baiSaHinh.LoaiBangLaiId });
        }
    }
}