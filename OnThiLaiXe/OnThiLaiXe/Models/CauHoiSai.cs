namespace OnThiLaiXe.Models
{
    public class CauHoiSai
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CauHoiId { get; set; }
        public DateTime NgaySai { get; set; }

        public ApplicationUser User { get; set; }

        // Navigation properties
        public virtual CauHoi CauHoi { get; set; }
    }


}
