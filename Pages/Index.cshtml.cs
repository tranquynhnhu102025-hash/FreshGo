using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshGo.Pages;
public class IndexModel : PageModel
{
    // Bản tách riêng chỉ có khu vực Admin: trang chủ chuyển thẳng vào Dashboard (chưa đăng nhập sẽ sang trang đăng nhập).
    public IActionResult OnGet() => RedirectToPage("/Dashboard");
}
