using FreshGo.Data;
using FreshGo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FreshGo.Pages.Admin.Products;
public class IndexModel : PageModel
{
    private readonly FreshGoDbContext _db;
    public IndexModel(FreshGoDbContext db) => _db = db;
    public List<Product> Products { get; set; } = new();
    public SelectList CategoryOptions { get; set; } = default!;
    [BindProperty] public ProductInput Input { get; set; } = new();
    public class ProductInput
    {
        [Required, StringLength(180)] public string Name { get; set; } = "";
        [Range(1, int.MaxValue)] public int CategoryId { get; set; }
        [Range(0, 100000000)] public decimal Price { get; set; }
        [Range(0, 100000000)] public decimal CostPrice { get; set; }
        public string Unit { get; set; } = "kg";
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
    }
    public async Task OnGetAsync() => await LoadAsync();
    private async Task LoadAsync()
    {
        Products = await _db.Products.Include(x => x.Category).OrderByDescending(x => x.Id).ToListAsync();
        CategoryOptions = new SelectList(await _db.Categories.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
    }
    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid) { await LoadAsync(); return Page(); }
        _db.Products.Add(new Product { Name = Input.Name.Trim(), CategoryId = Input.CategoryId, Price = Input.Price, CostPrice = Input.CostPrice, Unit = Input.Unit, ImageUrl = Input.ImageUrl, Description = Input.Description });
        await _db.SaveChangesAsync(); TempData["Success"] = "Đã thêm sản phẩm."; return RedirectToPage();
    }
    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var p = await _db.Products.FindAsync(id); if (p == null) return NotFound();
        p.IsActive = !p.IsActive; await _db.SaveChangesAsync(); TempData["Success"] = "Đã cập nhật trạng thái sản phẩm."; return RedirectToPage();
    }
}
