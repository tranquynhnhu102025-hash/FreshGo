using FreshGo.Data;
using FreshGo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FreshGo.Pages.Admin.Events;
public class IndexModel : PageModel
{
    private readonly FreshGoDbContext _db; public IndexModel(FreshGoDbContext db) => _db = db;
    public List<Event> Events { get; set; } = new(); [BindProperty] public EventInput Input { get; set; } = new();
    public class EventInput { [Required, StringLength(150)] public string Title { get; set; } = ""; public string? Description { get; set; } public string? ImageUrl { get; set; } public DateTime StartAt { get; set; } = DateTime.Now; public DateTime EndAt { get; set; } = DateTime.Now.AddDays(7); }
    public async Task OnGetAsync() => Events = await _db.Events.OrderByDescending(x => x.StartAt).ToListAsync();
    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.EndAt <= Input.StartAt) ModelState.AddModelError("Input.EndAt", "Thời gian kết thúc phải sau thời gian bắt đầu.");
        if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }
        _db.Events.Add(new Event { Title = Input.Title, Description = Input.Description, ImageUrl = Input.ImageUrl, StartAt = Input.StartAt.ToUniversalTime(), EndAt = Input.EndAt.ToUniversalTime() });
        await _db.SaveChangesAsync(); TempData["Success"] = "Đã tạo sự kiện."; return RedirectToPage();
    }
    public async Task<IActionResult> OnPostToggleAsync(int id) { var e = await _db.Events.FindAsync(id); if (e == null) return NotFound(); e.IsActive = !e.IsActive; await _db.SaveChangesAsync(); return RedirectToPage(); }
}
