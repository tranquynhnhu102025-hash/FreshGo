USE master;
GO

-- ============================================================
-- PHẦN A: DATABASE CHO PHÂN HỆ QUẢN LÝ & SHIPPER (FreshGoDB)
-- ============================================================
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

CREATE TABLE ChiNhanh (
    MaChiNhanh INT IDENTITY(1,1) PRIMARY KEY,
    TenChiNhanh NVARCHAR(150) NOT NULL,
    KhuVucQuan NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(255) NOT NULL,
    PhiShipCoDinh DECIMAL(18,2) DEFAULT 15000,
    TrangThaiHoatDong BIT DEFAULT 1
);

CREATE TABLE TaiKhoan (
    MaTaiKhoan INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    SoDienThoai NVARCHAR(20) NOT NULL,
    MatKhau NVARCHAR(255) NOT NULL,
    VaiTro NVARCHAR(20) NOT NULL CHECK (VaiTro IN ('Admin', 'Manager', 'Shipper', 'Customer', 'NhanVien')),
    MaChiNhanh INT NULL FOREIGN KEY REFERENCES ChiNhanh(MaChiNhanh),
    BienSoXe NVARCHAR(30) NULL,
    CCCD NVARCHAR(20) NULL,
    SanSangNhanDon BIT DEFAULT 1,
    DiemTichLuy INT DEFAULT 0,
    TrangThaiKhoa BIT DEFAULT 0,
    NgayTao DATETIME DEFAULT GETDATE()
);

CREATE TABLE DanhMuc (
    MaDanhMuc INT IDENTITY(1,1) PRIMARY KEY,
    TenDanhMuc NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(255) NULL
);

CREATE TABLE ThucPham (
    MaThucPham INT IDENTITY(1,1) PRIMARY KEY,
    TenThucPham NVARCHAR(150) NOT NULL,
    MaDanhMuc INT FOREIGN KEY REFERENCES DanhMuc(MaDanhMuc),
    DonViTinh NVARCHAR(50) NOT NULL,
    GiaBan DECIMAL(18,2) NOT NULL,
    HinhAnh NVARCHAR(255) NULL,
    MoTa NVARCHAR(500) NULL,
    SoLuongKhoTong INT DEFAULT 100
);

CREATE TABLE TonKhoChiNhanh (
    MaTonKho INT IDENTITY(1,1) PRIMARY KEY,
    MaChiNhanh INT FOREIGN KEY REFERENCES ChiNhanh(MaChiNhanh),
    MaThucPham INT FOREIGN KEY REFERENCES ThucPham(MaThucPham),
    SoLuongTon INT DEFAULT 0,
    SoLuongLoiHu INT DEFAULT 0,
    LyDoLoiHu NVARCHAR(255) NULL,
    HanSuDung DATE NULL,
    TrangThaiKinhDoanh NVARCHAR(20) NOT NULL DEFAULT N'ConHang' CHECK (TrangThaiKinhDoanh IN ('ConHang', 'HetHang', 'HetHan'))
);

CREATE TABLE Voucher (
    MaVoucher INT IDENTITY(1,1) PRIMARY KEY,
    CodeVoucher NVARCHAR(50) UNIQUE NOT NULL,
    PhanTramGiam INT NOT NULL,
    GiamToiDa DECIMAL(18,2) NOT NULL,
    NgayHetHan DATE NOT NULL,
    SoLuongConLai INT DEFAULT 50
);

