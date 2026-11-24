using Microsoft.EntityFrameworkCore;

namespace FreshGo.Models
{
    public class FreshGoContext : DbContext
    {
        public FreshGoContext(DbContextOptions<FreshGoContext> options) : base(options) { }

        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<SanPham>(e => { e.ToTable("SanPham"); e.HasKey(x => x.Id); });
            b.Entity<DanhMuc>(e => { e.ToTable("DanhMuc"); e.HasKey(x => x.Id); });
            b.Entity<Voucher>(e => { e.ToTable("Voucher"); e.HasKey(x => x.Id); });
            b.Entity<GioHang>(e =>        
            {
                e.ToTable("GioHang");
                e.HasKey(x => new { x.IdKhachHang, x.IdSanPham });
                e.Property(x => x.IdKhachHang).HasColumnName("ID_KhachHang");
                e.Property(x => x.IdSanPham).HasColumnName("ID_SanPham");
                e.HasOne(x => x.IdSanPhamNavigation).WithMany().HasForeignKey(x => x.IdSanPham);
            });
        }
    }
}