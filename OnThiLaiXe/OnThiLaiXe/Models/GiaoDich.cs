namespace OnThiLaiXe.Models
{
    public class GiaoDich
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string MaGiaoDich { get; set; }
        public decimal SoTien { get; set; }
        public DateTime NgayThanhToan { get; set; }
        public bool DaThanhToan { get; set; }

        public ApplicationUser User { get; set; }
    }

}
