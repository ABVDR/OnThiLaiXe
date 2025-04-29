namespace OnThiLaiXe.Models
{
    public class KetQuaBaiThi
    {
        public int BaiThiId { get; set; }
        public int CauHoiId { get; set; }
        public CauHoi CauHoi { get; set; }
        public char? CauTraLoi { get; set; }
        public char DapAnDung { get; set; }
        public bool DungSai { get; set; }
    }
}
