namespace OnThiLaiXe.Models
{
    public class Share
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string Topic { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
        public string? UserName { get; set; }

    }
}
