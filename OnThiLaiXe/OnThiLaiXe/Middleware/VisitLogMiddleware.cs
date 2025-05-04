using OnThiLaiXe.Models;

namespace OnThiLaiXe.Middleware
{
    public class VisitLogMiddleware
    {
        private readonly RequestDelegate _next;

        public VisitLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var sessionKey = "VisitRecorded";
            var currentTime = DateTime.UtcNow.AddHours(7);

            if (!context.Session.TryGetValue(sessionKey, out _))
            {
                var visitorIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                var visitLog = new VisitLog
                {
                    VisitTime = currentTime,
                    VisitorId = visitorIp
                };

                dbContext.VisitLogs.Add(visitLog);
                await dbContext.SaveChangesAsync();

                context.Session.SetString(sessionKey, "true");
            }

            await _next(context);
        }
    }
}