CREATE TABLE DonHang (
    MaDonHang INT IDENTITY(1,1) PRIMARY KEY,
    MaKhachHang INT FOREIGN KEY REFERENCES TaiKhoan(MaTaiKhoan),
    MaChiNhanh INT FOREIGN KEY REFERENCES ChiNhanh(MaChiNhanh),
    MaShipper INT NULL FOREIGN KEY REFERENCES TaiKhoan(MaTaiKhoan),
    DiaChiGiaoHang NVARCHAR(255) NOT NULL,
    SoDienThoaiNhan NVARCHAR(20) NOT NULL,
    TongTienHang DECIMAL(18,2) NOT NULL,
    PhiShip DECIMAL(18,2) NOT NULL,
    TienGiamVoucher DECIMAL(18,2) DEFAULT 0,
    TongThanhToan DECIMAL(18,2) NOT NULL,
    PhuongThucThanhToan NVARCHAR(50) DEFAULT N'COD',
    TrangThaiDon NVARCHAR(50) DEFAULT N'Chờ xử lý',
    HinhAnhXacNhanGiao NVARCHAR(255) NULL,
    LyDoThatBaiHoacHuy NVARCHAR(255) NULL,
    DaDoiSoatCOD BIT DEFAULT 0,
    DanhGiaSao INT NULL,
    PhanHoiKhachHang NVARCHAR(500) NULL,
    NgayDat DATETIME DEFAULT GETDATE()
);

CREATE TABLE ChiTietDonHang (
    MaChiTiet INT IDENTITY(1,1) PRIMARY KEY,
    MaDonHang INT FOREIGN KEY REFERENCES DonHang(MaDonHang),
    MaThucPham INT FOREIGN KEY REFERENCES ThucPham(MaThucPham),
    SoLuong INT NOT NULL,
    DonGiaLucMua DECIMAL(18,2) NOT NULL
);

CREATE TABLE YeuCauNhapHang (
    MaYeuCau INT IDENTITY(1,1) PRIMARY KEY,
    MaChiNhanh INT FOREIGN KEY REFERENCES ChiNhanh(MaChiNhanh),
    NgayTao DATETIME DEFAULT GETDATE(),
    NgayGui DATETIME NULL,
    TrangThai NVARCHAR(20) NOT NULL DEFAULT N'Nháp' CHECK (TrangThai IN (N'Nháp', N'Chờ duyệt'))
);

CREATE TABLE ChiTietYeuCauNhapHang (
    MaChiTiet INT IDENTITY(1,1) PRIMARY KEY,
    MaYeuCau INT FOREIGN KEY REFERENCES YeuCauNhapHang(MaYeuCau),
    MaThucPham INT FOREIGN KEY REFERENCES ThucPham(MaThucPham),
    SoLuongCan INT NOT NULL
);
GO

INSERT INTO ChiNhanh (TenChiNhanh, KhuVucQuan, DiaChi, PhiShipCoDinh) VALUES
(N'FreshGo Quận 12', N'Quận 12', N'331 Quốc lộ 1A, Quận 12', 15000),
(N'FreshGo Gò Vấp', N'Gò Vấp', N'120 Quang Trung, Gò Vấp', 20000);

INSERT INTO DanhMuc (TenDanhMuc) VALUES
(N'Rau củ'), (N'Trái cây'), (N'Thịt'), (N'Nước uống'), (N'Gia vị'), (N'Khác');

INSERT INTO TaiKhoan (HoTen, Email, SoDienThoai, MatKhau, VaiTro, MaChiNhanh, BienSoXe, CCCD, SanSangNhanDon) VALUES
(N'Zuns Nguyễn (Admin)', 'admin@freshgo.vn', '0901111111', '123456', 'Admin', NULL, NULL, NULL, 0),
(N'Hoàng Duy', 'manager@freshgo.vn', '0902222222', '123456', 'Manager', 1, NULL, '079099001234', 0),
(N'Gia Kiệt', 'shipper@freshgo.vn', '0903333333', '123456', 'Shipper', 1, '59-G1 123.45', '079098005678', 1),
(N'Trần Quỳnh Như', 'customer@freshgo.vn', '0904444444', '123456', 'Customer', NULL, NULL, NULL, 0),
(N'Thảo Vy', 'thaovy.q12@freshgo.vn', '0904123456', '123456', 'NhanVien', 1, NULL, '079201012345', 1),
(N'Lê Minh Khoa', 'minhkhoa@gmail.com', '0911222333', '123456', 'Customer', NULL, NULL, NULL, 1),
(N'Trần Gia Kiệt', 'anhkiet@gmail.com', '0912333444', '123456', 'Customer', NULL, NULL, NULL, 1),
(N'Nguyễn Hoàng Duy', 'giaduy@gmail.com', '0913444555', '123456', 'Customer', NULL, NULL, NULL, 1),
(N'Nguyễn Phúc Dũng', 'thanhhang@gmail.com', '0914555666', '123456', 'Customer', NULL, NULL, NULL, 1);
GO

