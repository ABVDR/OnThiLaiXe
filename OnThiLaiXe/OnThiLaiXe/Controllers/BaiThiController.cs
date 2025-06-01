using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OnThiLaiXe.Models;
using OnThiLaiXe.Repositories;

namespace OnThiLaiXe.Controllers
{
    public class BaiThiController : Controller
    {
        private readonly IBaiThiRepository _baiThiRepo;

        public BaiThiController(IBaiThiRepository baiThiRepo)
        {
            _baiThiRepo = baiThiRepo;
        }

       
        public IActionResult LamBaiThi(int id, Dictionary<int, string> answers)
        {
            var baiThi = _baiThiRepo.GetBaiThiWithDetails(id);

            if (baiThi == null)
            {
                return NotFound();
            }

            var (ketQuaList, diem, tongSoCau, diemToiThieu) = _baiThiRepo.ChamDiem(baiThi, answers);

            ViewBag.KetQuaList = ketQuaList;
            ViewBag.TongSoCau = tongSoCau;
            ViewBag.DiemToiThieu = diemToiThieu;
            ViewBag.Diem = diem;

            return View(baiThi);
        }

        [HttpPost]
        public async Task<IActionResult> NopBaiThiAjax([FromBody] SubmitBaiThiRequest request)
        {
            if (request == null || request.BaiThiId == 0 || request.Answers == null)
            {
                return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ!" });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _baiThiRepo.XuLyNopBaiThiAsync(request, userId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new
                {
                    success = true,
                    baiThiId = result.BaiThiId,
                    ketQuaList = result.KetQuaList.Select(kq => new
                    {
                        kq.CauHoiId,
                        kq.CauTraLoi,
                        kq.DapAnDung,
                        kq.DungSai,
                    }),
                    soCauDung = result.SoCauDung,
                    tongSoCau = result.TongSoCau,
                    tongDiem = result.TongDiem,
                    ketQua = result.KetQua,
                    macLoiNghiemTrong = result.MacLoiNghiemTrong,
                    soCauLoiNghiemTrong = result.SoCauLoiNghiemTrong
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi khi xử lý bài thi.",
                    error = ex.Message
                });
            }
        }

        public IActionResult ChonDeThi()
        {
            ViewBag.DanhSachChuDe = _baiThiRepo.GetDanhSachChuDe();
            ViewBag.DanhSachLoaiBangLai = _baiThiRepo.GetDanhSachLoaiBangLai();

            return View();
        }

        public IActionResult ChiTietBaiThi(int id)
        {
            var baiThi = _baiThiRepo.GetChiTietBaiThi(id);

            if (baiThi == null)
            {
                return NotFound();
            }

            return View(baiThi);
        }

        public IActionResult DanhSachBaiThi()
        {
            var baiThis = _baiThiRepo.GetDanhSachBaiThi();
            return View(baiThis);
        }

        public IActionResult DanhSachDeThi(string loaiXe)
        {
            var danhSachDeThi = _baiThiRepo.GetDanhSachDeThi(loaiXe);
            return View(danhSachDeThi);
        }

        public IActionResult LoaiBangLaiXeMay()
        {
            var danhSachLoaiBangLaiXeMay = _baiThiRepo.GetLoaiBangLaiXeMay();

            if (danhSachLoaiBangLaiXeMay == null || !danhSachLoaiBangLaiXeMay.Any())
            {
                ViewBag.ErrorMessage = "Không tìm thấy loại bằng lái xe máy!";
                return View();
            }

            return View(danhSachLoaiBangLaiXeMay);
        }

        public IActionResult LoaiBangLaiOTo()
        {
            var danhSachLoaiBangLaiOTo = _baiThiRepo.GetLoaiBangLaiOTo();
            return View(danhSachLoaiBangLaiOTo);
        }

        public IActionResult ThiDe(int loaiBangLaiId)
        {
            var loaiBangLai = _baiThiRepo.GetLoaiBangLaiById(loaiBangLaiId);
            if (loaiBangLai == null)
            {
                return NotFound();
            }

            var danhSachDeThi = _baiThiRepo.GetDeThiByLoaiBangLai(loaiBangLaiId);

            ViewBag.LoaiBangLai = loaiBangLai;
            return View(danhSachDeThi);
        }

        public IActionResult LamDeNgauNhien(int loaiBangLaiId)
        {
            var deThiNgauNhien = _baiThiRepo.GetDeThiNgauNhien(loaiBangLaiId);

            if (deThiNgauNhien == null)
                return NotFound("Không có đề thi nào phù hợp.");

            return RedirectToAction("LamBaiThi", new { id = deThiNgauNhien.Id });
        }

        public IActionResult OnTap(int loaiBangLaiId)
        {
            var cauHoiList = _baiThiRepo.GetCauHoiOnTap(loaiBangLaiId);

            if (!cauHoiList.Any())
            {
                return RedirectToAction("DanhSachLoaiBangLai");
            }

            ViewBag.LoaiBangLai = _baiThiRepo.GetLoaiBangLaiById(loaiBangLaiId);
            return View(cauHoiList);
        }

        public IActionResult OnTapChuDe(int loaiBangLaiId, int chuDeId)
        {
            var cauHoiList = _baiThiRepo.GetCauHoiTheoChuDe(loaiBangLaiId, chuDeId);

            if (!cauHoiList.Any())
            {
                TempData["Error"] = "Không có câu hỏi nào cho chủ đề này.";
                return RedirectToAction("OnTap", new { loaiBangLaiId });
            }

            ViewBag.LoaiBangLai = _baiThiRepo.GetLoaiBangLaiById(loaiBangLaiId);
            ViewBag.TenChuDe = _baiThiRepo.GetTenChuDeById(chuDeId);

            return View("OnTapChuDe", cauHoiList);
        }

        public IActionResult ChonChuDe(int loaiBangLaiId)
        {
            var chuDeInfo = _baiThiRepo.GetChuDeByLoaiBangLai(loaiBangLaiId);

            if (chuDeInfo.LoaiBangLai == null)
            {
                return NotFound();
            }

            ViewBag.TenLoai = chuDeInfo.LoaiBangLai.TenLoai;
            ViewBag.LoaiBangLaiId = loaiBangLaiId;

            return View("ChonChuDe", chuDeInfo.ChuDeList);
        }

        public IActionResult DanhSachLoaiBangLai()
        {
            var danhSachLoaiBangLai = _baiThiRepo.GetDanhSachLoaiBangLai();
            return View(danhSachLoaiBangLai);
        }
    }
}