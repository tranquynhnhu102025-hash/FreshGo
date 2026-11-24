using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshGo.Models
{
    [Table("SanPham")]
    public class SanPham
    {
        [Key]
        public int Id { get; set; }
        public string TenSanPham { get; set; } = null!;
        [Column("ID_DanhMuc")]
        public int? IdDanhMuc { get; set; }
        public decimal GiaBan { get; set; }
        public string? HinhAnh { get; set; }
        public string? MoTa { get; set; }

        [ForeignKey("IdDanhMuc")]
        public virtual DanhMuc? IdDanhMucNavigation { get; set; }
    }
}