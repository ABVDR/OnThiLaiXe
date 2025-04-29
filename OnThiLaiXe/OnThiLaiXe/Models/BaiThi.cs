namespace OnThiLaiXe.Models
{
    // Mở rộng model BaiThi với các trường mới
    public partial class BaiThi
    {
        public int Id { get; set; }
        public string TenBaiThi { get; set; }


        // Navigation properties
        public virtual ICollection<ChiTietBaiThi> ChiTietBaiThis { get; set; }
    }
}