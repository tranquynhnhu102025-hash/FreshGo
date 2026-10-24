using FreshGo.Pages.Manager.Models;
using FreshGo.Pages.Manager.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshGo.Pages.Manager.DonHang;

public class IndexModel : PageModel
{
    private readonly ManagerRepository _repo;

    public IndexModel(IConfiguration configuration)
    {
        _repo = new ManagerRepository(configuration);
    }

    public string? TrangThaiLoc { get; set; }
    public List<DonHangListItem> DanhSach { get; set; } = new();

    public async Task OnGetAsync(string? trangThaiLoc)
    {
        TrangThaiLoc = trangThaiLoc;
        DanhSach = await _repo.GetDonHangTheoChiNhanhAsync(BranchContext.CurrentBranchId, trangThaiLoc);
    }
}
