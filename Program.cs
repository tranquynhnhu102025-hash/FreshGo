using FreshGo.Pages.Customer;
using Microsoft.EntityFrameworkCore;
using FreshGo.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// KHAI BÁO LẠI KẾT NỐI DATABASE BỊ MẤT:
builder.Services.AddDbContext<FreshGoContext>(options =>
    options.UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=FreshGoDB;Trusted_Connection=True;TrustServerCertificate=True"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();