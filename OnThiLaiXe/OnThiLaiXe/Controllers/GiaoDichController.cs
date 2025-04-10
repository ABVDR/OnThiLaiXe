using Microsoft.AspNetCore.Mvc;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Controllers
{
    public class GiaoDichController : Controller
    {
        private readonly ApplicationDbContext _context;
       

        public GiaoDichController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult ThanhToanTruoc()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> HoanTatGiaoDich([FromBody] GiaoDichModel model)
        {
            var giaoDich = new GiaoDich
            {
                UserId = model.UserId,
                MaGiaoDich = model.OrderId,
                SoTien = 5.00m,
                NgayThanhToan = DateTime.Now,
                DaThanhToan = true
            };

            _context.GiaoDichs.Add(giaoDich);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
