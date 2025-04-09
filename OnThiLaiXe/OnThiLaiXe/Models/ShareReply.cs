namespace OnThiLaiXe.Models
{
    public class ShareReply
    {
        public int Id { get; set; }
        public int ShareId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
        public string? UserName { get; set; }

        public int? ParentReplyId { get; set; }
        public Share Share { get; set; }
    }
}
