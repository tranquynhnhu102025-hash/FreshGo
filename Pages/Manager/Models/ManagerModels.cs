namespace FreshGo.Pages.Manager.Models;

public class ChiNhanhInfo
{
    public int MaChiNhanh { get; set; }
    public string TenChiNhanh { get; set; } = string.Empty;
    public string KhuVucQuan { get; set; } = string.Empty;
    public string DiaChi { get; set; } = string.Empty;
    public decimal PhiShipCoDinh { get; set; }
    public bool TrangThaiHoatDong { get; set; }
}

public class DanhMucOption
{
    public int MaDanhMuc { get; set; }
    public string TenDanhMuc { get; set; } = string.Empty;
}

public class TonKhoItem
{
    public int MaTonKho { get; set; }
    public int MaThucPham { get; set; }
    public string TenThucPham { get; set; } = string.Empty;
    public string? HinhAnh { get; set; }
    public string TenDanhMuc { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public int SoLuongLoiHu { get; set; }
    public string? LyDoLoiHu { get; set; }
    public DateTime? HanSuDung { get; set; }

    // Trạng thái do Quản lý chi nhánh tự chọn tay (không tự tính nữa): ConHang | HetHang | HetHan
    public string TrangThaiKinhDoanh { get; set; } = TrangThaiOptions.ConHang;

    public string TrangThaiLabel => TrangThaiOptions.ToLabel(TrangThaiKinhDoanh);
}

public static class TrangThaiOptions
{
    public const string ConHang = "ConHang";
    public const string HetHang = "HetHang";
    public const string HetHan = "HetHan";

    public static readonly (string Value, string Label)[] All =
    {
        (ConHang, "Còn hàng"),
        (HetHang, "Hết hàng"),
        (HetHan, "Hết hạn")
    };

    public static string ToLabel(string value) => value switch
    {
        HetHang => "Hết hàng",
        HetHan => "Hết hạn",
        _ => "Còn hàng"
    };
}

public class DashboardStats
{
    public int TongMatHang { get; set; }
    public int TongSoLuongTon { get; set; }
    public int SoMatHangConHang { get; set; }
    public int SoMatHangHetHang { get; set; }
    public int SoMatHangLoiHu { get; set; }
    public int SoMatHangHetHan { get; set; }
}

// Doanh thu tính trên đơn hàng có TrangThaiDon = "Hoàn tất" tại chi nhánh.
public class DoanhThuSummary
{
    public decimal TongDoanhThu { get; set; }
    public decimal TongDoanhThuTienMat { get; set; }
    public decimal TongDoanhThuChuyenKhoan { get; set; }
    public int SoDonHoanTat { get; set; }
    public int SoDonThatBai { get; set; }
}

public class DoanhThuNgay
{
    public DateTime Ngay { get; set; }
    public decimal DoanhThu { get; set; }
}

public class DonHangGanDay
{
    public int MaDonHang { get; set; }
    public DateTime NgayDat { get; set; }
    public decimal TongThanhToan { get; set; }
    public string TrangThaiDon { get; set; } = string.Empty;
    public string PhuongThucThanhToan { get; set; } = string.Empty;

    // Doanh thu chỉ quan tâm 2 trạng thái: Hoàn tất (tính vào doanh thu) hoặc Thất bại.
    // Các trạng thái đang xử lý (Chờ xử lý, Đang giao) được quản lý chi tiết bên phân hệ Đơn hàng.
    public bool ThanhCong => TrangThaiDon == TrangThaiDonOptions.HoanTat;

    public string TrangThaiLabel => ThanhCong ? "Hoàn tất" : "Thất bại";

    public string PhuongThucLabel => PhuongThucThanhToan switch
    {
        "COD" => "Tiền mặt",
        _ => "Chuyển khoản"
    };
}

public class DonHangListItem
{
    public int MaDonHang { get; set; }
    public DateTime NgayDat { get; set; }
    public string TenKhachHang { get; set; } = string.Empty;
    public decimal TongThanhToan { get; set; }
    public string TrangThaiDon { get; set; } = string.Empty;
    public string? TenShipper { get; set; }

