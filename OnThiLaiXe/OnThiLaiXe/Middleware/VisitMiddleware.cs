using Microsoft.AspNetCore.Identity;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Middleware
{
    public class VisitMiddleware
    {
        private readonly RequestDelegate _next;

        public VisitMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sessionKey = "VisitRecorded";
            var currentTime = DateTime.UtcNow.AddHours(7); // Giờ VN

            var _context = context.RequestServices.GetRequiredService<ApplicationDbContext>();

            // Kiểm tra xem người dùng có phải là admin hay không
            var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.GetUserAsync(context.User);

            if (user != null && !context.User.IsInRole("Admin")) // Nếu người dùng không phải là Admin
            {
                // Kiểm tra session, nếu chưa có thì tạo mới
                if (!context.Session.TryGetValue(sessionKey, out _))
                {
                    var visitorIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                    var visitLog = new VisitLog
                    {
                        VisitTime = currentTime,
                        VisitorId = visitorIp
                    };

                    _context.VisitLogs.Add(visitLog);
                    await _context.SaveChangesAsync();

                    // Đặt session, lần sau trong cùng trình duyệt sẽ không ghi nữa
                    context.Session.SetString(sessionKey, "true");
                }
            }

            await _next(context);
        }
    }
}
