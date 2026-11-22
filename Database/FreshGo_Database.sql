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

-- Yêu cầu nhập hàng do Quản lý chi nhánh gửi lên kho tổng khi có mặt hàng hết hàng cần bổ sung.
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

-- Danh mục nền tảng cần có sẵn để phân hệ Quản lý chi nhánh (Thêm mặt hàng) hoạt động được ngay sau khi tạo CSDL.
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

-- Sản phẩm mẫu, thứ tự chèn phải giữ nguyên để MaThucPham khớp với MaDanhMuc/TonKhoChiNhanh/ChiTietDonHang bên dưới.
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

-- Đơn hàng mẫu. MaKhachHang/MaShipper tham chiếu đúng thứ tự tài khoản đã chèn ở trên (4=Trần Quỳnh Như, 6-9=khách hàng mới, 3=Gia Kiệt).
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

-- Sản phẩm trong từng đơn, theo đúng thứ tự MaDonHang ở trên (1-14).
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
