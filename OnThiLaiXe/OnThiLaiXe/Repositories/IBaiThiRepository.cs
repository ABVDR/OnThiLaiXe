using OnThiLaiXe.Models;

namespace OnThiLaiXe.Repositories
{
    public interface IBaiThiRepository
    {
        BaiThi GetBaiThiById(int baiThiId, bool includeChiTiet = true);
        List<KetQuaBaiThi> NopBaiThi(int baiThiId, string dapAnJson, string currentUserId, bool isLoggedIn);
        List<CauHoiSaiViewModel> GetDanhSachCauHoiSai(int userId);

        List<CauHoi> GetCauHoiLuyenLaiCauSai(int userId, int maxQuestions = 20);
        List<ChuDe> GetDanhSachChuDe();
        List<LoaiBangLai> GetDanhSachLoaiBangLai();
        BaiThi GetChiTietBaiThi(int id);
        List<BaiThi> GetDanhSachBaiThi();
        List<BaiThi> GetDanhSachDeThi(string loaiXe = null);
        List<LoaiBangLai> GetLoaiBangLaiXeMay();
        List<LoaiBangLai> GetLoaiBangLaiOTo();
        List<BaiThi> GetDeThiByLoaiBangLai(int loaiBangLaiId);
        List<CauHoi> GetCauHoiOnTap(int loaiBangLaiId);
        LoaiBangLai GetLoaiBangLaiById(int loaiBangLaiId);
        bool LuuDapAnTamThoi(DapAnTamThoi request);
    }
}
