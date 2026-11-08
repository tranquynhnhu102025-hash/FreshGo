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

        // Các biến thống kê
        public int DangGiaoCount { get; set; }
        public int DaGiaoCount { get; set; }
        public decimal TotalCod { get; set; }             // Tổng tiền mặt COD cần thu hộ
        public decimal TotalShippingEarnings { get; set; } // Tổng tiền phí ship shipper kiếm được

        public async Task OnGetAsync()
        {
            DonHangList = await _context.DonHangs.ToListAsync();

            // 1. Thống kê số lượng đơn
            DangGiaoCount = DonHangList.Count(d => d.TrangThaiDon == "Đang giao" || d.TrangThaiDon == "Chờ xử lý");
            DaGiaoCount = DonHangList.Count(d => d.TrangThaiDon == "Đã giao");

            // 2. Thống kê tiền thu hộ COD (Chỉ tính các đơn đã giao thành công và chọn thanh toán COD)
            TotalCod = DonHangList
                .Where(d => d.TrangThaiDon == "Đã giao" && d.PhuongThucThanhToan == "COD")
                .Sum(d => d.TongThanhToan);

            // 3. Thống kê tổng tiền phí ship thực nhận từ các đơn đã giao thành công
            TotalShippingEarnings = DonHangList
                .Where(d => d.TrangThaiDon == "Đã giao")
                .Sum(d => d.PhiShip);
        }
        // Handler hoàn thành đơn hàng
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

        // Handler hủy đơn hàng
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