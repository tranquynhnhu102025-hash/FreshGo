using FreshGo.Pages.Manager.Models;
using FreshGo.Pages.Manager.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshGo.Pages.Manager;

public class IndexModel : PageModel
{
    private readonly ManagerRepository _repo;

    public IndexModel(IConfiguration configuration)
    {
        _repo = new ManagerRepository(configuration);
    }

    public ChiNhanhInfo? ChiNhanh { get; set; }
    public DashboardStats Stats { get; set; } = new();
    public DoanhThuSummary DoanhThuHomNay { get; set; } = new();

    public async Task OnGetAsync()
    {
        ChiNhanh = await _repo.GetChiNhanhAsync(BranchContext.CurrentBranchId);
        Stats = await _repo.GetDashboardStatsAsync(BranchContext.CurrentBranchId);
        DoanhThuHomNay = await _repo.GetDoanhThuSummaryAsync(BranchContext.CurrentBranchId, DateTime.Today, DateTime.Today.AddDays(1));
    }
}
