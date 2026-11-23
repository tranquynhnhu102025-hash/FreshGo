using FreshGo.Data;
using FreshGo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FreshGo.Pages.Admin.Vouchers;
public class IndexModel : PageModel
{
    private readonly FreshGoDbContext _db; public IndexModel(FreshGoDbContext db) => _db = db;
    public List<Voucher> Vouchers { get; set; } = new(); [BindProperty] public VoucherInput Input { get; set; } = new();
    public class VoucherInput
    {
        [Required, StringLength(40)] public string Code { get; set; } = ""; public string? Name { get; set; }
        [Range(1, 100)] public decimal DiscountPercent { get; set; } = 10; [Range(0, 100000000)] public decimal MaxDiscount { get; set; } = 50000;
        [Range(0, 100000000)] public decimal MinOrderValue { get; set; } = 100000; [Range(1, 100000)] public int Quantity { get; set; } = 100;
        public DateTime StartAt { get; set; } = DateTime.Now; public DateTime EndAt { get; set; } = DateTime.Now.AddDays(30);
    }
    public async Task OnGetAsync() => Vouchers = await _db.Vouchers.OrderByDescending(x => x.Id).ToListAsync();
    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.EndAt <= Input.StartAt) ModelState.AddModelError("Input.EndAt", "Thời gian kết thúc phải sau thời gian bắt đầu.");
        if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }
        var code = Input.Code.Trim().ToUpperInvariant();
        if (await _db.Vouchers.AnyAsync(x => x.Code == code)) { ModelState.AddModelError("Input.Code", "Mã voucher đã tồn tại."); await OnGetAsync(); return Page(); }
        _db.Vouchers.Add(new Voucher { Code = code, Name = Input.Name ?? "", DiscountPercent = Input.DiscountPercent, MaxDiscount = Input.MaxDiscount, MinOrderValue = Input.MinOrderValue, Quantity = Input.Quantity, StartAt = Input.StartAt.ToUniversalTime(), EndAt = Input.EndAt.ToUniversalTime(), IsActive = true });
        await _db.SaveChangesAsync(); TempData["Success"] = "Đã tạo voucher."; return RedirectToPage();
    }
    public async Task<IActionResult> OnPostToggleAsync(int id) { var v = await _db.Vouchers.FindAsync(id); if (v == null) return NotFound(); v.IsActive = !v.IsActive; await _db.SaveChangesAsync(); return RedirectToPage(); }
}
