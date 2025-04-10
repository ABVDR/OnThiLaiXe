namespace OnThiLaiXe.Models
{
    public class ChiTietLichSuThi
    {
        public int Id { get; set; }
        public int LichSuThiId { get; set; }
        public int CauHoiId { get; set; }
        public string? CauTraLoi { get; set; }
        public bool? DungSai { get; set; }

        public LichSuThi LichSuThi { get; set; }
        public CauHoi CauHoi { get; set; }
    }

}
