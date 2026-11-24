using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshGo.Models
{
    [Table("DanhMuc")]
    public class DanhMuc
    {
        [Key]
        public int Id { get; set; }
        public string TenDanhMuc { get; set; } = null!;
    }
}