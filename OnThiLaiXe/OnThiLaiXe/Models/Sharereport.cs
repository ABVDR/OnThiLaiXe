namespace OnThiLaiXe.Models
{
    public class Sharereport
    {
        public int Id { get; set; }

        // Cho phép báo cáo Share gốc
        public int? ShareId { get; set; }
        public Share? Share { get; set; }

        // Cho phép báo cáo Reply
        public int? ShareReplyId { get; set; }
        public ShareReply? ShareReply { get; set; }

        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
    }
}
