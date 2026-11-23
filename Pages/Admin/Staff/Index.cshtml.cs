using FreshGo.Data;
using FreshGo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FreshGo.Pages.Admin.Staff;
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly FreshGoDbContext _db;
    public IndexModel(UserManager<ApplicationUser> users, FreshGoDbContext db) { _users = users; _db = db; }
    public record UserRow(string Id, string FullName, string? Email, string Role, string BranchName, bool IsActive);
    public List<UserRow> Users { get; set; } = new();
    public SelectList BranchOptions { get; set; } = default!;
    [BindProperty] public StaffInput Input { get; set; } = new();
    public class StaffInput
    {
        [Required] public string FullName { get; set; } = "";
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required] public string Role { get; set; } = Roles.Manager;
        public int? BranchId { get; set; }
        [Required, StringLength(100, MinimumLength = 8)] public string Password { get; set; } = "";
    }
    public async Task OnGetAsync() => await LoadAsync();
    private async Task LoadAsync()
    {
        var branches = await _db.Branches.OrderBy(x => x.Name).ToListAsync();
        BranchOptions = new SelectList(branches, "Id", "Name");
        var rows = new List<UserRow>();
        foreach (var u in await _users.Users.ToListAsync())
        {
            var roles = await _users.GetRolesAsync(u);
            if (roles.Contains(Roles.Admin)) continue;
            var branch = u.BranchId.HasValue ? branches.FirstOrDefault(x => x.Id == u.BranchId.Value)?.Name ?? "-" : "-";
            rows.Add(new UserRow(u.Id, u.FullName, u.Email, roles.FirstOrDefault() ?? "Chưa phân quyền", branch, u.IsActive));
        }
        Users = rows;
    }
    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (Input.Role is not (Roles.Manager or Roles.Shipper)) ModelState.AddModelError("Input.Role", "Vai trò không hợp lệ.");
        if ((Input.Role == Roles.Manager || Input.Role == Roles.Shipper) && !Input.BranchId.HasValue) ModelState.AddModelError("Input.BranchId", "Vui lòng chọn chi nhánh.");
        if (!ModelState.IsValid) { await LoadAsync(); return Page(); }
        var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email, FullName = Input.FullName, BranchId = Input.BranchId, IsActive = true, EmailConfirmed = true };
        var result = await _users.CreateAsync(user, Input.Password);
        if (!result.Succeeded) { foreach (var e in result.Errors) ModelState.AddModelError("", e.Description); await LoadAsync(); return Page(); }
        await _users.AddToRoleAsync(user, Input.Role); TempData["Success"] = "Đã tạo tài khoản nhân viên."; return RedirectToPage();
    }
    public async Task<IActionResult> OnPostToggleAsync(string id)
    {
        var user = await _users.FindByIdAsync(id); if (user == null) return NotFound();
        user.IsActive = !user.IsActive; await _users.UpdateAsync(user); TempData["Success"] = "Đã cập nhật trạng thái tài khoản."; return RedirectToPage();
    }
    public async Task<IActionResult> OnPostResetPasswordAsync(string id, string newPassword)
    {
        var user = await _users.FindByIdAsync(id); if (user == null) return NotFound();
        var token = await _users.GeneratePasswordResetTokenAsync(user);
        var result = await _users.ResetPasswordAsync(user, token, newPassword);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Đã đặt lại mật khẩu." : string.Join("; ", result.Errors.Select(x => x.Description));
        return RedirectToPage();
    }
}
