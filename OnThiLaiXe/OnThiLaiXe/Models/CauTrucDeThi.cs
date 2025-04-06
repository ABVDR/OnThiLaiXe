namespace OnThiLaiXe.Models
{
    public class CauTrucDeThi
    {
        public int Id { get; set; }
        public int LoaiBangLaiId { get; set; }
        public int ChuDeId { get; set; }
        public int SoLuongCauHoi { get; set; }

        public LoaiBangLai LoaiBangLai { get; set; }
        public ChuDe ChuDe { get; set; }
    }
}
