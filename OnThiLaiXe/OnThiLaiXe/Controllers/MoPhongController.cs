using Microsoft.AspNetCore.Mvc;
using OnThiLaiXe.Repositories;

namespace OnThiLaiXe.Controllers
{
    public class MoPhongController : Controller
    {
        private readonly IMoPhongRepository _repository;

        public MoPhongController(IMoPhongRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var moPhongs = await _repository.GetOrderedAsync();

            // If there are no items, just return the empty list to the view
            if (!moPhongs.Any())
            {
                return View(moPhongs);
            }

            // Redirect to the first question if available
            var firstItem = moPhongs.First();
            return RedirectToAction("Details", new { id = firstItem.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var moPhong = await _repository.GetByIdAsync(id);
            if (moPhong == null)
            {
                return NotFound();
            }

            var allMoPhongs = await _repository.GetOrderedAsync();

            // Pass both the current item and the list of all items to the view
            ViewBag.AllMoPhongs = allMoPhongs;
            return View(moPhong);
        }
    }
}
