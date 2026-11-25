using FreshGo.Data;
using FreshGo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FreshGo.Pages.Admin.Defects;
public class IndexModel : PageModel
{
    private readonly FreshGoDbContext _db; public IndexModel(FreshGoDbContext db) => _db = db;
    public List<DefectiveProduct> Defects { get; set; } = new();
    public async Task OnGetAsync() => Defects = await _db.DefectiveProducts.Include(x => x.Branch).Include(x => x.Product).OrderByDescending(x => x.CreatedAt).ToListAsync();
    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var defect = await _db.DefectiveProducts.FirstOrDefaultAsync(x => x.Id == id);
        if (defect == null) return NotFound();
        if (defect.Status != "Chờ xác nhận") return RedirectToPage();
        var warehouse = await _db.Warehouses.FirstOrDefaultAsync(x => x.BranchId == defect.BranchId && !x.IsMain);
        var stock = warehouse == null ? null : await _db.Inventories.FirstOrDefaultAsync(x => x.WarehouseId == warehouse.Id && x.ProductId == defect.ProductId);
        if (stock == null || stock.Quantity < defect.Quantity) { TempData["Error"] = "Tồn kho thực tế không đủ để trừ theo báo cáo. Vui lòng kiểm kê lại."; return RedirectToPage(); }
        stock.Quantity -= defect.Quantity; defect.Status = "Đã xác nhận"; defect.ReviewedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _db.SaveChangesAsync(); TempData["Success"] = "Đã xác nhận hàng lỗi và cập nhật tồn kho chi nhánh."; return RedirectToPage();
    }
    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var defect = await _db.DefectiveProducts.FindAsync(id); if (defect == null) return NotFound();
        if (defect.Status == "Chờ xác nhận") { defect.Status = "Từ chối"; defect.ReviewedById = User.FindFirstValue(ClaimTypes.NameIdentifier); await _db.SaveChangesAsync(); }
        TempData["Success"] = "Đã từ chối báo cáo hàng lỗi."; return RedirectToPage();
    }
}
