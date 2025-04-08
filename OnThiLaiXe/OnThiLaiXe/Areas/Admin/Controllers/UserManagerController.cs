using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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
        private readonly IConfiguration _configuration;
        public UserManagerController(IUserRepository userRepository, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _roleRepository = roleManager;
            _userManager = userManager;
            _configuration = configuration;
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
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = userRoles.ToList(); // Truyền danh sách vai trò của người dùng
            return View(user);
        }
        public async Task<IActionResult> ExportToExcel()
        {
            var users = await _userRepository.GetAllSysnc(User);
            var stream = new MemoryStream();
            string password = _configuration["ExcelPassword:Password"];
            // Thiết lập License cho EPPlus 8.0.1
            ExcelPackage.License.SetNonCommercialPersonal("Webonlaixe");
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");

                // Tiêu đề cột
                worksheet.Cells[1, 1].Value = "Họ Tên Người Dùng";
                worksheet.Cells[1, 2].Value = "Ngày Tạo";

                // Định dạng tiêu đề
                using (var range = worksheet.Cells[1, 1, 1, 2])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 120, 215));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }
                // Dữ liệu
                int row = 2;
                foreach (var user in users)
                {
                    worksheet.Cells[row, 1].Value = user.FullName;
                    worksheet.Cells[row, 2].Value = user.NgayTao.ToString("dd/MM/yyyy HH:mm:ss");
                    row++;
                }
                worksheet.Cells.AutoFitColumns();

                package.Encryption.IsEncrypted = true;
                package.Encryption.Password = password;
                package.Save();
            }
            stream.Position = 0;
            string excelName = $"Danh sách người dùng-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }
    }
}
