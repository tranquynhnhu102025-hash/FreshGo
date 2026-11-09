using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FreshGo.Models; // Đổi thành FreshGo.Data nếu báo lỗi đỏ

namespace FreshGo.Pages.Customer.GioHang
{
    public class CartModel : PageModel
    {
        private readonly FreshGoContext _context;

        public CartModel(FreshGoContext context)
        {
            _context = context;
        }

        // Tránh lỗi trùng tên class bằng cách gọi đích danh FreshGo.Models.GioHang
        public IList<FreshGo.Models.GioHang> Items { get; set; } = new List<FreshGo.Models.GioHang>();

        public decimal Total => Items.Sum(i => (i.IdSanPhamNavigation?.GiaBan ?? 0) * (i.SoLuong ?? 0));

        public async Task OnGetAsync()
        {
            int customerId = 4; // Tạm fix cứng khach1
            
            if (_context.GioHangs != null)
            {
                Items = await _context.GioHangs
                    .Include(g => g.IdSanPhamNavigation)
                    .Where(g => g.IdKhachHang == customerId)
                    .ToListAsync();
            }
        }

        // HÀM XÓA SẢN PHẨM KHỎI GIỎ HÀNG
        public async Task<IActionResult> OnPostDeleteAsync(int sanPhamId)
        {
            int customerId = 4; 

            var cartItem = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.IdKhachHang == customerId && g.IdSanPham == sanPhamId);

            if (cartItem != null)
            {
                // Lấy tên sản phẩm để hiện thông báo cho đẹp
                var tenSp = await _context.SanPhams
                    .Where(s => s.Id == sanPhamId)
                    .Select(s => s.TenSanPham)
                    .FirstOrDefaultAsync();

                _context.GioHangs.Remove(cartItem);
                await _context.SaveChangesAsync();

                // Tạo thông báo Xóa thành công
                TempData["ThongBao"] = $"Đã xóa {tenSp} khỏi giỏ hàng!";
            }

            return RedirectToPage();
        }
    }
}