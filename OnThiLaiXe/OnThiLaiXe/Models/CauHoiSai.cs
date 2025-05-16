namespace OnThiLaiXe.Models
{
    public class CauHoiSai
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CauHoiId { get; set; }
        public DateTime NgaySai { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }

        public virtual CauHoi CauHoi { get; set; }
    }


}
