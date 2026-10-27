using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshGo.Models
{
    [Table("DonHang")]
    public class DonHang
    {
        [Key]
        public int MaDonHang { get; set; }
        public int? MaKhachHang { get; set; }
        public int? MaChiNhanh { get; set; }
        public int? MaShipper { get; set; }
        public string DiaChiGiaoHang { get; set; } = string.Empty;
        public string SoDienThoaiNhan { get; set; } = string.Empty;
        public decimal TongTienHang { get; set; }
        public decimal PhiShip { get; set; }
        public decimal TienGiamVoucher { get; set; }
        public decimal TongThanhToan { get; set; }
        public string PhuongThucThanhToan { get; set; } = "COD";
        public string TrangThaiDon { get; set; } = "Chờ xử lý";
    }
}