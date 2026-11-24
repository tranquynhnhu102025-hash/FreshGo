using FreshGo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FreshGo.Pages.Customer.BaiViet
{
    public class ChiTietModel : PageModel
    {
        private readonly FreshGoContext _context;

        public ChiTietModel(FreshGoContext context)
        {
            _context = context;
        }

        public BaiVietItem? BaiViet { get; set; }
        public List<BaiVietItem> BaiLienQuan { get; set; } = new();
        public List<SanPham> NguyenLieuGoiY { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id = 1)
        {
            var tatCa = IndexModel.LayDanhSachBaiViet();
            BaiViet = tatCa.FirstOrDefault(b => b.Id == id) ?? tatCa.First();
            BaiLienQuan = tatCa.Where(b => b.Id != BaiViet.Id).Take(3).ToList();

            NguyenLieuGoiY = await _context.SanPhams
                .Take(4)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostThemGioAsync(int sanPhamId)
        {
            int khachHangId = 4;
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
    }
}