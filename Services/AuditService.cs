using FreshGo.Data;
using FreshGo.Models;
using System.Security.Claims;

namespace FreshGo.Services;

public class AuditService
{
    private readonly FreshGoDbContext _db;
    private readonly IHttpContextAccessor _http;
    public AuditService(FreshGoDbContext db, IHttpContextAccessor http) { _db = db; _http = http; }
    public async Task WriteAsync(string action, string? details = null)
    {
        _db.AuditLogs.Add(new AuditLog { UserId = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), Action = action, Details = details });
        await _db.SaveChangesAsync();
    }
}
