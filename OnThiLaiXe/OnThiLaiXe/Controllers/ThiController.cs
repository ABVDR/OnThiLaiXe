using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OnThiLaiXe.Models;

namespace OnThiLaiXe.Controllers
{
    public class ThiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị trang thi với danh sách câu hỏi ngẫu nhiên
        public async Task<IActionResult> Index()
        {
            var cauHoiList = await _context.CauHois
                .OrderBy(x => Guid.NewGuid()) // Chọn ngẫu nhiên
                .Take(10)
                .ToListAsync();

            // Lưu danh sách câu hỏi với thứ tự đúng
            TempData["CauHoiList"] = JsonConvert.SerializeObject(
                cauHoiList.Select((q, index) => new { Id = q.Id, ThuTu = index + 1 }).ToList()
            );

            return View(cauHoiList);
        }


        [HttpPost]
        public async Task<IActionResult> NopBai(Dictionary<int, char>? dapAnNguoiDung)
        {
            // Lấy danh sách câu hỏi từ TempData
            if (TempData["CauHoiList"] == null)
            {
                return BadRequest("Danh sách câu hỏi không tồn tại.");
            }

            var cauHoiData = JsonConvert.DeserializeObject<List<dynamic>>(TempData["CauHoiList"].ToString());
            var cauHoiIds = cauHoiData.Select(q => (int)q.Id).ToList();

            // Lấy câu hỏi từ database theo đúng danh sách đã chọn
            var cauHoiList = await _context.CauHois
                .Where(q => cauHoiIds.Contains(q.Id))
                .ToListAsync();

            // Giữ đúng thứ tự câu hỏi
            var cauHoiOrdered = cauHoiData
                .Select(q => new { ThuTu = (int)q.ThuTu, CauHoi = cauHoiList.First(ch => ch.Id == (int)q.Id) })
                .OrderBy(q => q.ThuTu)
                .ToList();

            // Tạo bài thi mới
            var baiThi = new BaiThi
            {
                NgayThi = DateTime.UtcNow,
                Diem = 0,
                MacLoiNghiemTrong = false
            };

            _context.BaiThis.Add(baiThi);
            await _context.SaveChangesAsync();

            var chiTietBaiThiList = new List<ChiTietBaiThi>();
            int diem = 0;
            bool macLoiNghiemTrong = false;

            foreach (var item in cauHoiOrdered)
            {
                var cauHoi = item.CauHoi;
                char dapAnNguoiChon = ' ';
                bool dungSai = false;

                if (dapAnNguoiDung != null && dapAnNguoiDung.TryGetValue(cauHoi.Id, out char dapAn))
                {
                    dapAnNguoiChon = dapAn;
                    dungSai = dapAn == cauHoi.DapAnDung;

                    if (dungSai) diem++;
                    if (cauHoi.DiemLiet && !dungSai) macLoiNghiemTrong = true;
                }

                chiTietBaiThiList.Add(new ChiTietBaiThi
                {
                    BaiThiId = baiThi.Id,
                    CauHoiId = cauHoi.Id,
                    CauTraLoi = dapAnNguoiChon,
                    DungSai = dungSai
                });
            }

            // Cập nhật điểm bài thi
            baiThi.Diem = diem;
            baiThi.MacLoiNghiemTrong = macLoiNghiemTrong;
            _context.BaiThis.Update(baiThi);

            // Lưu chi tiết bài thi
            _context.ChiTietBaiThis.AddRange(chiTietBaiThiList);
            await _context.SaveChangesAsync();

            return RedirectToAction("KetQua", new { id = baiThi.Id });
        }



        // Trang kết quả bài thi
        public async Task<IActionResult> KetQua(int id)
        {
            var baiThi = await _context.BaiThis
                .Include(bt => bt.ChiTietBaiThis)
                .ThenInclude(ct => ct.CauHoi)
                .FirstOrDefaultAsync(bt => bt.Id == id);

            if (baiThi == null)
            {
                return NotFound();
            }

            // Sắp xếp lại câu hỏi theo thứ tự đã lưu
            baiThi.ChiTietBaiThis = baiThi.ChiTietBaiThis
                .OrderBy(ct => ct.Id) // Hoặc dùng cột khác nếu có thứ tự lưu
                .ToList();

            return View(baiThi);
        }

    }
}
