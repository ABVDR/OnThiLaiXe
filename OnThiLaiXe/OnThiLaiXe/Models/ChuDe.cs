using System.ComponentModel.DataAnnotations;

namespace OnThiLaiXe.Models
{
    public class ChuDe
    {
        public int Id { get; set; }

        [Required]
        public string TenChuDe { get; set; }

        public string MoTa { get; set; }

        public ICollection<CauHoi> CauHois { get; set; }
    }
}
