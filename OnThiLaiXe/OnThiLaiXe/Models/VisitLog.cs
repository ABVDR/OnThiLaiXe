namespace OnThiLaiXe.Models
{
    public class VisitLog
    {
        public int Id { get; set; }
        public DateTime VisitTime { get; set; }
        public string VisitorId { get; set; } // Để theo dõi số khách đang truy cập
    }
}
