using FreshGo.Data;
using FreshGo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FreshGo.Pages.Admin.Orders;
public class IndexModel : PageModel
{
    private readonly FreshGoDbContext _db; private readonly UserManager<ApplicationUser> _users;
    public IndexModel(FreshGoDbContext db, UserManager<ApplicationUser> users) { _db = db; _users = users; }
    public record OrderRow(string Code, string? BranchName, string CustomerName, string? CustomerPhone, DateTime CreatedAt, string Status, string PaymentMethod, string PaymentStatus, decimal TotalAmount);
    public List<OrderRow> Orders { get; set; } = new();
    public async Task OnGetAsync()
    {
        var orders = await _db.Orders.Include(x => x.Branch).OrderByDescending(x => x.CreatedAt).Take(300).ToListAsync();
        var ids = orders.Select(x => x.CustomerId).Distinct().ToList();
        var customers = await _users.Users.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
        Orders = orders.Select(o => new OrderRow(o.OrderCode, o.Branch?.Name, customers.GetValueOrDefault(o.CustomerId)?.FullName ?? "Khách hàng", o.CustomerPhone, o.CreatedAt, o.Status, o.PaymentMethod, o.PaymentStatus, o.TotalAmount)).ToList();
    }
}