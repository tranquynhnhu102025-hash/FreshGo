using FreshGo.Pages.Manager.Models;
using FreshGo.Pages.Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshGo.Pages.Manager.TonKho;

public class YeuCauHangModel : PageModel
{
    private readonly ManagerRepository _repo;

    public YeuCauHangModel(IConfiguration configuration)
    {
        _repo = new ManagerRepository(configuration);
    }

    public List<TonKhoItem> DanhSach { get; set; } = new();
    public List<YeuCauNhapHangDong> DangSoan { get; set; } = new();

    public async Task OnGetAsync()
    {
        DanhSach = await _repo.GetTonKhoAsync(BranchContext.CurrentBranchId, search: null, maDanhMuc: null, trangThaiLoc: TrangThaiOptions.HetHang);
        DangSoan = await _repo.GetChiTietYeuCauDangSoanAsync(BranchContext.CurrentBranchId);
    }

    public async Task<IActionResult> OnPostThemAsync(int maThucPham, int soLuong)
    {
        if (soLuong > 0)
        {
            await _repo.ThemVaoYeuCauNhapHangAsync(BranchContext.CurrentBranchId, maThucPham, soLuong);
            TempData["Thanhcong"] = "Đã thêm vào yêu cầu nhập hàng đang soạn.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostXoaAsync(int maChiTiet)
    {
        await _repo.XoaDongYeuCauNhapHangAsync(maChiTiet, BranchContext.CurrentBranchId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGuiAsync()
    {
        var thanhCong = await _repo.GuiYeuCauNhapHangAsync(BranchContext.CurrentBranchId);
        TempData[thanhCong ? "Thanhcong" : "Loi"] = thanhCong
            ? "Đã gửi yêu cầu nhập hàng lên kho tổng."
            : "Chưa có món nào trong yêu cầu để gửi.";
        return RedirectToPage();
    }
}
