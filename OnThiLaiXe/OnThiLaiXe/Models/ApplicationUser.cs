using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace OnThiLaiXe.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
        //public ICollection<BaiThi> BaiThis { get; set; }
    }
}