INSERT INTO ThucPham (TenThucPham, MaDanhMuc, DonViTinh, GiaBan, MoTa, SoLuongKhoTong) VALUES
(N'Cà chua', (SELECT MaDanhMuc FROM DanhMuc WHERE TenDanhMuc = N'Rau củ'), N'kg', 28000, N'Cà chua Đà Lạt', 100),
(N'Rau muống', (SELECT MaDanhMuc FROM DanhMuc WHERE TenDanhMuc = N'Rau củ'), N'bó', 8000, N'Rau muống sạch', 100),
(N'Táo Envy', (SELECT MaDanhMuc FROM DanhMuc WHERE TenDanhMuc = N'Trái cây'), N'kg', 85000, N'Táo nhập khẩu New Zealand', 100),
(N'Chuối già', (SELECT MaDanhMuc FROM DanhMuc WHERE TenDanhMuc = N'Trái cây'), N'nải', 30000, N'Chuối già Nam Mỹ', 100),
(N'Thịt ba chỉ', (SELECT MaDanhMuc FROM DanhMuc WHERE TenDanhMuc = N'Thịt'), N'kg', 150000, N'Thịt heo ba chỉ tươi', 100),
(N'Dưa leo', (SELECT MaDanhMuc FROM DanhMuc WHERE TenDanhMuc = N'Rau củ'), N'kg', 12000, NULL, 0),
(N'Bánh tráng trộn', (SELECT MaDanhMuc FROM DanhMuc WHERE TenDanhMuc = N'Khác'), N'gói', 20000, NULL, 0),
(N'Ổi', (SELECT MaDanhMuc FROM DanhMuc WHERE TenDanhMuc = N'Trái cây'), N'kg', 25000, NULL, 0);

INSERT INTO TonKhoChiNhanh (MaChiNhanh, MaThucPham, SoLuongTon, SoLuongLoiHu, LyDoLoiHu, HanSuDung, TrangThaiKinhDoanh) VALUES
(1, 1, 35, 0, NULL, '2026-10-29', N'ConHang'),
(1, 2, 8, 1, N'Dập úng lúc vận chuyển', '2026-10-26', N'ConHang'),
(1, 3, 0, 0, NULL, '2026-11-13', N'ConHang'),
(1, 4, 25, 0, NULL, '2026-10-23', N'HetHang'),
(1, 5, 20, 0, NULL, NULL, N'ConHang'),
(1, 6, 50, 0, NULL, NULL, N'ConHang'),
(1, 7, 15, 0, NULL, NULL, N'ConHang'),
(1, 8, 0, 0, NULL, NULL, N'ConHang');

