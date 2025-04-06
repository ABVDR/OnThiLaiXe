namespace OnThiLaiXe.Models
{
    public class ChiTietBaiThi
    {
        public int Id { get; set; }
        public int BaiThiId { get; set; }
        public int CauHoiId { get; set; }
        public char? CauTraLoi { get; set; }
        public bool? DungSai { get; set; }

        // Navigation properties
        public virtual BaiThi BaiThi { get; set; }
        public virtual CauHoi CauHoi { get; set; }
    }
}
