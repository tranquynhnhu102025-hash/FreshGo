USE master;
GO

-- Xóa database cũ nếu đang tồn tại để làm mới hoàn toàn
IF DB_ID('FreshGoDB') IS NOT NULL
BEGIN
    ALTER DATABASE FreshGoDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FreshGoDB;
END
GO

CREATE DATABASE FreshGoDB;
GO
USE FreshGoDB;
GO

-- ==========================================
-- PHẦN 1: TẠO CẤU TRÚC CÁC BẢNG (TABLES)
-- ==========================================

CREATE TABLE TaiKhoan (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    SoDienThoai VARCHAR(15),
    Role VARCHAR(20) CHECK (Role IN ('Admin', 'Manager', 'Shipper', 'Customer')),
    TrangThaiHoatDong BIT DEFAULT 1
);

CREATE TABLE KhuVuc (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TenKhuVuc NVARCHAR(100) NOT NULL,
    PhiGiaoHang DECIMAL(18,2) DEFAULT 0
);

CREATE TABLE ChiNhanh (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TenChiNhanh NVARCHAR(150) NOT NULL,
    DiaChi NVARCHAR(255),
    ID_KhuVuc INT FOREIGN KEY REFERENCES KhuVuc(ID),
    ID_QuanLy INT FOREIGN KEY REFERENCES TaiKhoan(ID)
);

CREATE TABLE DanhMuc (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TenDanhMuc NVARCHAR(100) NOT NULL
);

CREATE TABLE SanPham (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TenSanPham NVARCHAR(200) NOT NULL,
    ID_DanhMuc INT FOREIGN KEY REFERENCES DanhMuc(ID),
    GiaBan DECIMAL(18,2) NOT NULL,
    HinhAnh VARCHAR(MAX),
    MoTa NVARCHAR(MAX)
);

CREATE TABLE KhoChiNhanh (
    ID_ChiNhanh INT FOREIGN KEY REFERENCES ChiNhanh(ID),
    ID_SanPham INT FOREIGN KEY REFERENCES SanPham(ID),
    SoLuong INT DEFAULT 0,
    PRIMARY KEY (ID_ChiNhanh, ID_SanPham)
);

CREATE TABLE Voucher (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    MaVoucher VARCHAR(50) UNIQUE NOT NULL,
    PhanTramGiam INT CHECK (PhanTramGiam > 0 AND PhanTramGiam <= 100),
    GiamToiDa DECIMAL(18,2),
    SoLuong INT DEFAULT 0,
    NgayHetHan DATETIME
);

CREATE TABLE GioHang (
    ID_KhachHang INT FOREIGN KEY REFERENCES TaiKhoan(ID),
    ID_SanPham INT FOREIGN KEY REFERENCES SanPham(ID),
    SoLuong INT DEFAULT 1,
    PRIMARY KEY (ID_KhachHang, ID_SanPham)
);

CREATE TABLE DonHang (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    ID_KhachHang INT FOREIGN KEY REFERENCES TaiKhoan(ID),
    ID_ChiNhanh INT FOREIGN KEY REFERENCES ChiNhanh(ID),
    ID_Shipper INT FOREIGN KEY REFERENCES TaiKhoan(ID),
    NgayDat DATETIME DEFAULT GETDATE(),
    TrangThai NVARCHAR(50) DEFAULT N'Chờ duyệt',
    PhuongThucThanhToan VARCHAR(50),
    DiaChiGiao NVARCHAR(255) NOT NULL,
    TongTienHang DECIMAL(18,2),
    PhiShip DECIMAL(18,2),
    TienGiam DECIMAL(18,2) DEFAULT 0,
    ThanhTien DECIMAL(18,2),
    HinhAnhXacNhan VARCHAR(255),
    LyDoThatBai NVARCHAR(255)
);

CREATE TABLE ChiTietDonHang (
    ID_DonHang INT FOREIGN KEY REFERENCES DonHang(ID),
    ID_SanPham INT FOREIGN KEY REFERENCES SanPham(ID),
    SoLuong INT NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL,
    PRIMARY KEY (ID_DonHang, ID_SanPham)
);
GO

-- ==========================================
-- PHẦN 2: BƠM DỮ LIỆU MẪU (DUMMY DATA CHUẨN)
-- ==========================================

