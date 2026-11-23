/*
  FreshGo database bootstrap for SQL Server.
  1) Run this script in SQL Server Management Studio (SSMS).
  2) Then open FreshGo in Visual Studio and run it once.
  DbInitializer seeds demo accounts and sample records after schema creation.
*/
IF DB_ID(N'FreshGoDb') IS NULL
    CREATE DATABASE FreshGoDb;
GO
USE FreshGoDb;
GO

IF OBJECT_ID(N'dbo.Branches', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Branches (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Branches PRIMARY KEY,
        Name nvarchar(150) NOT NULL,
        Address nvarchar(300) NOT NULL,
        Phone nvarchar(30) NULL,
        MapUrl nvarchar(250) NULL,
        IsActive bit NOT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetRoles (
        Id nvarchar(450) NOT NULL CONSTRAINT PK_AspNetRoles PRIMARY KEY,
        Name nvarchar(256) NULL,
        NormalizedName nvarchar(256) NULL,
        ConcurrencyStamp nvarchar(max) NULL
    );
    CREATE UNIQUE INDEX RoleNameIndex ON dbo.AspNetRoles(NormalizedName) WHERE NormalizedName IS NOT NULL;
END;
GO

IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUsers (
        Id nvarchar(450) NOT NULL CONSTRAINT PK_AspNetUsers PRIMARY KEY,
        UserName nvarchar(256) NULL,
        NormalizedUserName nvarchar(256) NULL,
        Email nvarchar(256) NULL,
        NormalizedEmail nvarchar(256) NULL,
        EmailConfirmed bit NOT NULL,
        PasswordHash nvarchar(max) NULL,
        SecurityStamp nvarchar(max) NULL,
        ConcurrencyStamp nvarchar(max) NULL,
        PhoneNumber nvarchar(max) NULL,
        PhoneNumberConfirmed bit NOT NULL,
        TwoFactorEnabled bit NOT NULL,
        LockoutEnd datetimeoffset NULL,
        LockoutEnabled bit NOT NULL,
        AccessFailedCount int NOT NULL,
        FullName nvarchar(120) NOT NULL,
        BranchId int NULL,
        IsActive bit NOT NULL,
        Points int NOT NULL,
        CONSTRAINT FK_AspNetUsers_Branches_BranchId FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id) ON DELETE SET NULL
    );
    CREATE INDEX EmailIndex ON dbo.AspNetUsers(NormalizedEmail);
    CREATE UNIQUE INDEX UserNameIndex ON dbo.AspNetUsers(NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
END;
GO

IF OBJECT_ID(N'dbo.AspNetRoleClaims', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetRoleClaims (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY,
        RoleId nvarchar(450) NOT NULL,
        ClaimType nvarchar(max) NULL,
        ClaimValue nvarchar(max) NULL,
        CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_AspNetRoleClaims_RoleId ON dbo.AspNetRoleClaims(RoleId);
END;
GO

IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserClaims (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AspNetUserClaims PRIMARY KEY,
        UserId nvarchar(450) NOT NULL,
        ClaimType nvarchar(max) NULL,
        ClaimValue nvarchar(max) NULL,
        CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_AspNetUserClaims_UserId ON dbo.AspNetUserClaims(UserId);
END;
GO

IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserLogins (
        LoginProvider nvarchar(128) NOT NULL,
        ProviderKey nvarchar(128) NOT NULL,
        ProviderDisplayName nvarchar(max) NULL,
        UserId nvarchar(450) NOT NULL,
        CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
        CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_AspNetUserLogins_UserId ON dbo.AspNetUserLogins(UserId);
END;
GO

IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserRoles (
        UserId nvarchar(450) NOT NULL,
        RoleId nvarchar(450) NOT NULL,
        CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_AspNetUserRoles_RoleId ON dbo.AspNetUserRoles(RoleId);
END;
GO

IF OBJECT_ID(N'dbo.AspNetUserTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserTokens (
        UserId nvarchar(450) NOT NULL,
        LoginProvider nvarchar(128) NOT NULL,
        Name nvarchar(128) NOT NULL,
        Value nvarchar(max) NULL,
        CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
        CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
CREATE TABLE dbo.Categories (Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY, Name nvarchar(100) NOT NULL);
GO
IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
        Name nvarchar(180) NOT NULL, Description nvarchar(1000) NULL, ImageUrl nvarchar(500) NULL,
        Unit nvarchar(30) NOT NULL, Price decimal(18,2) NOT NULL, CostPrice decimal(18,2) NOT NULL,
        CategoryId int NOT NULL, IsActive bit NOT NULL, CreatedAt datetime2 NOT NULL,
        CONSTRAINT FK_Products_Categories_CategoryId FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)
    );
    CREATE INDEX IX_Products_CategoryId ON dbo.Products(CategoryId);
    CREATE INDEX IX_Products_Name ON dbo.Products(Name);
END;
GO
IF OBJECT_ID(N'dbo.Warehouses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Warehouses (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Warehouses PRIMARY KEY,
        Name nvarchar(150) NOT NULL, IsMain bit NOT NULL, BranchId int NULL,
        CONSTRAINT FK_Warehouses_Branches_BranchId FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id) ON DELETE SET NULL
    );
    CREATE INDEX IX_Warehouses_BranchId ON dbo.Warehouses(BranchId);
END;
GO
IF OBJECT_ID(N'dbo.Inventories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Inventories (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventories PRIMARY KEY,
        WarehouseId int NOT NULL, ProductId int NOT NULL, Quantity int NOT NULL, MinQuantity int NOT NULL,
        CONSTRAINT FK_Inventories_Warehouses_WarehouseId FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Inventories_Products_ProductId FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
    );
    CREATE UNIQUE INDEX IX_Inventories_WarehouseId_ProductId ON dbo.Inventories(WarehouseId, ProductId);
    CREATE INDEX IX_Inventories_ProductId ON dbo.Inventories(ProductId);
END;
GO
IF OBJECT_ID(N'dbo.StockRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockRequests (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockRequests PRIMARY KEY,
        BranchId int NOT NULL, ProductId int NOT NULL, Quantity int NOT NULL, Note nvarchar(500) NULL,
        Status nvarchar(30) NOT NULL, RequestedById nvarchar(max) NOT NULL, ApprovedById nvarchar(max) NULL,
        CreatedAt datetime2 NOT NULL, ApprovedAt datetime2 NULL,
        CONSTRAINT FK_StockRequests_Branches_BranchId FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
        CONSTRAINT FK_StockRequests_Products_ProductId FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
    );
    CREATE INDEX IX_StockRequests_BranchId ON dbo.StockRequests(BranchId);
    CREATE INDEX IX_StockRequests_ProductId ON dbo.StockRequests(ProductId);
END;
GO
IF OBJECT_ID(N'dbo.StockTransfers', N'U') IS NULL
CREATE TABLE dbo.StockTransfers (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockTransfers PRIMARY KEY,
    StockRequestId int NOT NULL, SourceWarehouseId int NOT NULL, TargetWarehouseId int NOT NULL,
    ProductId int NOT NULL, Quantity int NOT NULL, Status nvarchar(30) NOT NULL, CreatedAt datetime2 NOT NULL, ReceivedAt datetime2 NULL
);
GO
IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
        OrderCode nvarchar(30) NOT NULL, CustomerId nvarchar(max) NOT NULL, BranchId int NOT NULL,
        CreatedAt datetime2 NOT NULL, Status nvarchar(30) NOT NULL, PaymentMethod nvarchar(30) NOT NULL,
        PaymentStatus nvarchar(30) NOT NULL, DeliveryAddress nvarchar(500) NOT NULL, CustomerPhone nvarchar(30) NULL,
        VoucherCode nvarchar(40) NULL, Subtotal decimal(18,2) NOT NULL, DiscountAmount decimal(18,2) NOT NULL,
        TotalAmount decimal(18,2) NOT NULL, ShipperId int NULL, CustomerConfirmedAt datetime2 NULL,
        CONSTRAINT FK_Orders_Branches_BranchId FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id)
    );
    CREATE UNIQUE INDEX IX_Orders_OrderCode ON dbo.Orders(OrderCode);
    CREATE INDEX IX_Orders_BranchId ON dbo.Orders(BranchId);
END;
GO
IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderItems PRIMARY KEY,
        OrderId int NOT NULL, ProductId int NOT NULL, Quantity int NOT NULL, UnitPrice decimal(18,2) NOT NULL,
        CONSTRAINT FK_OrderItems_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
    );
    CREATE INDEX IX_OrderItems_ProductId ON dbo.OrderItems(ProductId);
    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
END;
GO
IF OBJECT_ID(N'dbo.Shipments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Shipments (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Shipments PRIMARY KEY,
        OrderId int NOT NULL, ShipperId nvarchar(max) NOT NULL, Status nvarchar(30) NOT NULL,
        AssignedAt datetime2 NOT NULL, DeliveredAt datetime2 NULL, FailureReason nvarchar(500) NULL,
        CONSTRAINT FK_Shipments_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_Shipments_OrderId ON dbo.Shipments(OrderId);
END;
GO
IF OBJECT_ID(N'dbo.Attendances', N'U') IS NULL
CREATE TABLE dbo.Attendances (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Attendances PRIMARY KEY,
    EmployeeId nvarchar(max) NOT NULL, BranchId int NOT NULL, CheckIn datetime2 NOT NULL, CheckOut datetime2 NULL, Note nvarchar(500) NULL
);
GO
IF OBJECT_ID(N'dbo.DefectiveProducts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DefectiveProducts (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_DefectiveProducts PRIMARY KEY,
        BranchId int NOT NULL, ProductId int NOT NULL, Quantity int NOT NULL, Reason nvarchar(500) NOT NULL,
        ImageUrl nvarchar(500) NULL, ReportedById nvarchar(max) NOT NULL, ReviewedById nvarchar(max) NULL,
        Status nvarchar(30) NOT NULL, CreatedAt datetime2 NOT NULL,
        CONSTRAINT FK_DefectiveProducts_Branches_BranchId FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
        CONSTRAINT FK_DefectiveProducts_Products_ProductId FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
    );
    CREATE INDEX IX_DefectiveProducts_BranchId ON dbo.DefectiveProducts(BranchId);
    CREATE INDEX IX_DefectiveProducts_ProductId ON dbo.DefectiveProducts(ProductId);
END;
GO
IF OBJECT_ID(N'dbo.Vouchers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vouchers (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Vouchers PRIMARY KEY,
        Code nvarchar(40) NOT NULL, Name nvarchar(150) NOT NULL, DiscountPercent decimal(5,2) NOT NULL,
        MaxDiscount decimal(18,2) NOT NULL, MinOrderValue decimal(18,2) NOT NULL, Quantity int NOT NULL,
        StartAt datetime2 NOT NULL, EndAt datetime2 NOT NULL, IsActive bit NOT NULL
    );
    CREATE UNIQUE INDEX IX_Vouchers_Code ON dbo.Vouchers(Code);
END;
GO
IF OBJECT_ID(N'dbo.Events', N'U') IS NULL
CREATE TABLE dbo.Events (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Events PRIMARY KEY,
    Title nvarchar(150) NOT NULL, Description nvarchar(1000) NULL, ImageUrl nvarchar(500) NULL,
    StartAt datetime2 NOT NULL, EndAt datetime2 NOT NULL, IsActive bit NOT NULL
);
GO
IF OBJECT_ID(N'dbo.LoyaltyTransactions', N'U') IS NULL
CREATE TABLE dbo.LoyaltyTransactions (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoyaltyTransactions PRIMARY KEY,
    UserId nvarchar(max) NOT NULL, Points int NOT NULL, Description nvarchar(250) NOT NULL, CreatedAt datetime2 NOT NULL
);
GO
IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
CREATE TABLE dbo.AuditLogs (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
    UserId nvarchar(max) NULL, Action nvarchar(100) NOT NULL, Details nvarchar(1000) NULL, CreatedAt datetime2 NOT NULL
);
GO
IF OBJECT_ID(N'dbo.Gifts', N'U') IS NULL
CREATE TABLE dbo.Gifts (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Gifts PRIMARY KEY,
    Name nvarchar(150) NOT NULL, Description nvarchar(500) NULL, PointsRequired int NOT NULL, Quantity int NOT NULL, IsActive bit NOT NULL
);
GO
IF OBJECT_ID(N'dbo.GiftRedemptions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GiftRedemptions (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_GiftRedemptions PRIMARY KEY,
        UserId nvarchar(max) NOT NULL, GiftId int NOT NULL, PointsSpent int NOT NULL, Status nvarchar(40) NOT NULL, CreatedAt datetime2 NOT NULL,
        CONSTRAINT FK_GiftRedemptions_Gifts_GiftId FOREIGN KEY (GiftId) REFERENCES dbo.Gifts(Id)
    );
    CREATE INDEX IX_GiftRedemptions_GiftId ON dbo.GiftRedemptions(GiftId);
END;
GO
IF OBJECT_ID(N'dbo.CustomerComplaints', N'U') IS NULL
CREATE TABLE dbo.CustomerComplaints (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerComplaints PRIMARY KEY,
    UserId nvarchar(max) NOT NULL, OrderId int NULL, BranchId int NULL, Subject nvarchar(180) NOT NULL,
    Description nvarchar(2000) NOT NULL, ImageUrl nvarchar(500) NULL, Status nvarchar(40) NOT NULL,
    Reply nvarchar(1000) NULL, ReviewedById nvarchar(max) NULL, CreatedAt datetime2 NOT NULL
);
GO

PRINT N'FreshGoDb schema ready. Start the FreshGo web app once to seed roles, demo users, branches, categories and products.';
GO
