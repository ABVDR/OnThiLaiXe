﻿using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Controllers
{
    public class ShareController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ShareController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Share(string? sortOrder, string? searchString)
        {
            var sharesQuery = _context.Shares.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                var matchingShares = _context.Shares.Where(s =>
                    s.Content.Contains(searchString) ||
                    (s.Topic != null && s.Topic.Contains(searchString)));

                var matchingReplyShareIds = _context.ShareReplies
                    .Where(r => r.Content.Contains(searchString))
                    .Select(r => r.ShareId)
                    .Distinct();
                sharesQuery = matchingShares
                          .Union(_context.Shares.Where(s => matchingReplyShareIds.Contains(s.Id)));
            }

            ViewBag.CurrentSort = sortOrder;
            sharesQuery = sortOrder == "oldest"
                ? sharesQuery.OrderBy(s => s.CreatedAt)
                : sharesQuery.OrderByDescending(s => s.CreatedAt);

            var shares = await sharesQuery.ToListAsync();

            var replies = await _context.ShareReplies.ToListAsync();
            ViewBag.AllReplies = replies;
            ViewBag.ChuDeList = new List<string> { "GPLX", "Lý Thuyết", "Mô Phỏng", "Khác" };
            ViewBag.SearchString = searchString;

            return View(shares);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateShare(string Content, string? Topic)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để chia sẻ." });
            }

            if (string.IsNullOrEmpty(Content))
            {
                return Json(new { success = false, message = "Nội dung không được để trống." });
            }

            if (string.IsNullOrEmpty(Topic))
            {
                return Json(new { success = false, message = "Vui lòng chọn chủ đề trước khi chia sẻ." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.Identity.Name;
            var userName = email.Split('@')[0];

            var share = new Share
            {
                Content = Content,
                Topic = Topic,
                UserId = userId,
                UserName = userName,
                CreatedAt = DateTime.Now
            };

            _context.Add(share);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReply(int shareId, string content, int? parentReplyId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để trả lời." });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Nội dung trả lời không được để trống." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.Identity.Name;
            var userName = email.Split('@')[0];

            var reply = new ShareReply
            {
                ShareId = shareId,
                Content = content,
                ParentReplyId = parentReplyId,
                CreatedAt = DateTime.Now,
                UserId = userId,
                UserName = userName
            };

            _context.ShareReplies.Add(reply);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                id = reply.Id,
                userId = reply.UserId,
                userName,
                content,
                createdAt = reply.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                shareId = reply.ShareId,
                parentReplyId = parentReplyId
            });
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile upload)
        {
            if (upload != null && upload.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(upload.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }
                var imageUrl = Url.Content("~/uploads/" + fileName);
                return Json(new { uploaded = true, url = imageUrl });
            }

            return BadRequest("No file uploaded.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteShare(int id)
        {
            var share = await _context.Shares.FindAsync(id);
            if (share == null) return Json(new { success = false, message = "Không tìm thấy chia sẻ." });

            _context.Shares.Remove(share);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReply(int id)
        {
            var reply = await _context.ShareReplies.FindAsync(id);
            if (reply == null) return Json(new { success = false, message = "Không tìm thấy trả lời." });

            _context.ShareReplies.Remove(reply);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetShareRepliesPartial(int shareId)
        {
            var replies = await _context.ShareReplies
                .Where(r => r.ShareId == shareId)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.AllReplies = replies;

            var share = await _context.Shares.FirstOrDefaultAsync(s => s.Id == shareId);
            return PartialView("Share", share);
        }

        [HttpGet]
        public IActionResult Sharereport(int? shareId, int? replyId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["RequireLogin"] = "Bạn cần đăng nhập để báo cáo nội dung.";
                return RedirectToAction("Share");
            }

            ViewBag.ShareId = shareId;
            ViewBag.ReplyId = replyId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sharereport(int? shareId, int? replyId, string reason)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["RequireLogin"] = "Bạn cần đăng nhập để báo cáo nội dung.";
                return RedirectToAction("Share");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Lý do báo cáo không được để trống.";
                return RedirectToAction("Share");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Đảm bảo chỉ gán 1 trường, trường còn lại là null
            int? safeShareId = (shareId.HasValue && shareId > 0) ? shareId : null;
            int? safeReplyId = (replyId.HasValue && replyId > 0) ? replyId : null;

            var report = new Sharereport
            {
                ShareId = safeShareId,
                ShareReplyId = safeReplyId,
                Reason = reason,
                CreatedAt = DateTime.Now,
                UserId = userId
            };

            _context.ShareReports.Add(report);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Báo cáo của bạn đã được gửi.";
            return RedirectToAction("Share");
        }
    }
}
