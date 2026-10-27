using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FreshGo.Models; 
using FreshGo;

namespace FreshGo.Pages.Shipper
{
    public class IndexModel : PageModel
    {
        private readonly FreshGoContext _context;

        public IndexModel(FreshGoContext context)
        {
            _context = context;
        }

        public IList<DonHang> DonHangList { get; set; } = default!;

        public int DangGiaoCount { get; set; }
        public int DaGiaoCount { get; set; }
        public decimal TotalCod { get; set; }

        public async Task OnGetAsync()
        {
            DonHangList = await _context.DonHangs.ToListAsync();

            // Sửa lại: Xóa bỏ chữ N trước chuỗi
            DangGiaoCount = DonHangList.Count(d => d.TrangThaiDon == "Đang giao");
            DaGiaoCount = DonHangList.Count(d => d.TrangThaiDon == "Đã giao");

            TotalCod = DonHangList
                .Where(d => d.TrangThaiDon == "Đang giao" && d.PhuongThucThanhToan == "COD")
                .Sum(d => d.TongThanhToan);
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang != null)
            {
                donHang.TrangThaiDon = "Đã giao";
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang != null)
            {
                donHang.TrangThaiDon = "Đã hủy";
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}