    public string PhuongThucLabel { get; set; } = string.Empty;
}

public class SanPhamTrongDon
{
    public string TenThucPham { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal DonGiaLucMua { get; set; }
    public decimal ThanhTien => SoLuong * DonGiaLucMua;
}

public class DonHangChiTiet
{
    public int MaDonHang { get; set; }
    public DateTime NgayDat { get; set; }
    public string TrangThaiDon { get; set; } = string.Empty;

    // Khách hàng
    public string TenKhachHang { get; set; } = string.Empty;

    // Thông tin giao hàng
    public string DiaChiGiaoHang { get; set; } = string.Empty;
    public string SoDienThoaiNhan { get; set; } = string.Empty;
    public string PhuongThucThanhToan { get; set; } = string.Empty;
    public string PhuongThucLabel => PhuongThucThanhToan == "COD" ? "Tiền mặt" : "Chuyển khoản";
    public string? HinhAnhXacNhanGiao { get; set; }
    public string? LyDoThatBaiHoacHuy { get; set; }
    public int? DanhGiaSao { get; set; }
    public string? PhanHoiKhachHang { get; set; }

    // Tiền
    public decimal TongTienHang { get; set; }
    public decimal PhiShip { get; set; }
    public decimal TienGiamVoucher { get; set; }
    public decimal TongThanhToan { get; set; }
    public bool DaDoiSoatCOD { get; set; }

    // Hồ sơ người giao hàng (null nếu đơn chưa được gán shipper)
    public string? TenShipper { get; set; }
    public string? SdtShipper { get; set; }
    public string? BienSoXeShipper { get; set; }
    public string? CccdShipper { get; set; }
    public bool? SanSangNhanDonShipper { get; set; }
    public int? DiemTichLuyShipper { get; set; }

    public List<SanPhamTrongDon> SanPham { get; set; } = new();
}

public static class TrangThaiDonOptions
{
    public const string ChoXuLy = "Chờ xử lý";
    public const string DangGiao = "Đang giao";
    public const string HoanTat = "Hoàn tất";
    public const string DaHuy = "Đã hủy";

    public static string BadgeClass(string trangThai) => trangThai switch
    {
        HoanTat => "badge-status-conhang",
        DangGiao => "badge-status-danggiao",
        DaHuy => "badge-status-hethan",
        _ => "badge-status-hethang" // Chờ xử lý và các trạng thái khác
    };
}

// Người giao hàng (VaiTro = Shipper) của chi nhánh, dùng để chọn khi xử lý đơn.
public class ShipperOption
{
    public int MaTaiKhoan { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public bool SanSangNhanDon { get; set; }
}

// Hồ sơ đầy đủ của một shipper thuộc chi nhánh, hiển thị ở trang Thông tin chi nhánh.
public class NhanVienGiaoHang
{
    public int MaTaiKhoan { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string? BienSoXe { get; set; }
    public string? CCCD { get; set; }
}

// Hồ sơ nhân viên đóng gói/thanh toán tại quầy (VaiTro = NhanVien), hiển thị ở trang Thông tin chi nhánh.
public class NhanVienDongGoi
{
    public int MaTaiKhoan { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string? CCCD { get; set; }
}

// Hồ sơ tài khoản Quản lý phụ trách chi nhánh, hiển thị ở trang Thông tin chi nhánh.
public class QuanLyChiNhanh
{
    public int MaTaiKhoan { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string? CCCD { get; set; }
}

// Một dòng trong yêu cầu nhập hàng đang soạn (bản Nháp) của chi nhánh, gửi lên kho tổng khi bấm "Yêu cầu".
public class YeuCauNhapHangDong
{
    public int MaChiTiet { get; set; }
    public int MaThucPham { get; set; }
    public string TenThucPham { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public int SoLuongCan { get; set; }
}
