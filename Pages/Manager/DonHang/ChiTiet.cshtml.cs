using FreshGo.Pages.Manager.Models;
using FreshGo.Pages.Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshGo.Pages.Manager.DonHang;

public class ChiTietModel : PageModel
{
    private readonly ManagerRepository _repo;

    public ChiTietModel(IConfiguration configuration)
    {
        _repo = new ManagerRepository(configuration);
    }

    public DonHangChiTiet? DonHang { get; set; }
    public List<ShipperOption> DanhSachShipper { get; set; } = new();

    [BindProperty]
    public int? MaShipperChon { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        DonHang = await _repo.GetChiTietDonHangAsync(id, BranchContext.CurrentBranchId);
        if (DonHang is null) return NotFound();

        if (DonHang.TrangThaiDon == TrangThaiDonOptions.ChoXuLy)
        {
            DanhSachShipper = await _repo.GetShipperTheoChiNhanhAsync(BranchContext.CurrentBranchId);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostXuLyAsync(int id)
    {
        if (MaShipperChon is null or <= 0)
        {
            TempData["Loi"] = "Vui lòng chọn người giao hàng trước khi xử lý đơn.";
            return RedirectToPage(new { id });
        }

        var thanhCong = await _repo.XuLyDonHangAsync(id, BranchContext.CurrentBranchId, MaShipperChon.Value);
        TempData[thanhCong ? "Thanhcong" : "Loi"] = thanhCong
            ? "Đã xử lý đơn hàng và gán người giao hàng thành công."
            : "Không thể xử lý đơn này (có thể đã được xử lý trước đó).";
        return RedirectToPage(new { id });
    }
}
