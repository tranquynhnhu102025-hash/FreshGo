using FreshGo.Data;
using FreshGo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FreshGo.Pages.Admin.Gifts;
public class IndexModel : PageModel
{
    private readonly FreshGoDbContext _db; public IndexModel(FreshGoDbContext db) => _db = db;
    public List<Gift> Gifts { get; set; } = new(); [BindProperty] public GiftInput Input { get; set; } = new();
    public class GiftInput { [Required, StringLength(150)] public string Name { get; set; } = ""; public string? Description { get; set; } [Range(1, 100000)] public int PointsRequired { get; set; } [Range(0, 100000)] public int Quantity { get; set; } }
    public async Task OnGetAsync() => Gifts = await _db.Gifts.OrderBy(x => x.PointsRequired).ToListAsync();
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }
        _db.Gifts.Add(new Gift { Name = Input.Name, Description = Input.Description, PointsRequired = Input.PointsRequired, Quantity = Input.Quantity }); await _db.SaveChangesAsync(); TempData["Success"] = "Đã thêm quà tặng."; return RedirectToPage();
    }
    public async Task<IActionResult> OnPostToggleAsync(int id) { var g = await _db.Gifts.FindAsync(id); if (g == null) return NotFound(); g.IsActive = !g.IsActive; await _db.SaveChangesAsync(); return RedirectToPage(); }
}