INSERT INTO DonHang (MaKhachHang, MaChiNhanh, MaShipper, DiaChiGiaoHang, SoDienThoaiNhan, TongTienHang, PhiShip, TienGiamVoucher, TongThanhToan, PhuongThucThanhToan, TrangThaiDon, DaDoiSoatCOD, DanhGiaSao, PhanHoiKhachHang, NgayDat) VALUES
(4, 1, 3, N'12 Lê Văn Khương, Quận 12', '0904444444', 185000, 15000, 0, 200000, N'COD', N'Hoàn tất', 0, NULL, NULL, '2026-10-15 11:24:39'),
(6, 1, 3, N'12 Lê Văn Khương, Quận 12', '0904444444', 320000, 15000, 20000, 315000, N'COD', N'Hoàn tất', 0, NULL, NULL, '2026-10-17 11:24:39'),
(7, 1, 3, N'12 Lê Văn Khương, Quận 12', '0904444444', 95000, 15000, 0, 110000, N'Chuyển khoản', N'Hoàn tất', 0, NULL, NULL, '2026-10-19 11:24:39'),
(8, 1, 3, N'12 Lê Văn Khương, Quận 12', '0904444444', 410000, 15000, 30000, 395000, N'COD', N'Hoàn tất', 0, NULL, NULL, '2026-10-21 11:24:39'),
(9, 1, 3, N'12 Lê Văn Khương, Quận 12', '0904444444', 150000, 15000, 0, 165000, N'COD', N'Hoàn tất', 0, NULL, NULL, '2026-10-22 11:24:39'),
(6, 1, 3, N'12 Lê Văn Khương, Quận 12', '0904444444', 260000, 15000, 0, 275000, N'Chuyển khoản', N'Hoàn tất', 0, NULL, NULL, '2026-10-23 11:24:39'),
(7, 1, 3, N'12 Lê Văn Khương, Quận 12', '0904444444', 180000, 15000, 0, 195000, N'COD', N'Hoàn tất', 0, 5, N'Giao nhanh, đóng gói cẩn thận.', '2026-10-24 11:24:39'),
(4, 1, 3, N'12 Lê Văn Khương, Quận 12', '0904444444', 220000, 15000, 0, 235000, N'COD', N'Đang giao', 0, NULL, NULL, '2026-10-24 11:24:39'),
(6, 1, 3, N'12 Lê Văn Khương, Quận 12', '0904444444', 90000, 15000, 0, 105000, N'COD', N'Đang giao', 0, NULL, NULL, '2026-10-24 11:24:39'),
(7, 1, 3, N'45 Tô Ký, Quận 12', '0905111222', 80000, 15000, 0, 95000, N'COD', N'Đang giao', 0, NULL, NULL, '2026-10-25 00:57:27'),
(8, 1, NULL, N'12A Hà Huy Giáp, Quận 12', '0905333444', 85000, 15000, 10000, 90000, N'Chuyển khoản', N'Chờ xử lý', 0, NULL, NULL, '2026-10-25 00:57:27'),
(6, 1, NULL, N'78 Nguyễn Văn Quá, Quận 12', '0905555666', 174000, 15000, 0, 189000, N'COD', N'Chờ xử lý', 0, NULL, NULL, '2026-10-25 00:57:27'),
(4, 1, NULL, N'9 Lê Thị Riêng, Quận 12', '0905777888', 106000, 15000, 5000, 116000, N'Chuyển khoản', N'Chờ xử lý', 0, NULL, NULL, '2026-10-25 00:57:27'),
(9, 1, 3, N'22 Lê Đức Thọ, Quận 12', '0905999000', 56000, 15000, 0, 71000, N'COD', N'Đã hủy', 0, NULL, NULL, '2026-10-25 01:16:51');

UPDATE DonHang SET LyDoThatBaiHoacHuy = N'Khách hàng không nghe máy, hủy đơn do giao không thành công.' WHERE MaDonHang = 14;

INSERT INTO ChiTietDonHang (MaDonHang, MaThucPham, SoLuong, DonGiaLucMua) VALUES
(1, 1, 3, 25000), (1, 2, 5, 8000), (1, 4, 3, 30000),
(7, 5, 1, 150000), (7, 1, 1, 25000),
(8, 3, 2, 85000), (8, 2, 5, 8000),
(9, 6, 4, 12000), (9, 4, 1, 30000),
(10, 1, 2, 28000), (10, 2, 3, 8000),
(11, 3, 1, 85000),
(12, 5, 1, 150000), (12, 6, 2, 12000),
(13, 4, 3, 30000), (13, 2, 2, 8000),
(14, 1, 2, 28000);
GO

-- ============================================================
-- PHẦN B: DATABASE CHO PHÂN HỆ KHÁCH HÀNG (FreshGoCustomerDB)
-- ============================================================
USE master;
GO

IF DB_ID('FreshGoCustomerDB') IS NOT NULL
BEGIN
    ALTER DATABASE FreshGoCustomerDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FreshGoCustomerDB;
END
GO

CREATE DATABASE FreshGoCustomerDB;
GO
USE FreshGoCustomerDB;
GO

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

