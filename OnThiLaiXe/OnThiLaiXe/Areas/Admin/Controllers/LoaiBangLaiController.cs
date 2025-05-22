using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnThiLaiXe.Models;
using OnThiLaiXe.Repositories;

namespace OnThiLaiXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class LoaiBangLaiController : Controller
    {
        private readonly ILoaiBangLaiRepository _loaiBangLaiRepository;
        public LoaiBangLaiController(ILoaiBangLaiRepository loaiBangLaiRepository)
        {
            _loaiBangLaiRepository = loaiBangLaiRepository;
        }
        public async Task<IActionResult> Index()
        {
            var loaiBangLais = await _loaiBangLaiRepository.GetAllAsync();
            return View(loaiBangLais);
        }
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoaiBangLai loaiBangLai)
        {
            if (ModelState.IsValid)
            {
                await _loaiBangLaiRepository.AddAsync(loaiBangLai);
                return RedirectToAction(nameof(Index));
            }
            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState)
                {
                    Console.WriteLine($"Key: {modelError.Key}, Errors: {string.Join(", ", modelError.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            return View(loaiBangLai);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaibanglai = await _loaiBangLaiRepository.GetByIdAsync(id.Value);
            if (loaibanglai == null)
            {
                return NotFound();
            }
            return View(loaibanglai);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LoaiBangLai loaiBangLai)
        {
            if (ModelState.IsValid)
            {
                await _loaiBangLaiRepository.UpdateAsync(loaiBangLai);
                return RedirectToAction(nameof(Index));
            }
            return View(loaiBangLai);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaibanglai = await _loaiBangLaiRepository.GetByIdAsync(id.Value);
            if (loaibanglai == null)
            {
                return NotFound();
            }

            return View(loaibanglai);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _loaiBangLaiRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> ToggleActive(int id)
        {
            var loaibanglai = await _loaiBangLaiRepository.GetByIdAsync(id);
            if (loaibanglai == null)
            {
                return NotFound();
            }

            // Đảo ngược trạng thái isDeleted
            loaibanglai.isDeleted = !loaibanglai.isDeleted;
            await _loaiBangLaiRepository.UpdateAsync(loaibanglai);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiBangLai = await _loaiBangLaiRepository.GetByIdAsync(id.Value);
            if (loaiBangLai == null)
            {
                return NotFound();
            }

            return View(loaiBangLai);
        }
    }
}
