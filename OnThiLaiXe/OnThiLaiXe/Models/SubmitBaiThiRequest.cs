namespace OnThiLaiXe.Models
{
    public class SubmitBaiThiRequest
    {
        public int BaiThiId { get; set; }
        public Dictionary<int, string> Answers { get; set; }
    }
}
