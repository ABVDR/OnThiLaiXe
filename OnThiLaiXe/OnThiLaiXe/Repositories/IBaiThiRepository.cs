using OnThiLaiXe.Models;
using OnThiLaiXe.ModelView;

namespace OnThiLaiXe.Repositories
{
    public interface IBaiThiRepository
    {
        BaiThi GetBaiThiWithDetails(int id);
        (List<KetQuaBaiThi> ketQuaList, float diem, int tongSoCau, int diemToiThieu) ChamDiem(BaiThi baiThi, Dictionary<int, string> answers);
        Task<NopBaiThiResult> XuLyNopBaiThiAsync(SubmitBaiThiRequest request, string userId);
        BaiThi GetDeThiNgauNhien(int loaiBangLaiId);
        (LoaiBangLai LoaiBangLai, List<ChuDe> ChuDeList) GetChuDeByLoaiBangLai(int loaiBangLaiId);
        string GetTenChuDeById(int chuDeId);
        List<ChuDe> GetDanhSachChuDe();
        List<LoaiBangLai> GetDanhSachLoaiBangLai();
        List<LoaiBangLai> GetLoaiBangLaiXeMay();
        List<LoaiBangLai> GetLoaiBangLaiOTo();
        LoaiBangLai GetLoaiBangLaiById(int loaiBangLaiId);
        BaiThi GetChiTietBaiThi(int id);
        List<BaiThi> GetDanhSachBaiThi();
        List<BaiThi> GetDanhSachDeThi(string loaiXe = null);
        List<BaiThi> GetDeThiByLoaiBangLai(int loaiBangLaiId);
        List<CauHoi> GetCauHoiOnTap(int loaiBangLaiId);
        List<CauHoi> GetCauHoiTheoChuDe(int loaiBangLaiId, int chuDeId);
    }
}