-- 1. Thêm Tài Khoản
INSERT INTO TaiKhoan (Username, PasswordHash, HoTen, SoDienThoai, Role) VALUES
('admin',    '123456', N'Quản trị viên Hệ thống', '0900000001', 'Admin'),
('manager1', '123456', N'Quản lý Chi nhánh Q1',  '0900000002', 'Manager'),
('shipper1', '123456', N'Shipper Nguyễn Văn A',   '0900000003', 'Shipper'),
('khach1',   '123456', N'Khách Trần B',           '0900000004', 'Customer'),
('khach2',   '123456', N'Khách Lê C',             '0900000005', 'Customer');

-- 2. Thêm Khu Vực & Chi Nhánh
INSERT INTO KhuVuc (TenKhuVuc, PhiGiaoHang) VALUES
(N'Quận 1', 15000), 
(N'Quận 7', 20000);

INSERT INTO ChiNhanh (TenChiNhanh, DiaChi, ID_KhuVuc, ID_QuanLy) VALUES
(N'FreshGo Quận 1', N'12 Lê Lợi, Bến Nghé, Quận 1, TP.HCM', 1, 2);

-- 3. Thêm Danh Mục
INSERT INTO DanhMuc (TenDanhMuc) VALUES
(N'Rau củ hữu cơ'), 
(N'Trái cây tươi'), 
(N'Thịt heo & Bò'),
(N'Thủy hải sản'),
(N'Thực phẩm khô');

-- 4. Thêm 15 Sản Phẩm (Đã cập nhật lại toàn bộ Link Ảnh chính xác)
INSERT INTO SanPham (TenSanPham, ID_DanhMuc, GiaBan, HinhAnh, MoTa) VALUES
-- Rau củ (ID_DanhMuc = 1)
(N'Cà chua VietGAP', 1, 25000, 'https://images.unsplash.com/photo-1592924357228-91a4daadcfea?w=600', N'Cà chua tươi Đà Lạt, chua ngọt tự nhiên'),
(N'Rau muống thủy canh', 1, 15000, 'https://images.unsplash.com/photo-1576045057995-568f588f82fb?w=600', N'Rau muống giòn, xanh mướt, không thuốc trừ sâu'),
(N'Súp lơ xanh (Bông cải)', 1, 35000, 'https://images.unsplash.com/photo-1459411621453-7b03977f4bfc?w=600', N'Súp lơ xanh giàu vitamin'),
(N'Cà rốt Đà Lạt', 1, 20000, 'https://images.unsplash.com/photo-1598170845058-32b9d6a5da37?w=600', N'Cà rốt tươi ngọt nguyên củ'),

-- Trái cây (ID_DanhMuc = 2)
(N'Táo Fuji Nhập Khẩu', 2, 85000, 'https://images.unsplash.com/photo-1568702846914-96b305d2aaeb?w=600', N'Táo đỏ giòn ngọt, xuất xứ New Zealand'),
(N'Cam sành Miền Tây', 2, 45000, 'https://images.unsplash.com/photo-1611080626919-7cf5a9dbab5b?w=600', N'Cam sành mọng nước, nhiều vitamin C'),
(N'Dưa hấu không hạt', 2, 22000, 'https://images.unsplash.com/photo-1589984662646-e7b2e4962f18?w=600', N'Dưa hấu ruột đỏ ngọt lịm'),
(N'Nho xanh không hạt', 2, 120000, 'https://images.unsplash.com/photo-1596363505729-4190a9506133?w=600', N'Nho xanh Úc giòn tan'),

-- Thịt (ID_DanhMuc = 3)
(N'Thịt ba chỉ heo', 3, 140000, 'https://images.unsplash.com/photo-1544025162-d76694265947?w=600', N'Thịt heo tươi giết mổ trong ngày'),
(N'Thịt bò thăn Úc', 3, 320000, 'https://images.unsplash.com/photo-1551028150-64b9f398f678?w=600', N'Bò thăn nạc, mềm, thích hợp làm bít tết'),
(N'Ức gà ta', 3, 85000, 'https://images.unsplash.com/photo-1604908176997-125f25cc6f3d?w=600', N'Ức gà ta dai ngon, phù hợp ăn kiêng'),

