﻿using System.ComponentModel.DataAnnotations;

namespace OnThiLaiXe.Models
{
    public class LichSuThi
    {
        [Key] // Chỉ định khóa chính
        public int Id { get; set; }
        public int BaiThiId { get; set; }
        public string TenBaiThi { get; set; }
        public DateTime NgayThi { get; set; }
        public string LoaiBaiThi { get; set; }
        public int TongSoCau { get; set; }
        public int SoCauDung { get; set; }
        public double PhanTramDung { get; set; }
        public int Diem { get; set; }
        public string KetQua { get; set; }
        public bool MacLoiNghiemTrong { get; set; }
        public int? UserId { get; set; }
        public ICollection<ChiTietLichSuThi> ChiTietLichSuThis { get; set; }
    }
}