INSERT INTO TaiKhoan (Username, PasswordHash, HoTen, SoDienThoai, Role) VALUES
('admin',    '123456', N'Quản trị viên Hệ thống', '0900000001', 'Admin'),
('manager1', '123456', N'Quản lý Chi nhánh Q1',  '0900000002', 'Manager'),
('shipper1', '123456', N'Shipper Nguyễn Văn A',   '0900000003', 'Shipper'),
('khach1',   '123456', N'Khách Trần B',           '0900000004', 'Customer'),
('khach2',   '123456', N'Khách Lê C',             '0900000005', 'Customer');

INSERT INTO KhuVuc (TenKhuVuc, PhiGiaoHang) VALUES
(N'Quận 1', 15000), 
(N'Quận 7', 20000);

INSERT INTO ChiNhanh (TenChiNhanh, DiaChi, ID_KhuVuc, ID_QuanLy) VALUES
(N'FreshGo Quận 1', N'12 Lê Lợi, Bến Nghé, Quận 1, TP.HCM', 1, 2);

INSERT INTO DanhMuc (TenDanhMuc) VALUES
(N'Rau củ hữu cơ'), 
(N'Trái cây tươi'), 
(N'Thịt heo & Bò'),
(N'Thủy hải sản'),
(N'Thực phẩm khô');

INSERT INTO SanPham (TenSanPham, ID_DanhMuc, GiaBan, HinhAnh, MoTa) VALUES
(N'Cà chua VietGAP', 1, 25000, 'https://images.unsplash.com/photo-1592924357228-91a4daadcfea?w=600', N'Cà chua tươi Đà Lạt, chua ngọt tự nhiên'),
(N'Rau muống thủy canh', 1, 15000, 'https://images.unsplash.com/photo-1576045057995-568f588f82fb?w=600', N'Rau muống giòn, xanh mướt, không thuốc trừ sâu'),
(N'Súp lơ xanh (Bông cải)', 1, 35000, 'https://images.unsplash.com/photo-1459411621453-7b03977f4bfc?w=600', N'Súp lơ xanh giàu vitamin'),
(N'Cà rốt Đà Lạt', 1, 20000, 'https://images.unsplash.com/photo-1598170845058-32b9d6a5da37?w=600', N'Cà rốt tươi ngọt nguyên củ'),
(N'Táo Fuji Nhập Khẩu', 2, 85000, 'https://images.unsplash.com/photo-1568702846914-96b305d2aaeb?w=600', N'Táo đỏ giòn ngọt, xuất xứ New Zealand'),
(N'Cam sành Miền Tây', 2, 45000, 'https://images.unsplash.com/photo-1611080626919-7cf5a9dbab5b?w=600', N'Cam sành mọng nước, nhiều vitamin C'),
(N'Dưa hấu không hạt', 2, 22000, 'https://images.unsplash.com/photo-1589984662646-e7b2e4962f18?w=600', N'Dưa hấu ruột đỏ ngọt lịm'),
(N'Nho xanh không hạt', 2, 120000, 'https://images.unsplash.com/photo-1596363505729-4190a9506133?w=600', N'Nho xanh Úc giòn tan'),
(N'Thịt ba chỉ heo', 3, 140000, 'https://images.unsplash.com/photo-1544025162-d76694265947?w=600', N'Thịt heo tươi giết mổ trong ngày'),
(N'Thịt bò thăn Úc', 3, 320000, 'https://images.unsplash.com/photo-1551028150-64b9f398f678?w=600', N'Bò thăn nạc, mềm, thích hợp làm bít tết'),
(N'Ức gà ta', 3, 85000, 'https://images.unsplash.com/photo-1604908176997-125f25cc6f3d?w=600', N'Ức gà ta dai ngon, phù hợp ăn kiêng'),
(N'Cá hồi phi lê Nauy', 4, 450000, 'https://images.unsplash.com/photo-1519708227418-c8fd9a32b7a2?w=600', N'Cá hồi tươi nhập khẩu ăn sashimi'),
(N'Tôm sú sống', 4, 280000, 'https://images.unsplash.com/photo-1565680018434-b513d5e5fd47?w=600', N'Tôm sú thiên nhiên to khỏe'),
(N'Gạo ST25 Ông Cua', 5, 45000, 'https://images.unsplash.com/photo-1586201375761-83865001e31c?w=600', N'Gạo ngon nhất thế giới, thơm lá dứa'),
(N'Nấm hương rừng', 5, 150000, 'https://images.unsplash.com/photo-1518977676601-b53f82aba655?w=600', N'Nấm hương rừng sấy khô tự nhiên');

