using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FreshGo.Models; 

namespace FreshGo.Pages.Customer.TrangChu
{
    public class IndexModel : PageModel
    {
        private readonly FreshGoContext _context;

        public IndexModel(FreshGoContext context)
        {
            _context = context;
        }

        public IList<SanPham> SanPhams { get; set; } = new List<SanPham>();
        public IList<SanPham> SanPhamNoiBats { get; set; } = new List<SanPham>();
        public IList<DanhMuc> DanhMucs { get; set; } = new List<DanhMuc>();

        [BindProperty(SupportsGet = true)]
        public string? SearchKeyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        public async Task OnGetAsync()
        {
            if (_context.DanhMucs != null)
            {
                DanhMucs = await _context.DanhMucs.ToListAsync();
            }

            var query = _context.SanPhams.AsQueryable();

            if (!string.IsNullOrEmpty(SearchKeyword))
            {
                query = query.Where(s => s.TenSanPham.Contains(SearchKeyword));
            }

            if (CategoryId.HasValue)
            {
                query = query.Where(s => s.IdDanhMuc == CategoryId.Value);
            }

            SanPhams = await query.ToListAsync();
            SanPhamNoiBats = await _context.SanPhams.Take(4).ToListAsync();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int sanPhamId, int soLuong = 1)
        {
            int customerId = 4; // Tạm fix cứng khach1

            var cartItem = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.IdKhachHang == customerId && g.IdSanPham == sanPhamId);

            if (cartItem == null)
            {
                cartItem = new FreshGo.Models.GioHang
                {
                    IdKhachHang = customerId,
                    IdSanPham = sanPhamId,
                    SoLuong = soLuong
                };
                _context.GioHangs.Add(cartItem);
            }
            else
            {
                cartItem.SoLuong += soLuong;
            }

            await _context.SaveChangesAsync();

            // Ghi thông báo và KHÔNG chuyển trang
            TempData["ThongBao"] = $"Đã thêm {soLuong} Kg vào giỏ hàng thành công!";
            return RedirectToPage();
        }
    }
}