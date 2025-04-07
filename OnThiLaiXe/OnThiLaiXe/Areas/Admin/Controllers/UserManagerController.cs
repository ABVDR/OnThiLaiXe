using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;
using OnThiLaiXe.Repositories;

namespace OnThiLaiXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserManagerController : Controller
    {
        public readonly IUserRepository _userRepository;
        public readonly RoleManager<IdentityRole> _roleRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserManagerController(IUserRepository userRepository, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _roleRepository = roleManager;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllSysnc(User);
            var roles = await _roleRepository.Roles.ToListAsync();
            var userRolesDict = new Dictionary<string, List<string>>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                userRolesDict[user.Id] = userRoles.ToList(); // Lưu vai trò theo ID user
            }

            ViewBag.UserRoles = userRolesDict; // Truyền xuống View
            ViewBag.Roles = new SelectList(roles, "Name", "Name");

            return View(users); // Không cần class ViewModel
        }
        //public async Task<IActionResult> Update()
        //{
        //    var roles = await _roleRepository.Roles.ToListAsync();
        //    ViewBag.Roles = new SelectList(roles, "Name", "Name");
        //    return View();
        //}
        [HttpPost]
        public async Task<IActionResult> UpdateRoleUser(string id, string role)
        {
            var result = await _userRepository.GetUserByIdAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            // Lấy tất cả các vai trò hiện tại của người dùng
            var userRoles = await _userManager.GetRolesAsync(result);

            // Kiểm tra nếu vai trò mới chưa có thì thêm vào
            if (!userRoles.Contains(role))
            {
                await _userManager.AddToRoleAsync(result, role);
            }

            // Loại bỏ các vai trò không còn phù hợp với người dùng
            foreach (var userRole in userRoles)
            {
                // Nếu người dùng có vai trò cũ và vai trò đó không phải là vai trò đang chọn, xóa vai trò cũ
                if (userRole != role)
                {
                    await _userManager.RemoveFromRoleAsync(result, userRole);
                }
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
