using System.ComponentModel.DataAnnotations.Schema;

namespace FreshGo.Models
{
    public class SanPham
    {
        public int Id { get; set; }
        public string TenSanPham { get; set; } = null!;
        public int? IdDanhMuc { get; set; }
        public decimal GiaBan { get; set; }
        public string? HinhAnh { get; set; }
        public string? MoTa { get; set; }

        // Chỉ định rõ IdDanhMucNavigation liên kết tới cột IdDanhMuc
        [ForeignKey("IdDanhMuc")]
        public virtual DanhMuc? IdDanhMucNavigation { get; set; }
    }
}