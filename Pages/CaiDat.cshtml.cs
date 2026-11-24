using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshGo.Pages
{
    public class CaiDatModel : PageModel
    {
        [BindProperty]
        public string KhuVucMacDinh { get; set; } = "Quận 1";

        [BindProperty]
        public string CheDoGiaoDien { get; set; } = "light";

        [BindProperty]
        public string NgonNgu { get; set; } = "vi";

        [BindProperty]
        public bool ThongBaoDonHang { get; set; } = true;

        [BindProperty]
        public bool ThongBaoKhuyenMai { get; set; } = true;

        [TempData]
        public string? ThongBaoThanhCong { get; set; }

        public void OnGet()
        {
            if (Request.Cookies.TryGetValue("FreshGo_KhuVuc", out var kv) && !string.IsNullOrEmpty(kv))
                KhuVucMacDinh = kv;

            if (Request.Cookies.TryGetValue("FreshGo_Theme", out var theme) && !string.IsNullOrEmpty(theme))
                CheDoGiaoDien = theme;
        }

        public IActionResult OnPost()
        {
            var options = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30),
                HttpOnly = false
            };

            Response.Cookies.Append("FreshGo_KhuVuc", KhuVucMacDinh, options);
            Response.Cookies.Append("FreshGo_Theme", CheDoGiaoDien, options);

            ThongBaoThanhCong = "Đã lưu cài đặt hệ thống FreshGo thành công!";
            return RedirectToPage();
        }
    }
}