using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreshGo.Models;

namespace FreshGo.Pages.Order
{
    public class CreateModel : PageModel
    {
        private readonly FreshGoContext _context;

        public CreateModel(FreshGoContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DonHang DonHang { get; set; } = new();

        public void OnGet()
        {
            DonHang.PhiShip = 15000;
            DonHang.PhuongThucThanhToan = "COD";
            DonHang.TrangThaiDon = "Đang giao";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            DonHang.TrangThaiDon = "Đang giao";
            DonHang.TongThanhToan = DonHang.TongTienHang + DonHang.PhiShip;

            _context.DonHangs.Add(DonHang);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Shipper/Index");
        }
    }
}