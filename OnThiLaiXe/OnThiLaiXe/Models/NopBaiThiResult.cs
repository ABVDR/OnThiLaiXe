namespace OnThiLaiXe.Models
{
    public class NopBaiThiResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int BaiThiId { get; set; }
        public List<KetQuaBaiThi> KetQuaList { get; set; } = new List<KetQuaBaiThi>();
        public int SoCauDung { get; set; }
        public int TongSoCau { get; set; }
        public int TongDiem { get; set; }
        public string KetQua { get; set; } = string.Empty;
        public bool MacLoiNghiemTrong { get; set; }
        public int SoCauLoiNghiemTrong { get; set; }
    }
}
