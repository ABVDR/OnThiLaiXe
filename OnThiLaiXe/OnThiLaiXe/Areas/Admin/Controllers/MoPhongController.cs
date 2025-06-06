using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;
using OnThiLaiXe.Repositories;

namespace OnThiLaiXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class MoPhongController : Controller
    {
        private readonly IMoPhongRepository _moPhongRepository;

        public MoPhongController(IMoPhongRepository moPhongRepository)
        {
            _moPhongRepository = moPhongRepository;
        }

        // GET: Admin/MoPhong
        public async Task<IActionResult> Index()
        {
            var moPhongs = await _moPhongRepository.GetOrderedAsync();
            return View(moPhongs);
        }
        private async Task<string> SaveVideo(IFormFile video)
        {
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(video.FileName);

            var savePath = Path.Combine("wwwroot/videos", uniqueFileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await video.CopyToAsync(fileStream);
            }
            return "/videos/" + uniqueFileName; // Trả về đường dẫn tương đối
        }
        // GET: Admin/MoPhong/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/MoPhong/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MoPhong moPhong, IFormFile videoUrl)
        {
            if (ModelState.IsValid)
            {
                if (videoUrl != null)
                {
                    moPhong.VideoUrl = await SaveVideo(videoUrl);
                }
                await _moPhongRepository.AddAsync(moPhong);
                return RedirectToAction(nameof(Index));
            }
            return View(moPhong);
        }
        // GET: Admin/MoPhong/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moPhong = await _moPhongRepository.GetByIdAsync(id.Value);
            if (moPhong == null)
            {
                return NotFound();
            }

            return View(moPhong);
        }

        

        // GET: Admin/MoPhong/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moPhong = await _moPhongRepository.GetByIdAsync(id.Value);
            if (moPhong == null)
            {
                return NotFound();
            }
            return View(moPhong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MoPhong moPhong, IFormFile videoUrl)
        {
            
            ModelState.Remove("videoUrl");
            if (id != moPhong.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingMoPhong = await _moPhongRepository.GetByIdAsync(id);
                    if (videoUrl != null)
                    {
                    existingMoPhong.VideoUrl = await SaveVideo(videoUrl);
                    }
                    else
                    {
                    Console.WriteLine("dang null");
                    moPhong.VideoUrl = existingMoPhong.VideoUrl;
                    }
                    existingMoPhong.NoiDung = moPhong.NoiDung;
                    existingMoPhong.DapAn = moPhong.DapAn;
                    existingMoPhong.Order = moPhong.Order;
                // Cập nhật thông tin mô phỏng 

                await _moPhongRepository.UpdateAsync(existingMoPhong);

                    return RedirectToAction(nameof(Index));
                }
            return View(moPhong);
        }

        // GET: Admin/MoPhong/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moPhong = await _moPhongRepository.GetByIdAsync(id.Value);
            if (moPhong == null)
            {
                return NotFound();
            }

            return View(moPhong);
        }

        // POST: Admin/MoPhong/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _moPhongRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
