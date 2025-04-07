namespace OnThiLaiXe.Models
{
    public class BaiSaHinh
    {
        public int Id { get; set; }
        public string TenBai { get; set; }
        public int Order { get; set; }
        public string NoiDung { get; set; }
        public int LoaiBangLaiId { get; set; }
        public LoaiBangLai? LoaiBangLai { get; set; }
    }
}
