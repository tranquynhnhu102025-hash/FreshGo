using FreshGo.Data;
using FreshGo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FreshGo.Pages.Admin.Branches;
public class IndexModel : PageModel
{
    private readonly FreshGoDbContext _db;
    public IndexModel(FreshGoDbContext db) => _db = db;
    public List<Branch> Branches { get; set; } = new();
    [BindProperty] public BranchInput Input { get; set; } = new();
    public class BranchInput { [Required, StringLength(150)] public string Name { get; set; } = ""; [Required, StringLength(300)] public string Address { get; set; } = ""; public string? Phone { get; set; } public string? MapUrl { get; set; } }
    public async Task OnGetAsync() => Branches = await _db.Branches.OrderBy(x => x.Name).ToListAsync();
    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }
        var branch = new Branch { Name = Input.Name.Trim(), Address = Input.Address.Trim(), Phone = Input.Phone, MapUrl = Input.MapUrl };
        _db.Branches.Add(branch); await _db.SaveChangesAsync();
        _db.Warehouses.Add(new Warehouse { Name = "Kho " + branch.Name, BranchId = branch.Id, IsMain = false });
        await _db.SaveChangesAsync(); TempData["Success"] = "Đã tạo chi nhánh và kho chi nhánh."; return RedirectToPage();
    }
    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var b = await _db.Branches.FindAsync(id); if (b == null) return NotFound();
        b.IsActive = !b.IsActive; await _db.SaveChangesAsync(); TempData["Success"] = "Đã cập nhật chi nhánh."; return RedirectToPage();
    }
}
