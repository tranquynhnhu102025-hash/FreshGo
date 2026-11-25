using FreshGo.Data;
using FreshGo.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FreshGo.Pages.Admin;
public class IndexModel : PageModel
{
    private readonly FreshGoDbContext _db;
    public IndexModel(FreshGoDbContext db) => _db = db;
    public record Kpi(string Label, string Value, string Hint);
    public List<Kpi> Kpis { get; set; } = new();
    public List<Order> RecentOrders { get; set; } = new();
    public record BranchRevenueRow(string BranchName, decimal Revenue, int OrderCount);
    public List<BranchRevenueRow> BranchRevenue { get; set; } = new();
    public int PendingRequests { get; set; }
    public async Task OnGetAsync()
    {
        var revenue = await _db.Orders.Where(x => x.Status == OrderStates.Completed || x.Status == OrderStates.Delivered).SumAsync(x => (decimal?)x.TotalAmount) ?? 0;
        Kpis = new() {
            new("Tổng doanh thu", revenue.ToString("N0") + " đ", "Đơn đã giao / hoàn tất"),
            new("Đơn hàng", (await _db.Orders.CountAsync()).ToString("N0"), "Tất cả trạng thái"),
            new("Chi nhánh", (await _db.Branches.CountAsync(x => x.IsActive)).ToString(), "Đang hoạt động"),
            new("Sản phẩm", (await _db.Products.CountAsync(x => x.IsActive)).ToString(), "Đang kinh doanh")
        };
        PendingRequests = await _db.StockRequests.CountAsync(x => x.Status == RequestStates.Pending);
        RecentOrders = await _db.Orders.Include(x => x.Branch).OrderByDescending(x => x.CreatedAt).Take(8).ToListAsync();
        var branchList = await _db.Branches.ToListAsync();
        var totals = await _db.Orders.Where(x => x.Status == OrderStates.Completed || x.Status == OrderStates.Delivered)
            .GroupBy(x => x.BranchId)
            .Select(g => new { BranchId = g.Key, Revenue = g.Sum(x => x.TotalAmount), OrderCount = g.Count() })
            .ToListAsync();
        BranchRevenue = totals.Select(x => new BranchRevenueRow(branchList.FirstOrDefault(b => b.Id == x.BranchId)?.Name ?? "Chi nhánh", x.Revenue, x.OrderCount))
            .OrderByDescending(x => x.Revenue).ToList();
    }
}
