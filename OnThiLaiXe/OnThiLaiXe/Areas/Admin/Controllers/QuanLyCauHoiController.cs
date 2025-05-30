﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnThiLaiXe.Models;
using OnThiLaiXe.Repositories;

namespace OnThiLaiXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class QuanLyCauHoiController : Controller
    {

        private readonly ICauHoiRepository _cauHoiRepository;
        private readonly IChuDeRepository _chuDeRepository;
        private readonly ILoaiBangLaiRepository _loaiBangLaiRepository;
        public QuanLyCauHoiController(ICauHoiRepository cauHoiRepository, IChuDeRepository chuDeRepository, ILoaiBangLaiRepository loaiBangLaiRepository)
        {
            _cauHoiRepository = cauHoiRepository;
            _chuDeRepository = chuDeRepository;
            _loaiBangLaiRepository = loaiBangLaiRepository;
        }

        public async Task<IActionResult> Index()
        {
            var loaiBangLais = await _loaiBangLaiRepository.GetAllNotDelete();
            ViewBag.LoaiBangLaisList = loaiBangLais;
            ViewBag.LoaiBangLais = new SelectList(loaiBangLais, "Id", "TenLoai");

            var chuDes = await _chuDeRepository.GetAllNotDelete();
            ViewBag.ChuDesList = chuDes;
            var cauHois = await _cauHoiRepository.GetAllAsync();
            return View(cauHois);
        }

        public async Task<IActionResult> Add()
        {
            var chudes = await _chuDeRepository.GetAllNotDelete();
            ViewBag.ChuDes = new SelectList(chudes, "Id", "TenChuDe");

            var loaiBangLais = await _loaiBangLaiRepository.GetAllNotDelete();
            ViewBag.LoaiBangLais = new SelectList(loaiBangLais, "Id", "TenLoai");

            return View();
        }

        // POST: Xử lý thêm câu hỏi mới
        [HttpPost]
        public async Task<IActionResult> Add(CauHoi cauhoi, IFormFile? imageUrl)
        {
            if (!ModelState.IsValid)
            {
                var chudes = await _chuDeRepository.GetAllNotDelete();
                ViewBag.ChuDes = new SelectList(chudes, "Id", "TenChuDe");

                var loaiBangLais = await _loaiBangLaiRepository.GetAllNotDelete();
                ViewBag.LoaiBangLais = new SelectList(loaiBangLais, "Id", "TenLoai");

                return View(cauhoi);
            }

            // Nếu có ảnh, lưu vào server
            if (imageUrl != null && imageUrl.Length > 0)
            {
                cauhoi.MediaUrl = await SaveImage(imageUrl);
            }

            await _cauHoiRepository.AddAsync(cauhoi);
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            var savePath = Path.Combine("wwwroot/images", uniqueFileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + uniqueFileName;
        }

        public async Task<IActionResult> Display(int id)
        {
            var cauhoi = await _cauHoiRepository.GetByIdAsync(id);

            if (cauhoi == null)
            {
                Console.WriteLine($"Không tìm thấy câu hỏi với ID: {id}");
                return NotFound();
            }
            if (cauhoi == null)
            {
                return NotFound();
            }
            return View(cauhoi);
        }

        public async Task<IActionResult> Update(int id)
        {
            var cauhoi = await _cauHoiRepository.GetByIdAsync(id);
            if (cauhoi == null)
            {
                return NotFound();
            }

            var chudes = await _chuDeRepository.GetAllAsync();
            ViewBag.ChuDes = new SelectList(chudes, "Id", "TenChuDe");
            var loaiBangLais = await _loaiBangLaiRepository.GetAllAsync();
            ViewBag.LoaiBangLais = new SelectList(loaiBangLais, "Id", "TenLoai");
            return View(cauhoi);
        }

        // Xử lý cập nhật sản phẩm 
        [HttpPost]
        public async Task<IActionResult> Update(int id, CauHoi cauHoi, IFormFile imageUrl)
        {
            ModelState.Remove("ImageUrl");
            if (id != cauHoi.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingCauHoi = await _cauHoiRepository.GetByIdAsync(id);

                if (imageUrl == null)
                {
                    cauHoi.MediaUrl = existingCauHoi.MediaUrl;
                }
                else
                {
                    // Lưu hình ảnh mới
                    cauHoi.MediaUrl = await SaveImage(imageUrl);
                }
                // Cập nhật các thông tin khác của sản phẩm
                existingCauHoi.NoiDung = cauHoi.NoiDung;
                existingCauHoi.LuaChonA = cauHoi.LuaChonA;
                existingCauHoi.LuaChonB = cauHoi.LuaChonB;
                existingCauHoi.LuaChonC = cauHoi.LuaChonC;
                existingCauHoi.LuaChonD = cauHoi.LuaChonD;
                existingCauHoi.DapAnDung = cauHoi.DapAnDung;
                existingCauHoi.GiaiThich = cauHoi.GiaiThich;
                existingCauHoi.DiemLiet = cauHoi.DiemLiet;
                existingCauHoi.MediaUrl = cauHoi.MediaUrl;
                existingCauHoi.LoaiMedia = cauHoi.LoaiMedia;
                existingCauHoi.MeoGhiNho = cauHoi.MeoGhiNho;
                existingCauHoi.ChuDeId = cauHoi.ChuDeId;
                existingCauHoi.LoaiBangLaiId = cauHoi.LoaiBangLaiId;
                await _cauHoiRepository.UpdateAsync(existingCauHoi);
                return RedirectToAction(nameof(Index));
            }
            var chudes = await _chuDeRepository.GetAllAsync();
            ViewBag.ChuDes = new SelectList(chudes, "Id", "TenChuDe");

            var loaiBangLais = await _loaiBangLaiRepository.GetAllAsync();
            ViewBag.LoaiBangLais = new SelectList(loaiBangLais, "Id", "TenLoai");
            return View(cauHoi);
        }

        // Hiển thị form xác nhận xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            var cauhoi = await _cauHoiRepository.GetByIdAsync(id);
            if (cauhoi == null)
            {
                return NotFound();
            }
            return View(cauhoi);
        }

        // Xử lý xóa sản phẩm 
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _cauHoiRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet("CauHoi/GetAutocompleteData")]
        public async Task<IActionResult> GetAutocompleteData(string term)
        {
            if(string.IsNullOrEmpty(term))
            {
                return Json(new List<object>());
            }
            var cauhois = await _cauHoiRepository.SearchCauHoiAsync(term);
            var result = cauhois.Select(c => new
            {
                label = c.NoiDung,
                value = c.Id,
            });
            return Json(result);
        }
        public async Task<IActionResult> ToggleActive(int id)
        {
            var cauHoi = await _cauHoiRepository.GetByIdAsync(id);
            if (cauHoi == null)
            {
                return NotFound();
            }

            // Đảo ngược trạng thái isDeleted
            cauHoi.isDeleted = !cauHoi.isDeleted;
            await _cauHoiRepository.UpdateAsync(cauHoi);
            return RedirectToAction(nameof(Index));
        }
    }
}
