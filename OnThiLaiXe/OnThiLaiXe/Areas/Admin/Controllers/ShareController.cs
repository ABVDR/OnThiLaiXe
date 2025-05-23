using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class SharereportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SharereportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reports = await _context.ShareReports
                .Include(r => r.Share)
                .Include(r => r.ShareReply)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reports);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _context.ShareReports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            _context.ShareReports.Remove(report);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteContent(string type, int id)
        {
            if (type == "share")
            {
                var share = await _context.Shares.FindAsync(id);
                if (share != null)
                {
                    // Xóa báo cáo liên quan đến share gốc
                    var shareReports = _context.ShareReports.Where(r => r.ShareId == id);
                    _context.ShareReports.RemoveRange(shareReports);

                    _context.Shares.Remove(share);
                    await _context.SaveChangesAsync();
                }
            }
            else if (type == "reply")
            {
                var reply = await _context.ShareReplies.FindAsync(id);
                if (reply != null)
                {
                    // Xóa báo cáo liên quan đến reply này
                    var replyReports = _context.ShareReports.Where(r => r.ShareReplyId == id);
                    _context.ShareReports.RemoveRange(replyReports);

                    _context.ShareReplies.Remove(reply);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
