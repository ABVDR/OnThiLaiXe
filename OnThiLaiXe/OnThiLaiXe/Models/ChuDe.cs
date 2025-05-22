using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace OnThiLaiXe.Models
{
    public class ChuDe
    {
        public int Id { get; set; }

        [Required]
        public string TenChuDe { get; set; }

        public string MoTa { get; set; }
        public string? ImageUrl { get; set; }
        public bool isDeleted { get; set; } = false;
        [ValidateNever]
        public ICollection<CauHoi> CauHois { get; set; }
    }
}
