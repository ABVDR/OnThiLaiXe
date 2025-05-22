using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace OnThiLaiXe.Models
{
    public class LoaiBangLai
    {
        public int Id { get; set; }

        [Required]
        public string TenLoai { get; set; }

        public string MoTa { get; set; }
        //them 3 cai nay 
        public string LoaiXe { get; set; }
        public int ThoiGianThi { get; set; }
        public int DiemToiThieu { get; set; }
        //
        public bool isDeleted { get; set; } = false;
        [ValidateNever]
        public ICollection<CauHoi> CauHois { get; set; }
        [ValidateNever]
        public ICollection<BaiSaHinh> BaiSaHinhs { get; set; }
    }
}
