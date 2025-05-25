using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnThiLaiXe.Models;
using OnThiLaiXe.Repositories;

namespace OnThiLaiXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ChuDeController : Controller
    {
        private readonly IChuDeRepository _chuDeRepository;

        public ChuDeController(IChuDeRepository chuDeRepository)
        {
            _chuDeRepository = chuDeRepository;
        }

        // GET: Admin/ChuDe
        public async Task<IActionResult> Index()
        {
            var chuDes = await _chuDeRepository.GetAllAsync();
            return View(chuDes);
        }

        // GET: Admin/ChuDe/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/ChuDe/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChuDe chuDe, IFormFile? imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null && imageUrl.Length > 0)
                {
                    chuDe.ImageUrl = await SaveImage(imageUrl);
                }
                await _chuDeRepository.AddAsync(chuDe);
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState)
                {
                    Console.WriteLine($"Key: {modelError.Key}, Errors: {string.Join(", ", modelError.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            return View(chuDe);
        }

        // GET: Admin/ChuDe/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chuDe = await _chuDeRepository.GetByIdAsync(id.Value);
            if (chuDe == null)
            {
                return NotFound();
            }

            return View(chuDe);
        }

        // GET: Admin/ChuDe/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chuDe = await _chuDeRepository.GetByIdAsync(id.Value);
            if (chuDe == null)
            {
                return NotFound();
            }

            return View(chuDe);
        }

        // POST: Admin/ChuDe/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ChuDe chuDe, IFormFile? imageUrl)
        {
            if (ModelState.IsValid)
            {
                // Get the tracked entity
                var existingChuDe = await _chuDeRepository.GetByIdAsync(chuDe.Id);

                if (existingChuDe == null)
                {
                    return NotFound();
                }

                // Update properties manually
                existingChuDe.TenChuDe = chuDe.TenChuDe;
                existingChuDe.MoTa = chuDe.MoTa;
                // Update other properties as needed

                // Handle image upload
                if (imageUrl != null && imageUrl.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(existingChuDe.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                            existingChuDe.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save new image
                    existingChuDe.ImageUrl = await SaveImage(imageUrl);
                }

                await _chuDeRepository.UpdateAsync(existingChuDe);
                return RedirectToAction(nameof(Index));
            }

            return View(chuDe);
        }

        // GET: Admin/ChuDe/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chuDe = await _chuDeRepository.GetByIdAsync(id.Value);
            if (chuDe == null)
            {
                return NotFound();
            }

            return View(chuDe);
        }

        // POST: Admin/ChuDe/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _chuDeRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/ChuDe/ToggleActive/5
        public async Task<IActionResult> ToggleActive(int id)
        {
            var chuDe = await _chuDeRepository.GetByIdAsync(id);
            if (chuDe == null)
            {
                return NotFound();
            }

            // Đảo ngược trạng thái isDeleted
            chuDe.isDeleted = !chuDe.isDeleted;
            await _chuDeRepository.UpdateAsync(chuDe);
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
    }
}
