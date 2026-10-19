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
    VaiTro NVARCHAR(20) NOT NULL CHECK (VaiTro IN ('Admin', 'Manager', 'Shipper', 'Customer')),
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
    HanSuDung DATE NULL
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
GO

INSERT INTO ChiNhanh (TenChiNhanh, KhuVucQuan, DiaChi, PhiShipCoDinh) VALUES
(N'FreshGo Quận 12', N'Quận 12', N'331 Quốc lộ 1A, Quận 12', 15000),
(N'FreshGo Gò Vấp', N'Gò Vấp', N'120 Quang Trung, Gò Vấp', 20000);

INSERT INTO TaiKhoan (HoTen, Email, SoDienThoai, MatKhau, VaiTro, MaChiNhanh, BienSoXe, SanSangNhanDon) VALUES
(N'Zuns Nguyễn (Admin)', 'admin@freshgo.vn', '0901111111', '123456', 'Admin', NULL, NULL, 0),
(N'Hoàng Duy (Quản lý Q12)', 'manager@freshgo.vn', '0902222222', '123456', 'Manager', 1, NULL, 0),
(N'Gia Kiệt (Shipper Q12)', 'shipper@freshgo.vn', '0903333333', '123456', 'Shipper', 1, '59-G1 123.45', 1),
(N'Trần Quỳnh Như (Khách hàng)', 'customer@freshgo.vn', '0904444444', '123456', 'Customer', NULL, NULL, 0);
GO
