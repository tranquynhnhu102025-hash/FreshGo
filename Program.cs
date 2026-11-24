using FreshGo.Pages.Customer;
using Microsoft.EntityFrameworkCore;
using FreshGo.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Trỏ đúng sang database của phân hệ Khách hàng
builder.Services.AddDbContext<FreshGoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CustomerConnection")
        ?? @"Server=localhost\SQLEXPRESS;Database=FreshGoCustomerDB;Trusted_Connection=True;TrustServerCertificate=True;"));

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