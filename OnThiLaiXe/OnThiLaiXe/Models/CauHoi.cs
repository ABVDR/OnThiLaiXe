using System.ComponentModel.DataAnnotations;

namespace OnThiLaiXe.Models
{
    public class CauHoi
    {
        public int Id { get; set; }

       

        [Required]
        public string NoiDung { get; set; }

        [Required]
        public string LuaChonA { get; set; }

        [Required]
        public string LuaChonB { get; set; }

        public string? LuaChonC { get; set; }

        public string? LuaChonD { get; set; }

        [Required]
        public char DapAnDung { get; set; }

        public string? GiaiThich { get; set; }

        public bool DiemLiet { get; set; } = false;

        public string? MediaUrl { get; set; }

        public string? LoaiMedia { get; set; }

        public string? MeoGhiNho { get; set; }
        ////them vao  
        //public string LoaiCauHoi { get; set; }
        public int ChuDeId { get; set; }
        public ChuDe? ChuDe { get; set; }

        public int LoaiBangLaiId { get; set; }
        public LoaiBangLai? LoaiBangLai { get; set; }
        public ICollection<ChiTietBaiThi>? ChiTietBaiThis { get; set; }
        public ICollection<ChiTietLichSuThi> ChiTietLichSuThis { get; set; }
    }
}
