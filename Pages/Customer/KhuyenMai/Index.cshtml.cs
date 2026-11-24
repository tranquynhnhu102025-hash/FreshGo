using FreshGo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FreshGo.Pages.Customer.KhuyenMai
{
    public class IndexModel : PageModel
    {
        private readonly FreshGoContext _context;

        public IndexModel(FreshGoContext context)
        {
            _context = context;
        }

        public List<Voucher> DanhSachVoucher { get; set; } = new();
        public List<SanPham> SanPhamGiamGia { get; set; } = new();

        public async Task OnGetAsync()
        {
            DanhSachVoucher = await _context.Vouchers
                .Where(v => v.SoLuong > 0 && (v.NgayHetHan == null || v.NgayHetHan >= DateTime.Now))
                .ToListAsync();

            SanPhamGiamGia = await _context.SanPhams
                .Take(8)
                .ToListAsync();
        }

        // Bấm "Mua ngay" -> Lưu vào bảng GioHang và nhảy thẳng qua trang Giỏ hàng
        public async Task<IActionResult> OnPostMuaNgayAsync(int sanPhamId)
        {
            int khachHangId = 4; // ID mặc định của khach1 trong FreshGoCustomerDB

            var item = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.IdKhachHang == khachHangId && g.IdSanPham == sanPhamId);

            if (item != null)
            {
                item.SoLuong += 1;
            }
            else
            {
                _context.GioHangs.Add(new FreshGo.Models.GioHang
                {
                    IdKhachHang = khachHangId,
                    IdSanPham = sanPhamId,
                    SoLuong = 1
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("/Customer/GioHang/Cart");
        }

        // Bấm "Dùng ngay" -> Chuyển sang Giỏ hàng kèm theo mã Voucher
        public IActionResult OnPostDungMaAsync(string maVoucher)
        {
            return RedirectToPage("/Customer/GioHang/Cart", new { voucher = maVoucher });
        }
    }
}