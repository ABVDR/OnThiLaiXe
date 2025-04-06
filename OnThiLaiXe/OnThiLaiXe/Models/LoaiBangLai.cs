using System.ComponentModel.DataAnnotations;

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
        public ICollection<CauHoi> CauHois { get; set; }
    }
}
