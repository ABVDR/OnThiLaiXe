
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public class BaiThiRepository : IBaiThiRepository
    {
        private readonly ApplicationDbContext _context;

        public BaiThiRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public BaiThi GetBaiThiById(int baiThiId, bool includeChiTiet = true)
        {
            if (includeChiTiet)
            {
                return _context.BaiThis
                    .Include(bt => bt.ChiTietBaiThis)
                    .ThenInclude(ct => ct.CauHoi)
                    .ThenInclude(c => c.LoaiBangLai)
                    .FirstOrDefault(bt => bt.Id == baiThiId);
            }
            else
            {
                return _context.BaiThis
                    .FirstOrDefault(bt => bt.Id == baiThiId);
            }
        }

        public List<KetQuaBaiThi> NopBaiThi(int baiThiId, string dapAnJson, string currentUserId, bool isLoggedIn)
        {
            var baiThi = _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                    .ThenInclude(ct => ct.CauHoi)
                .FirstOrDefault(bt => bt.Id == baiThiId);

            if (baiThi == null)
                return null;

            // Parse JSON đáp án
            Dictionary<string, string> dapAnDict = new();
            try
            {
                if (!string.IsNullOrEmpty(dapAnJson))
                    dapAnDict = JsonSerializer.Deserialize<Dictionary<string, string>>(dapAnJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi parse JSON: {ex.Message}");
            }

            var chiTietList = baiThi.ChiTietBaiThis.ToList();
            int correctCount = 0;
            int wrongCount = 0;
            int unansweredCount = 0;
            bool saiDiemLiet = false;

            // Xử lý từng câu trả lời
            for (int i = 0; i < chiTietList.Count; i++)
            {
                string key = $"dapAn_{i}";
                var chiTiet = chiTietList[i];

                if (dapAnDict.ContainsKey(key) && !string.IsNullOrEmpty(dapAnDict[key]))
                {
                    char dapAn = dapAnDict[key][0];
                    chiTiet.CauTraLoi = dapAn;
                    chiTiet.DungSai = dapAn == chiTiet.CauHoi.DapAnDung;

                    if (chiTiet.DungSai == true)
                        correctCount++;
                    else
                    {
                        wrongCount++;
                        if (isLoggedIn)
                            SaveCauHoiSai(true, currentUserId, chiTiet.CauHoi.Id);
                    }

                    if (chiTiet.CauHoi.DiemLiet && dapAn != chiTiet.CauHoi.DapAnDung)
                        saiDiemLiet = true;
                }
                else
                {
                    chiTiet.CauTraLoi = '\0';
                    chiTiet.DungSai = false;
                    unansweredCount++;

                    if (chiTiet.CauHoi.DiemLiet)
                        saiDiemLiet = true;

                    if (isLoggedIn)
                        SaveCauHoiSai(true, currentUserId, chiTiet.CauHoi.Id);
                }
            }

            // Lấy loại bằng để kiểm tra điều kiện đậu
            var loaiBang = baiThi.ChiTietBaiThis.FirstOrDefault()?.CauHoi?.LoaiBangLai;
            int diemToiThieu = loaiBang?.DiemToiThieu ?? 21;

            // Mỗi câu đúng = 1 điểm, không tính điểm nếu sai câu điểm liệt
            int tongDiem = correctCount;

            baiThi.Diem = tongDiem;
            baiThi.MacLoiNghiemTrong = saiDiemLiet;
            baiThi.SoCauDung = correctCount;
            baiThi.SoCauSai = wrongCount;
            baiThi.SoCauChuaTraLoi = unansweredCount;
            baiThi.PhanTramDung = chiTietList.Count > 0 ? (double)correctCount / chiTietList.Count * 100 : 0;
            baiThi.KetQua = (tongDiem >= diemToiThieu && !saiDiemLiet) ? "Đạt" : "Không đạt";
            baiThi.DaHoanThanh = true;

            _context.SaveChanges();

            // Chuẩn bị dữ liệu hiển thị kết quả
            var ketQuaList = chiTietList.Select(ct => new KetQuaBaiThi
            {
                BaiThiId = baiThiId,
                CauHoiId = ct.CauHoi.Id,
                CauHoi = ct.CauHoi,
                CauTraLoi = ct.CauTraLoi ?? '\0',
                DungSai = ct.DungSai ?? false
            }).ToList();

            return ketQuaList;
        }

        private void SaveCauHoiSai(bool isLoggedIn, string currentUserId, int cauHoiId)
        {
            if (!isLoggedIn || string.IsNullOrEmpty(currentUserId)) return;

            try
            {
                int userId;
                if (int.TryParse(currentUserId, out userId))
                {
                    _context.CauHoiSais.Add(new CauHoiSai
                    {
                        UserId = userId,
                        CauHoiId = cauHoiId,
                        NgaySai = DateTime.Now
                    });
                }
                else if (Guid.TryParse(currentUserId, out var userGuid))
                {
                    // Sử dụng int.MaxValue & operation để tránh overflow 
                    // nếu hashcode là số âm
                    int hashUserId = userGuid.GetHashCode() & int.MaxValue;
                    _context.CauHoiSais.Add(new CauHoiSai
                    {
                        UserId = hashUserId,
                        CauHoiId = cauHoiId,
                        NgaySai = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw exception để tiếp tục xử lý
                Console.WriteLine($"Lỗi khi lưu câu hỏi sai: {ex.Message}");
            }
        }

        public List<CauHoiSaiViewModel> GetDanhSachCauHoiSai(int userId)
        {
            return _context.CauHoiSais
                .Where(c => c.UserId == userId)
                .Include(c => c.CauHoi)
                .ThenInclude(ch => ch.ChuDe)
                .GroupBy(c => c.CauHoiId)
                .Select(g => new CauHoiSaiViewModel
                {
                    CauHoi = g.First().CauHoi,
                    SoLanSai = g.Count(),
                    LanSaiGanNhat = g.Max(c => c.NgaySai)
                })
                .OrderByDescending(c => c.LanSaiGanNhat)
                .ToList();
        }

      
        public List<CauHoi> GetCauHoiLuyenLaiCauSai(int userId, int maxQuestions = 20)
        {
            var cauHoiIds = _context.CauHoiSais
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.NgaySai)
                .Select(c => c.CauHoiId)
                .Distinct()
                .Take(maxQuestions)
                .ToList();

            return _context.CauHois
                .Where(c => cauHoiIds.Contains(c.Id))
                .ToList();
        }

        public List<ChuDe> GetDanhSachChuDe()
        {
            return _context.ChuDes.ToList();
        }

        public List<LoaiBangLai> GetDanhSachLoaiBangLai()
        {
            return _context.LoaiBangLais.ToList();
        }

        public BaiThi GetChiTietBaiThi(int id)
        {
            return _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                .ThenInclude(ct => ct.CauHoi)
                .FirstOrDefault(bt => bt.Id == id);
        }

        public List<BaiThi> GetDanhSachBaiThi()
        {
            return _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                .OrderByDescending(bt => bt.NgayThi)
                .ToList();
        }

        public List<BaiThi> GetDanhSachDeThi(string loaiXe = null)
        {
            var query = _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                    .ThenInclude(ct => ct.CauHoi)
                        .ThenInclude(c => c.LoaiBangLai)
                .AsQueryable(); // Ép kiểu về IQueryable<BaiThi>

            if (!string.IsNullOrEmpty(loaiXe))
            {
                query = query.Where(bt => bt.ChiTietBaiThis
                       .Any(ct => ct.CauHoi.LoaiBangLai.LoaiXe == loaiXe));
            }

            return query.ToList();
        }


        public List<LoaiBangLai> GetLoaiBangLaiXeMay()
        {
            return _context.LoaiBangLais.Where(l => l.LoaiXe == "Xe máy").ToList();
        }

        public List<LoaiBangLai> GetLoaiBangLaiOTo()
        {
            return _context.LoaiBangLais.Where(l => l.LoaiXe == "Xe oto").ToList();
        }

        public List<BaiThi> GetDeThiByLoaiBangLai(int loaiBangLaiId)
        {
            return _context.BaiThis
                .Where(bt => bt.ChiTietBaiThis.Any(ct => ct.CauHoi.LoaiBangLaiId == loaiBangLaiId))
                .Include(bt => bt.ChiTietBaiThis)
                .ThenInclude(ct => ct.CauHoi)
                .ToList();
        }

        public List<CauHoi> GetCauHoiOnTap(int loaiBangLaiId)
        {
            return _context.CauHois
                .Where(c => c.LoaiBangLaiId == loaiBangLaiId)
                .OrderBy(c => c.ChuDeId)
                .ToList();
        }

        public LoaiBangLai GetLoaiBangLaiById(int loaiBangLaiId)
        {
            return _context.LoaiBangLais.FirstOrDefault(l => l.Id == loaiBangLaiId);
        }

        public bool LuuDapAnTamThoi(DapAnTamThoi request)
        {
            try
            {
                // Implement your logic to save temporary answers
                // This could use a cache, session, or separate database table
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}