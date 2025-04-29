using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace OnThiLaiXe.Services
{
    public class OnlineUsersHub : Hub
    {
        private static int _onlineUsers = 0;
        private static readonly ConcurrentDictionary<string, bool> _connectedUsers = new ConcurrentDictionary<string, bool>();

        public override async Task OnConnectedAsync()
        {
            var context = Context.GetHttpContext();
            bool isAdmin = false;

            // Kiểm tra xem người dùng có phải là Admin không
            if (context.User.Identity.IsAuthenticated)
            {
                isAdmin = context.User.IsInRole("Admin") || context.User.Claims.Any(c =>
                    c.Type == ClaimTypes.Role && c.Value == "Admin") ||
                    context.Request.Path.StartsWithSegments("/Admin");
            }

            // Lưu trạng thái Admin của connection
            _connectedUsers.TryAdd(Context.ConnectionId, isAdmin);

            if (!isAdmin)
            {
                _onlineUsers++;
                await UpdateUserCount();
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Kiểm tra xem connection là của Admin hay không
            if (_connectedUsers.TryRemove(Context.ConnectionId, out bool isAdmin) && !isAdmin)
            {
                _onlineUsers--;
                await UpdateUserCount();
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task UpdateUserCount()
        {
            await Clients.All.SendAsync("UpdateUserCount", _onlineUsers);
        }
    }
}