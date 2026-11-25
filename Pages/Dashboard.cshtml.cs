using FreshGo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshGo.Pages;
[Authorize]
public class DashboardModel : PageModel
{
    // Bản tách riêng chỉ có khu vực Admin: Admin vào /Admin, còn lại bị từ chối.
    public IActionResult OnGet()
    {
        if (User.IsInRole(Roles.Admin)) return RedirectToPage("/Admin/Index");
        return RedirectToPage("/Account/AccessDenied");
    }
}
