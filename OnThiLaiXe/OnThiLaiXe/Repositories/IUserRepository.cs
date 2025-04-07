using System.Security.Claims;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public interface IUserRepository
    {
        public Task<IEnumerable<ApplicationUser>> GetAllSysnc(ClaimsPrincipal currentUser);

        public Task<ApplicationUser> GetUserByIdAsync(string id);

        public Task AddAsync(ApplicationUser user);

        public Task UpdateAsync(ApplicationUser user);
        public Task DeleteAsync(string id);
    }
}
