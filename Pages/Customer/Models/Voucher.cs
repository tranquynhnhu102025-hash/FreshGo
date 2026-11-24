using System.ComponentModel.DataAnnotations.Schema;

namespace FreshGo.Models
{
    [Table("Voucher")]
    public class Voucher
    {
        [Column("ID")]
        public int Id { get; set; }
        public string MaVoucher { get; set; } = string.Empty;
        public int PhanTramGiam { get; set; }
        public decimal? GiamToiDa { get; set; }
        public int SoLuong { get; set; }
        public DateTime? NgayHetHan { get; set; }
    }
}