-- Hải sản (ID_DanhMuc = 4)
(N'Cá hồi phi lê Nauy', 4, 450000, 'https://images.unsplash.com/photo-1519708227418-c8fd9a32b7a2?w=600', N'Cá hồi tươi nhập khẩu ăn sashimi'),
(N'Tôm sú sống', 4, 280000, 'https://images.unsplash.com/photo-1565680018434-b513d5e5fd47?w=600', N'Tôm sú thiên nhiên to khỏe'),

-- Đồ khô (ID_DanhMuc = 5)
(N'Gạo ST25 Ông Cua', 5, 45000, 'https://images.unsplash.com/photo-1586201375761-83865001e31c?w=600', N'Gạo ngon nhất thế giới, thơm lá dứa'),
(N'Khoai tâp ', 5, 150000, 'https://images.unsplash.com/photo-1518977676601-b53f82aba655?w=600', N'Nấm hương rừng sấy khô tự nhiên');

-- 5. Cấp hàng cho kho Chi nhánh 1
INSERT INTO KhoChiNhanh (ID_ChiNhanh, ID_SanPham, SoLuong)
SELECT 1, ID, 100 FROM SanPham;

-- 6. Thêm Voucher
INSERT INTO Voucher (MaVoucher, PhanTramGiam, GiamToiDa, SoLuong, NgayHetHan) VALUES
('FRESH10', 10, 30000, 100, '2027-12-31'),
('FREESHIP', 100, 15000, 50, '2027-12-31');

-- 7. Thêm Giỏ Hàng cho khach1 (ID = 4)
INSERT INTO GioHang (ID_KhachHang, ID_SanPham, SoLuong) VALUES
(4, 1, 2), -- 2kg Cà chua
(4, 5, 1); -- 1kg Táo Fuji

-- 8. Thêm Đơn Hàng Mẫu cho khach1
INSERT INTO DonHang (ID_KhachHang, ID_ChiNhanh, ID_Shipper, NgayDat, TrangThai, PhuongThucThanhToan, DiaChiGiao, TongTienHang, PhiShip, TienGiam, ThanhTien) 
VALUES (4, 1, 3, DATEADD(DAY, -5, GETDATE()), N'Đã giao', 'COD', N'123 Đường Nguyễn Trãi, Quận 1, TP.HCM', 170000, 15000, 0, 185000);

INSERT INTO DonHang (ID_KhachHang, ID_ChiNhanh, ID_Shipper, NgayDat, TrangThai, PhuongThucThanhToan, DiaChiGiao, TongTienHang, PhiShip, TienGiam, ThanhTien) 
VALUES (4, 1, 3, GETDATE(), N'Đang giao', 'BankTransfer', N'123 Đường Nguyễn Trãi, Quận 1, TP.HCM', 320000, 15000, 15000, 320000);

-- 9. Thêm Chi tiết Đơn Hàng
INSERT INTO ChiTietDonHang (ID_DonHang, ID_SanPham, SoLuong, DonGia) VALUES
(1, 5, 2, 85000),
(2, 10, 1, 320000);
GO
-- Thêm 3 sản phẩm mới vào bảng SanPham
INSERT INTO SanPham (TenSanPham, ID_DanhMuc, GiaBan, HinhAnh, MoTa) VALUES
(N'Chuối Laba Đà Lạt', 2, 30000, 'https://images.unsplash.com/photo-1571771894821-ce9b6c11b08e?w=600', N'Chuối Laba dẻo ngọt, giàu chất dinh dưỡng'),
(N'Măng tây xanh VietGAP', 1, 65000, 'https://images.unsplash.com/photo-1515471209610-dae1c92d8777?w=600', N'Măng tây giòn ngọt, mập rọng tươi ngon'),
(N'Cánh gà tươi', 3, 95000, 'https://images.unsplash.com/photo-1587593810167-a84920ea0781?w=600', N'Cánh gà ta tươi ngon, đóng gói khay sạch');

-- Cấp kho cho 3 sản phẩm mới (ID 16, 17, 18)
INSERT INTO KhoChiNhanh (ID_ChiNhanh, ID_SanPham, SoLuong)
VALUES 
(1, 16, 100),
(1, 17, 100),
(1, 18, 100);

GO

PRINT N'✅ Đã khởi tạo cấu trúc và làm mới dữ liệu chuẩn xác cho FreshGoDB!';