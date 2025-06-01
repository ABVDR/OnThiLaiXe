using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;
using OnThiLaiXe.ModelView;

namespace OnThiLaiXe.Repositories
{
    public class BaiThiRepository : IBaiThiRepository
    {
        private readonly ApplicationDbContext _context;

        public BaiThiRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public BaiThi GetBaiThiWithDetails(int id)
        {
            return _context.BaiThis
                .Include(b => b.ChiTietBaiThis)
                .ThenInclude(ct => ct.CauHoi)
                .ThenInclude(b => b.LoaiBangLai)
                .FirstOrDefault(b => b.Id == id);
        }

        public (List<KetQuaBaiThi> ketQuaList, float diem, int tongSoCau, int diemToiThieu) ChamDiem(BaiThi baiThi, Dictionary<int, string> answers)
        {
            int totalCorrectAnswers = 0;
            List<KetQuaBaiThi> ketQuaList = new List<KetQuaBaiThi>();

            foreach (var chiTiet in baiThi.ChiTietBaiThis)
            {
                var userAnswer = answers.ContainsKey(chiTiet.Id) ? answers[chiTiet.Id] : null;
                var isCorrect = userAnswer != null && userAnswer.Length > 0 &&
                               chiTiet.CauHoi.DapAnDung.Equals(userAnswer[0]);

                ketQuaList.Add(new KetQuaBaiThi
                {
                    CauHoiId = chiTiet.CauHoiId,
                    CauTraLoi = userAnswer != null && userAnswer.Length > 0 ? userAnswer[0] : ' ',
                    DungSai = isCorrect
                });

                if (isCorrect)
                {
                    totalCorrectAnswers++;
                }
            }

            var diem = (totalCorrectAnswers / (float)baiThi.ChiTietBaiThis.Count) * 10;
            var diemToiThieu = baiThi.ChiTietBaiThis
                             .FirstOrDefault()?.CauHoi?.LoaiBangLai?.DiemToiThieu ?? 0;

            return (ketQuaList, diem, baiThi.ChiTietBaiThis.Count, diemToiThieu);
        }

        public async Task<NopBaiThiResult> XuLyNopBaiThiAsync(SubmitBaiThiRequest request, string userId)
        {
            var baiThi = _context.BaiThis
                .Include(b => b.ChiTietBaiThis)
                .ThenInclude(ct => ct.CauHoi)
                .ThenInclude(ch => ch.LoaiBangLai)
                .FirstOrDefault(b => b.Id == request.BaiThiId);

            if (baiThi == null)
            {
                return new NopBaiThiResult
                {
                    Success = false,
                    Message = "Không tìm thấy bài thi."
                };
            }

            int totalCorrectAnswers = 0;
            List<KetQuaBaiThi> ketQuaList = new List<KetQuaBaiThi>();

            foreach (var chiTiet in baiThi.ChiTietBaiThis)
            {
                var userAnswer = request.Answers.ContainsKey(chiTiet.CauHoiId)
                                   ? request.Answers[chiTiet.CauHoiId]
                                   : null;

                var isCorrect = false;

                if (!string.IsNullOrEmpty(userAnswer))
                {
                    isCorrect = chiTiet.CauHoi.DapAnDung.Equals(userAnswer[0]);
                }

                ketQuaList.Add(new KetQuaBaiThi
                {
                    CauHoiId = chiTiet.CauHoiId,
                    CauTraLoi = userAnswer != null ? userAnswer[0] : (char?)null,
                    DapAnDung = chiTiet.CauHoi.DapAnDung,
                    DungSai = !isCorrect
                });

                if (isCorrect)
                {
                    totalCorrectAnswers++;
                }
            }

            var tongSoCau = baiThi.ChiTietBaiThis.Count;
            var phanTramDung = (double)totalCorrectAnswers / tongSoCau * 100;
            int diem = totalCorrectAnswers;

            var diemToiThieu = baiThi.ChiTietBaiThis
                .FirstOrDefault()?.CauHoi?.LoaiBangLai?.DiemToiThieu ?? 0;

            var ketQua = diem >= diemToiThieu ? "Đậu" : "Không Đạt";

            bool macLoiNghiemTrong = ketQuaList
                .Any(kq => kq.DungSai &&
                    baiThi.ChiTietBaiThis
                        .First(ct => ct.CauHoiId == kq.CauHoiId)
                        .CauHoi.DiemLiet);

            int soCauLoiNghiemTrong = ketQuaList
                .Count(kq => kq.DungSai && baiThi.ChiTietBaiThis
                    .First(ct => ct.CauHoiId == kq.CauHoiId).CauHoi.DiemLiet);

            // Lưu lịch sử thi
            if (!string.IsNullOrEmpty(userId))
            {
                await LuuLichSuThiAsync(userId, baiThi, ketQuaList, tongSoCau, totalCorrectAnswers,
                                      phanTramDung, diem, ketQua, macLoiNghiemTrong);
            }

            return new NopBaiThiResult
            {
                Success = true,
                BaiThiId = baiThi.Id,
                KetQuaList = ketQuaList,
                SoCauDung = diem,
                TongSoCau = tongSoCau,
                TongDiem = diem,
                KetQua = ketQua,
                MacLoiNghiemTrong = macLoiNghiemTrong,
                SoCauLoiNghiemTrong = soCauLoiNghiemTrong
            };
        }

