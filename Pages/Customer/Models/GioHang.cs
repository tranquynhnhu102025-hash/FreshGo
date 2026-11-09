namespace FreshGo.Models
{
    public class GioHang
    {
        public int IdKhachHang { get; set; }
        public int IdSanPham { get; set; }
        public int? SoLuong { get; set; }
        public SanPham IdSanPhamNavigation { get; set; } = null!;
    }
}