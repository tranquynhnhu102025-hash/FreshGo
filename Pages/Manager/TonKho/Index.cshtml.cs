using FreshGo.Pages.Manager.Models;
using FreshGo.Pages.Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshGo.Pages.Manager.TonKho;

public class IndexModel : PageModel
{
    private readonly ManagerRepository _repo;

    public IndexModel(IConfiguration configuration)
    {
        _repo = new ManagerRepository(configuration);
    }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DanhMuc { get; set; }

    public List<TonKhoItem> DanhSach { get; set; } = new();
    public List<DanhMucOption> DanhMucList { get; set; } = new();

    public async Task OnGetAsync()
    {
        DanhMucList = await _repo.GetDanhMucAsync();
        DanhSach = await _repo.GetTonKhoAsync(BranchContext.CurrentBranchId, Search, DanhMuc);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _repo.DeleteTonKhoAsync(id, BranchContext.CurrentBranchId);
        TempData["Thanhcong"] = "Đã ngừng kinh doanh mặt hàng tại chi nhánh.";
        return RedirectToPage(new { Search, DanhMuc });
    }
}
