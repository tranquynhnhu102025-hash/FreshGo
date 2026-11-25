using FreshGo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FreshGo.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<FreshGoDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var role in new[] { Roles.Admin, Roles.Manager, Roles.Shipper, Roles.Customer })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        if (!await db.Branches.AnyAsync())
        {
            db.Branches.AddRange(
                new Branch { Name = "FreshGo Quận 12", Address = "Quận 12, TP. Hồ Chí Minh", Phone = "0280000001" },
                new Branch { Name = "FreshGo Gò Vấp", Address = "Gò Vấp, TP. Hồ Chí Minh", Phone = "0280000002" });
            await db.SaveChangesAsync();
        }

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(new Category { Name = "Rau củ" }, new Category { Name = "Trái cây" }, new Category { Name = "Thịt cá" }, new Category { Name = "Đồ khô" });
            await db.SaveChangesAsync();
        }

        if (!await db.Warehouses.AnyAsync())
        {
            db.Warehouses.Add(new Warehouse { Name = "Kho tổng FreshGo", IsMain = true });
            foreach (var branch in await db.Branches.ToListAsync())
                db.Warehouses.Add(new Warehouse { Name = "Kho " + branch.Name, BranchId = branch.Id, IsMain = false });
            await db.SaveChangesAsync();
        }

        if (!await db.Products.AnyAsync())
        {
            var cats = await db.Categories.OrderBy(x => x.Id).ToListAsync();
            db.Products.AddRange(
                new Product { Name = "Cải ngọt tươi", CategoryId = cats[0].Id, Unit = "kg", Price = 25000, CostPrice = 16000, Description = "Rau xanh tươi mỗi ngày" },
                new Product { Name = "Cà rốt Đà Lạt", CategoryId = cats[0].Id, Unit = "kg", Price = 22000, CostPrice = 14000 },
                new Product { Name = "Táo đỏ", CategoryId = cats[1].Id, Unit = "kg", Price = 65000, CostPrice = 45000 },
                new Product { Name = "Chuối già", CategoryId = cats[1].Id, Unit = "kg", Price = 30000, CostPrice = 19000 },
                new Product { Name = "Ức gà", CategoryId = cats[2].Id, Unit = "kg", Price = 95000, CostPrice = 72000 },
                new Product { Name = "Gạo thơm", CategoryId = cats[3].Id, Unit = "túi", Price = 45000, CostPrice = 35000 });
            await db.SaveChangesAsync();
        }

        if (!await db.Inventories.AnyAsync())
        {
            var main = await db.Warehouses.FirstAsync(x => x.IsMain);
            var products = await db.Products.ToListAsync();
            foreach (var product in products)
                db.Inventories.Add(new Inventory { WarehouseId = main.Id, ProductId = product.Id, Quantity = 500, MinQuantity = 30 });
            foreach (var warehouse in await db.Warehouses.Where(x => !x.IsMain).ToListAsync())
                foreach (var product in products)
                    db.Inventories.Add(new Inventory { WarehouseId = warehouse.Id, ProductId = product.Id, Quantity = 80, MinQuantity = 10 });
            await db.SaveChangesAsync();
        }

        if (!await db.Gifts.AnyAsync())
        {
            db.Gifts.AddRange(
                new Gift { Name = "Túi vải FreshGo", Description = "Túi mua sắm thân thiện môi trường", PointsRequired = 50, Quantity = 100 },
                new Gift { Name = "Bình nước FreshGo", Description = "Bình nước dùng nhiều lần", PointsRequired = 120, Quantity = 40 },
                new Gift { Name = "Voucher 30.000đ", Description = "Ưu đãi cho đơn hàng tiếp theo", PointsRequired = 200, Quantity = 30 });
            await db.SaveChangesAsync();
        }

        var firstBranch = await db.Branches.OrderBy(x => x.Id).FirstAsync();
        await EnsureUser(userManager, "admin@freshgo.local", "Admin FreshGo", "Admin@123", Roles.Admin, null);
        await EnsureUser(userManager, "manager@freshgo.local", "Quản lý chi nhánh", "Manager@123", Roles.Manager, firstBranch.Id);
        await EnsureUser(userManager, "shipper@freshgo.local", "Shipper FreshGo", "Shipper@123", Roles.Shipper, firstBranch.Id);
        await EnsureUser(userManager, "customer@freshgo.local", "Khách hàng mẫu", "Customer@123", Roles.Customer, null);
    }

    private static async Task EnsureUser(UserManager<ApplicationUser> manager, string email, string name, string password, string role, int? branchId)
    {
        var user = await manager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = name, BranchId = branchId, IsActive = true };
            var result = await manager.CreateAsync(user, password);
            if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
        if (!await manager.IsInRoleAsync(user, role)) await manager.AddToRoleAsync(user, role);
    }
}
