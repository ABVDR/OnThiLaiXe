namespace OnThiLaiXe.Models
{
    // Mở rộng model BaiThi với các trường mới
    public partial class BaiThi
    {
        public int Id { get; set; }
        public DateTime NgayThi { get; set; }
        public string TenBaiThi { get; set; }
        public string LoaiBaiThi { get; set; } // Bài thi thử, chính thức, etc.
        public bool DaHoanThanh { get; set; }
        public int? Diem { get; set; }
        public string KetQua { get; set; }
        public bool MacLoiNghiemTrong { get; set; }
        public int SoCauDung { get; set; }
        public int SoCauSai { get; set; }
        public int SoCauChuaTraLoi { get; set; }
        public double PhanTramDung { get; set; }

        // Foreign key
        public int? UserId { get; set; }

        // Navigation properties
        public virtual ICollection<ChiTietBaiThi> ChiTietBaiThis { get; set; }

    }
}