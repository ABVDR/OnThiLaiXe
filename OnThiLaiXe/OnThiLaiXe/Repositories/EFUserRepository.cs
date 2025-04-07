using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public class EFUserRepository : IUserRepository
    {
        public readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public EFUserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task AddAsync(ApplicationUser user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllSysnc(ClaimsPrincipal currentUser)
        {
            var users = await _context.ApplicationUsers.ToListAsync();
            if (currentUser != null)
            {
                var user = await _userManager.GetUserAsync(currentUser);
                if (user != null)
                {
                    users = users.Where(u => u.Id != user.Id).ToList();
                }
            }
            return users;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }
        public async Task UpdateAsync(ApplicationUser user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