INSERT INTO KhoChiNhanh (ID_ChiNhanh, ID_SanPham, SoLuong)
SELECT 1, ID, 100 FROM SanPham;

INSERT INTO Voucher (MaVoucher, PhanTramGiam, GiamToiDa, SoLuong, NgayHetHan) VALUES
('FRESH10', 10, 30000, 100, '2027-12-31'),
('FREESHIP', 100, 15000, 50, '2027-12-31');

INSERT INTO GioHang (ID_KhachHang, ID_SanPham, SoLuong) VALUES
(4, 1, 2),
(4, 5, 1);

INSERT INTO DonHang (ID_KhachHang, ID_ChiNhanh, ID_Shipper, NgayDat, TrangThai, PhuongThucThanhToan, DiaChiGiao, TongTienHang, PhiShip, TienGiam, ThanhTien) 
VALUES (4, 1, 3, DATEADD(DAY, -5, GETDATE()), N'Đã giao', 'COD', N'123 Đường Nguyễn Trãi, Quận 1, TP.HCM', 170000, 15000, 0, 185000);

INSERT INTO DonHang (ID_KhachHang, ID_ChiNhanh, ID_Shipper, NgayDat, TrangThai, PhuongThucThanhToan, DiaChiGiao, TongTienHang, PhiShip, TienGiam, ThanhTien) 
VALUES (4, 1, 3, GETDATE(), N'Đang giao', 'BankTransfer', N'123 Đường Nguyễn Trãi, Quận 1, TP.HCM', 320000, 15000, 15000, 320000);

INSERT INTO ChiTietDonHang (ID_DonHang, ID_SanPham, SoLuong, DonGia) VALUES
(1, 5, 2, 85000),
(2, 10, 1, 320000);

INSERT INTO SanPham (TenSanPham, ID_DanhMuc, GiaBan, HinhAnh, MoTa) VALUES
(N'Chuối Laba Đà Lạt', 2, 30000, 'https://images.unsplash.com/photo-1571771894821-ce9b6c11b08e?w=600', N'Chuối Laba dẻo ngọt, giàu chất dinh dưỡng'),
(N'Măng tây xanh VietGAP', 1, 65000, 'https://images.unsplash.com/photo-1515471209610-dae1c92d8777?w=600', N'Măng tây giòn ngọt, mập rọng tươi ngon'),
(N'Cánh gà tươi', 3, 95000, 'https://images.unsplash.com/photo-1587593810167-a84920ea0781?w=600', N'Cánh gà ta tươi ngon, đóng gói khay sạch');

INSERT INTO KhoChiNhanh (ID_ChiNhanh, ID_SanPham, SoLuong)
VALUES 
(1, 16, 100),
(1, 17, 100),
(1, 18, 100);
GO

-- ============================================================
-- PHẦN C: DATABASE CHO PHÂN HỆ ADMIN MỚI (FreshGoAdminDB)
-- ============================================================
USE master;
GO

IF DB_ID(N'FreshGoAdminDB') IS NOT NULL
BEGIN
    ALTER DATABASE FreshGoAdminDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FreshGoAdminDB;
END
GO

CREATE DATABASE FreshGoAdminDB;
GO
USE FreshGoAdminDB;
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

PRINT N'✅ Đã tạo thành công cả 3 database: FreshGoDB (Quản lý/Shipper), FreshGoCustomerDB (Khách hàng), và FreshGoAdminDB (Admin)!';
GO