        private async Task LuuLichSuThiAsync(string userId, BaiThi baiThi, List<KetQuaBaiThi> ketQuaList,
            int tongSoCau, int totalCorrectAnswers, double phanTramDung, int diem,
            string ketQua, bool macLoiNghiemTrong)
        {
            var lichSuThi = new LichSuThi
            {
                UserId = userId,
                BaiThiId = baiThi.Id,
                TenBaiThi = baiThi.TenBaiThi.Length > 100 ? baiThi.TenBaiThi.Substring(0, 100) : baiThi.TenBaiThi,
                NgayThi = DateTime.Now,
                TongSoCau = tongSoCau,
                SoCauDung = totalCorrectAnswers,
                PhanTramDung = phanTramDung,
                Diem = diem,
                KetQua = ketQua.Length > 20 ? ketQua.Substring(0, 20) : ketQua,
                MacLoiNghiemTrong = macLoiNghiemTrong,
            };

            _context.LichSuThis.Add(lichSuThi);
            await _context.SaveChangesAsync();

            foreach (var kq in ketQuaList)
            {
                _context.ChiTietLichSuThis.Add(new ChiTietLichSuThi
                {
                    LichSuThiId = lichSuThi.Id,
                    CauHoiId = kq.CauHoiId,
                    CauTraLoi = kq.CauTraLoi,
                    DungSai = kq.DungSai
                });

                if (kq.DungSai)
                {
                    await XuLyCauHoiSaiAsync(userId, kq.CauHoiId);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task XuLyCauHoiSaiAsync(string userId, int cauHoiId)
        {
            var existingSai = await _context.CauHoiSais
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CauHoiId == cauHoiId);

            if (existingSai != null)
            {
                existingSai.NgaySai = DateTime.Now;
            }
            else
            {
                _context.CauHoiSais.Add(new CauHoiSai
                {
                    UserId = userId,
                    CauHoiId = cauHoiId,
                    NgaySai = DateTime.Now
                });
            }
        }

        public BaiThi GetDeThiNgauNhien(int loaiBangLaiId)
        {
            return _context.BaiThis
                .Where(bt => bt.ChiTietBaiThis.Any(ct => ct.CauHoi.LoaiBangLaiId == loaiBangLaiId))
                .OrderBy(x => Guid.NewGuid())
                .FirstOrDefault();
        }

        public (LoaiBangLai LoaiBangLai, List<ChuDe> ChuDeList) GetChuDeByLoaiBangLai(int loaiBangLaiId)
        {
            var loai = _context.LoaiBangLais
                .FirstOrDefault(l => l.Id == loaiBangLaiId && !l.isDeleted);

            var chuDeList = _context.ChuDes
                .Include(cd => cd.CauHois)
                .Where(cd => !cd.isDeleted &&
                             cd.CauHois.Any(ch => ch.LoaiBangLaiId == loaiBangLaiId &&
                                                  !ch.LoaiBangLai.isDeleted &&
                                                  !ch.ChuDe.isDeleted))
                .Distinct()
                .ToList();

            return (loai, chuDeList);
        }


        public string GetTenChuDeById(int chuDeId)
        {
            return _context.ChuDes
                .Where(cd => cd.Id == chuDeId && !cd.isDeleted)
                .Select(cd => cd.TenChuDe)
                .FirstOrDefault() ?? "Không rõ chủ đề";
        }

        public List<ChuDe> GetDanhSachChuDe()
        {
            return _context.ChuDes
                .Where(cd => !cd.isDeleted)
                .ToList();
        }

        public List<LoaiBangLai> GetDanhSachLoaiBangLai()
        {
            return _context.LoaiBangLais
                .Where(l => !l.isDeleted)
                .ToList();
        }

        public BaiThi GetChiTietBaiThi(int id)
        {
            return _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                    .ThenInclude(ct => ct.CauHoi)
                        .ThenInclude(c => c.LoaiBangLai)
                .Include(bt => bt.ChiTietBaiThis)
                    .ThenInclude(ct => ct.CauHoi)
                        .ThenInclude(c => c.ChuDe)
                .FirstOrDefault(bt => bt.Id == id);
        }

        public List<BaiThi> GetDanhSachBaiThi()
        {
            return _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                .ToList();
        }

        public List<BaiThi> GetDanhSachDeThi(string loaiXe = null)
        {
            var query = _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                    .ThenInclude(ct => ct.CauHoi)
                        .ThenInclude(c => c.LoaiBangLai)
                .Include(bt => bt.ChiTietBaiThis)
                    .ThenInclude(ct => ct.CauHoi)
                        .ThenInclude(c => c.ChuDe)
                .AsQueryable();

            if (!string.IsNullOrEmpty(loaiXe))
            {
                query = query.Where(bt => bt.ChiTietBaiThis
                    .Any(ct =>
                        !ct.CauHoi.LoaiBangLai.isDeleted &&
                        ct.CauHoi.LoaiBangLai.LoaiXe == loaiXe));
            }

            return query.ToList();
        }

        public List<LoaiBangLai> GetLoaiBangLaiXeMay()
        {
            return _context.LoaiBangLais
                .Where(l => l.LoaiXe == "Xe máy" && !l.isDeleted)
                .ToList();
        }

        public List<LoaiBangLai> GetLoaiBangLaiOTo()
        {
            return _context.LoaiBangLais
                .Where(l => l.LoaiXe == "Xe oto" && !l.isDeleted)
                .ToList();
        }

        public List<BaiThi> GetDeThiByLoaiBangLai(int loaiBangLaiId)
        {
            return _context.BaiThis
                .Where(bt => bt.ChiTietBaiThis
                    .Any(ct => ct.CauHoi.LoaiBangLaiId == loaiBangLaiId && !ct.CauHoi.LoaiBangLai.isDeleted))
                .Include(bt => bt.ChiTietBaiThis)
                    .ThenInclude(ct => ct.CauHoi)
                        .ThenInclude(c => c.LoaiBangLai)
                .Include(bt => bt.ChiTietBaiThis)
                    .ThenInclude(ct => ct.CauHoi)
                        .ThenInclude(c => c.ChuDe)
                .ToList();
        }

        public List<CauHoi> GetCauHoiOnTap(int loaiBangLaiId)
        {
            return _context.CauHois
                .Where(c => c.LoaiBangLaiId == loaiBangLaiId &&
                            !c.LoaiBangLai.isDeleted &&
                            !c.ChuDe.isDeleted)
                .OrderBy(c => c.ChuDeId)
                .ToList();
        }

        public LoaiBangLai GetLoaiBangLaiById(int loaiBangLaiId)
        {
            return _context.LoaiBangLais
                .FirstOrDefault(l => l.Id == loaiBangLaiId && !l.isDeleted);
        }

        public List<CauHoi> GetCauHoiTheoChuDe(int loaiBangLaiId, int chuDeId)
        {
            return _context.CauHois
                .Where(c => c.LoaiBangLaiId == loaiBangLaiId &&
                            c.ChuDeId == chuDeId &&
                            !c.LoaiBangLai.isDeleted &&
                            !c.ChuDe.isDeleted)
                .OrderBy(c => c.NoiDung)
                .ToList();
        }

    }
}