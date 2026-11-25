using FreshGo.Data;
using FreshGo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FreshGo.Pages.Admin.StockRequests;
public class IndexModel : PageModel
{
    private readonly FreshGoDbContext _db;
    public IndexModel(FreshGoDbContext db) => _db = db;
    public List<StockRequest> Requests { get; set; } = new();
    public async Task OnGetAsync() => Requests = await _db.StockRequests.Include(x => x.Branch).Include(x => x.Product).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();
        var request = await _db.StockRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (request == null) return NotFound();
        if (request.Status != RequestStates.Pending) { TempData["Error"] = "Yêu cầu đã được xử lý."; return RedirectToPage(); }
        var main = await _db.Warehouses.FirstOrDefaultAsync(x => x.IsMain);
        var target = await _db.Warehouses.FirstOrDefaultAsync(x => x.BranchId == request.BranchId && !x.IsMain);
        if (main == null || target == null) { TempData["Error"] = "Không tìm thấy kho nguồn hoặc kho chi nhánh."; return RedirectToPage(); }
        var sourceStock = await _db.Inventories.FirstOrDefaultAsync(x => x.WarehouseId == main.Id && x.ProductId == request.ProductId);
        if (sourceStock == null || sourceStock.Quantity < request.Quantity) { TempData["Error"] = "Kho tổng không đủ số lượng để duyệt yêu cầu."; return RedirectToPage(); }
        sourceStock.Quantity -= request.Quantity;
        request.Status = RequestStates.Approved;
        request.ApprovedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
        request.ApprovedAt = DateTime.UtcNow;
        _db.StockTransfers.Add(new StockTransfer { StockRequestId = request.Id, SourceWarehouseId = main.Id, TargetWarehouseId = target.Id, ProductId = request.ProductId, Quantity = request.Quantity, Status = "Đang vận chuyển" });
        await _db.SaveChangesAsync(); await tx.CommitAsync();
        TempData["Success"] = "Đã duyệt. Hàng đang được vận chuyển; quản lý chi nhánh cần xác nhận nhận hàng để cộng tồn kho."; return RedirectToPage();
    }
    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var r = await _db.StockRequests.FindAsync(id); if (r == null) return NotFound();
        if (r.Status == RequestStates.Pending) { r.Status = RequestStates.Rejected; r.ApprovedById = User.FindFirstValue(ClaimTypes.NameIdentifier); r.ApprovedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Đã từ chối yêu cầu."; return RedirectToPage();
    }
}
