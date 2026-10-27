using Microsoft.EntityFrameworkCore;
using FreshGo.Models;

namespace FreshGo
{
    public class FreshGoContext : DbContext
    {
        public FreshGoContext(DbContextOptions<FreshGoContext> options) : base(options)
        {
        }

        public DbSet<DonHang> DonHangs { get; set; }
    }
}