using FreshGo.Pages.Manager.Models;
using Microsoft.Data.SqlClient;

namespace FreshGo.Pages.Manager.Services;

// Đăng nhập/phân quyền cho Manager chưa được xây (nằm ngoài phạm vi phân hệ này),
// nên tạm cố định chi nhánh đang quản lý theo dữ liệu mẫu (Hoàng Duy - FreshGo Quận 12).
public static class BranchContext
{
    public const int CurrentBranchId = 1;
}

public class ManagerRepository
{
    private readonly string _connectionString;

    public ManagerRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Thiếu chuỗi kết nối DefaultConnection trong appsettings.json");
    }

    private SqlConnection CreateConnection() => new(_connectionString);

    public async Task<ChiNhanhInfo?> GetChiNhanhAsync(int maChiNhanh)
    {
        const string sql = @"SELECT MaChiNhanh, TenChiNhanh, KhuVucQuan, DiaChi, PhiShipCoDinh, TrangThaiHoatDong
                              FROM ChiNhanh WHERE MaChiNhanh = @MaChiNhanh";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new ChiNhanhInfo
        {
            MaChiNhanh = reader.GetInt32(0),
            TenChiNhanh = reader.GetString(1),
            KhuVucQuan = reader.GetString(2),
            DiaChi = reader.GetString(3),
            PhiShipCoDinh = reader.GetDecimal(4),
            TrangThaiHoatDong = reader.GetBoolean(5)
        };
    }

    public async Task UpdateChiNhanhAsync(ChiNhanhInfo model)
    {
        const string sql = @"UPDATE ChiNhanh SET TenChiNhanh = @Ten, KhuVucQuan = @KhuVuc, DiaChi = @DiaChi,
                              PhiShipCoDinh = @Phi, TrangThaiHoatDong = @TrangThai
                              WHERE MaChiNhanh = @MaChiNhanh";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Ten", model.TenChiNhanh);
        cmd.Parameters.AddWithValue("@KhuVuc", model.KhuVucQuan);
        cmd.Parameters.AddWithValue("@DiaChi", model.DiaChi);
        cmd.Parameters.AddWithValue("@Phi", model.PhiShipCoDinh);
        cmd.Parameters.AddWithValue("@TrangThai", model.TrangThaiHoatDong);
        cmd.Parameters.AddWithValue("@MaChiNhanh", model.MaChiNhanh);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<DashboardStats> GetDashboardStatsAsync(int maChiNhanh)
    {
        const string sql = @"
            SELECT
                COUNT(*) AS TongMatHang,
                ISNULL(SUM(SoLuongTon), 0) AS TongSoLuongTon,
                SUM(CASE WHEN TrangThaiKinhDoanh = 'ConHang' THEN 1 ELSE 0 END) AS ConHang,
                SUM(CASE WHEN TrangThaiKinhDoanh = 'HetHang' THEN 1 ELSE 0 END) AS HetHang,
                SUM(CASE WHEN SoLuongLoiHu > 0 THEN 1 ELSE 0 END) AS LoiHu,
                SUM(CASE WHEN TrangThaiKinhDoanh = 'HetHan' THEN 1 ELSE 0 END) AS HetHan
            FROM TonKhoChiNhanh WHERE MaChiNhanh = @MaChiNhanh";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return new DashboardStats();

        return new DashboardStats
        {
            TongMatHang = reader.GetInt32(0),
            TongSoLuongTon = reader.GetInt32(1),
            SoMatHangConHang = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
            SoMatHangHetHang = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
            SoMatHangLoiHu = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
            SoMatHangHetHan = reader.IsDBNull(5) ? 0 : reader.GetInt32(5)
        };
    }

    public async Task<List<DanhMucOption>> GetDanhMucAsync()
    {
        // "Khác" luôn xếp cuối danh sách vì là danh mục gộp chung, các danh mục còn lại sắp theo bảng chữ cái.
        const string sql = @"
            SELECT MaDanhMuc, TenDanhMuc FROM DanhMuc
            ORDER BY CASE WHEN TenDanhMuc = N'Khác' THEN 1 ELSE 0 END, TenDanhMuc";

        var list = new List<DanhMucOption>();
        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new DanhMucOption { MaDanhMuc = reader.GetInt32(0), TenDanhMuc = reader.GetString(1) });
        }
        return list;
    }

    public async Task<List<TonKhoItem>> GetTonKhoAsync(int maChiNhanh, string? search, int? maDanhMuc, string? trangThaiLoc = null)
    {
        var sql = @"
            SELECT tk.MaTonKho, tp.MaThucPham, tp.TenThucPham, tp.HinhAnh, dm.TenDanhMuc,
                   tp.DonViTinh, tp.GiaBan, tk.SoLuongTon, tk.SoLuongLoiHu, tk.LyDoLoiHu, tk.HanSuDung, tk.TrangThaiKinhDoanh
            FROM TonKhoChiNhanh tk
            JOIN ThucPham tp ON tp.MaThucPham = tk.MaThucPham
            JOIN DanhMuc dm ON dm.MaDanhMuc = tp.MaDanhMuc
            WHERE tk.MaChiNhanh = @MaChiNhanh";

        if (!string.IsNullOrWhiteSpace(search)) sql += " AND tp.TenThucPham LIKE @Search";
        if (maDanhMuc is > 0) sql += " AND tp.MaDanhMuc = @MaDanhMuc";
        if (!string.IsNullOrWhiteSpace(trangThaiLoc)) sql += " AND tk.TrangThaiKinhDoanh = @TrangThai";
        sql += " ORDER BY tp.TenThucPham";

        var list = new List<TonKhoItem>();
        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
        if (!string.IsNullOrWhiteSpace(search)) cmd.Parameters.AddWithValue("@Search", $"%{search}%");
        if (maDanhMuc is > 0) cmd.Parameters.AddWithValue("@MaDanhMuc", maDanhMuc.Value);
        if (!string.IsNullOrWhiteSpace(trangThaiLoc)) cmd.Parameters.AddWithValue("@TrangThai", trangThaiLoc);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new TonKhoItem
            {
                MaTonKho = reader.GetInt32(0),
                MaThucPham = reader.GetInt32(1),
                TenThucPham = reader.GetString(2),
                HinhAnh = reader.IsDBNull(3) ? null : reader.GetString(3),
                TenDanhMuc = reader.GetString(4),
                DonViTinh = reader.GetString(5),
                GiaBan = reader.GetDecimal(6),
                SoLuongTon = reader.GetInt32(7),
                SoLuongLoiHu = reader.GetInt32(8),
                LyDoLoiHu = reader.IsDBNull(9) ? null : reader.GetString(9),
                HanSuDung = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                TrangThaiKinhDoanh = reader.GetString(11)
            });
        }
        return list;
    }

    public async Task<TonKhoItem?> GetTonKhoItemAsync(int maTonKho, int maChiNhanh)
    {
        const string sql = @"
            SELECT tk.MaTonKho, tp.MaThucPham, tp.TenThucPham, tp.HinhAnh, dm.TenDanhMuc,
                   tp.DonViTinh, tp.GiaBan, tk.SoLuongTon, tk.SoLuongLoiHu, tk.LyDoLoiHu, tk.HanSuDung, tk.TrangThaiKinhDoanh
            FROM TonKhoChiNhanh tk
            JOIN ThucPham tp ON tp.MaThucPham = tk.MaThucPham
            JOIN DanhMuc dm ON dm.MaDanhMuc = tp.MaDanhMuc
            WHERE tk.MaTonKho = @MaTonKho AND tk.MaChiNhanh = @MaChiNhanh";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaTonKho", maTonKho);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new TonKhoItem
        {
            MaTonKho = reader.GetInt32(0),
            MaThucPham = reader.GetInt32(1),
            TenThucPham = reader.GetString(2),
            HinhAnh = reader.IsDBNull(3) ? null : reader.GetString(3),
            TenDanhMuc = reader.GetString(4),
            DonViTinh = reader.GetString(5),
            GiaBan = reader.GetDecimal(6),
            SoLuongTon = reader.GetInt32(7),
            SoLuongLoiHu = reader.GetInt32(8),
            LyDoLoiHu = reader.IsDBNull(9) ? null : reader.GetString(9),
            HanSuDung = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
            TrangThaiKinhDoanh = reader.GetString(11)
        };
    }

    public async Task UpdateTonKhoAsync(int maTonKho, int maChiNhanh, int maThucPham, int soLuongTon, int soLuongLoiHu,
        string? lyDoLoiHu, DateTime? hanSuDung, string trangThaiKinhDoanh, int giaBan)
    {
        const string sql = @"
            UPDATE TonKhoChiNhanh
            SET SoLuongTon = @SoLuong, SoLuongLoiHu = @LoiHu, LyDoLoiHu = @LyDo, HanSuDung = @HanSuDung, TrangThaiKinhDoanh = @TrangThai
            WHERE MaTonKho = @MaTonKho AND MaChiNhanh = @MaChiNhanh;

            UPDATE ThucPham SET GiaBan = @GiaBan WHERE MaThucPham = @MaThucPham;";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SoLuong", soLuongTon);
        cmd.Parameters.AddWithValue("@LoiHu", soLuongLoiHu);
        cmd.Parameters.AddWithValue("@LyDo", (object?)lyDoLoiHu ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@HanSuDung", (object?)hanSuDung ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TrangThai", trangThaiKinhDoanh);
        cmd.Parameters.AddWithValue("@MaTonKho", maTonKho);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
        cmd.Parameters.AddWithValue("@GiaBan", giaBan);
        cmd.Parameters.AddWithValue("@MaThucPham", maThucPham);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteTonKhoAsync(int maTonKho, int maChiNhanh)
    {
        const string sql = "DELETE FROM TonKhoChiNhanh WHERE MaTonKho = @MaTonKho AND MaChiNhanh = @MaChiNhanh";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaTonKho", maTonKho);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    // Tạo một sản phẩm hoàn toàn mới (chưa từng có trong catalog ThucPham) rồi đưa luôn vào kho chi nhánh.
    public async Task AddThucPhamMoiVaoKhoAsync(int maChiNhanh, string tenThucPham, int maDanhMuc, string donViTinh,
        int giaBan, int soLuongTon, DateTime? hanSuDung, string trangThaiKinhDoanh)
    {
        const string sql = @"
            INSERT INTO ThucPham (TenThucPham, MaDanhMuc, DonViTinh, GiaBan, SoLuongKhoTong)
            VALUES (@Ten, @MaDanhMuc, @DonVi, @GiaBan, 0);

            DECLARE @NewId INT = SCOPE_IDENTITY();

            INSERT INTO TonKhoChiNhanh (MaChiNhanh, MaThucPham, SoLuongTon, SoLuongLoiHu, HanSuDung, TrangThaiKinhDoanh)
            VALUES (@MaChiNhanh, @NewId, @SoLuong, 0, @HanSuDung, @TrangThai);";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Ten", tenThucPham);
        cmd.Parameters.AddWithValue("@MaDanhMuc", maDanhMuc);
        cmd.Parameters.AddWithValue("@DonVi", donViTinh);
        cmd.Parameters.AddWithValue("@GiaBan", giaBan);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
        cmd.Parameters.AddWithValue("@SoLuong", soLuongTon);
        cmd.Parameters.AddWithValue("@HanSuDung", (object?)hanSuDung ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TrangThai", trangThaiKinhDoanh);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    // Doanh thu chỉ tính trên đơn có TrangThaiDon = "Hoàn tất". Khi phân hệ Đơn hàng được xây,
    // nếu chuỗi trạng thái thực tế khác đi thì chỉ cần sửa hằng số này.
    public const string TrangThaiDonHoanTat = "Hoàn tất";

    public async Task<DoanhThuSummary> GetDoanhThuSummaryAsync(int maChiNhanh, DateTime? tuNgay, DateTime? denNgayExclusive)
    {
        var sql = @"
            SELECT
                ISNULL(SUM(CASE WHEN TrangThaiDon = @HoanTat THEN TongThanhToan ELSE 0 END), 0) AS TongDoanhThu,
                ISNULL(SUM(CASE WHEN TrangThaiDon = @HoanTat AND PhuongThucThanhToan = 'COD' THEN TongThanhToan ELSE 0 END), 0) AS TienMat,
                ISNULL(SUM(CASE WHEN TrangThaiDon = @HoanTat AND PhuongThucThanhToan <> 'COD' THEN TongThanhToan ELSE 0 END), 0) AS ChuyenKhoan,
                ISNULL(SUM(CASE WHEN TrangThaiDon = @HoanTat THEN 1 ELSE 0 END), 0) AS SoDonHoanTat,
                ISNULL(SUM(CASE WHEN TrangThaiDon = @DaHuy THEN 1 ELSE 0 END), 0) AS SoDonThatBai
            FROM DonHang
            WHERE MaChiNhanh = @MaChiNhanh AND TrangThaiDon IN (@HoanTat, @DaHuy)";
        if (tuNgay is not null) sql += " AND NgayDat >= @TuNgay";
        if (denNgayExclusive is not null) sql += " AND NgayDat < @DenNgay";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
        cmd.Parameters.AddWithValue("@HoanTat", TrangThaiDonHoanTat);
        cmd.Parameters.AddWithValue("@DaHuy", TrangThaiDonOptions.DaHuy);
        if (tuNgay is not null) cmd.Parameters.AddWithValue("@TuNgay", tuNgay.Value);
        if (denNgayExclusive is not null) cmd.Parameters.AddWithValue("@DenNgay", denNgayExclusive.Value);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return new DoanhThuSummary();

        return new DoanhThuSummary
        {
            TongDoanhThu = reader.GetDecimal(0),
            TongDoanhThuTienMat = reader.GetDecimal(1),
            TongDoanhThuChuyenKhoan = reader.GetDecimal(2),
            SoDonHoanTat = reader.GetInt32(3),
            SoDonThatBai = reader.GetInt32(4)
        };
    }

    // tuNgay/denNgay: cả hai đều là mốc ngày (không giờ), bao gồm cả hai đầu mút.
    public async Task<List<DoanhThuNgay>> GetDoanhThuTheoNgayAsync(int maChiNhanh, DateTime tuNgay, DateTime denNgay)
    {
        tuNgay = tuNgay.Date;
        denNgay = denNgay.Date;
        var denNgayExclusive = denNgay.AddDays(1);

        const string sql = @"
            SELECT CAST(NgayDat AS DATE) AS Ngay, SUM(TongThanhToan) AS DoanhThu
            FROM DonHang
            WHERE MaChiNhanh = @MaChiNhanh AND TrangThaiDon = @TrangThai AND NgayDat >= @TuNgay AND NgayDat < @DenNgay
            GROUP BY CAST(NgayDat AS DATE)";

        var theoNgay = new Dictionary<DateTime, decimal>();
        await using (var conn = CreateConnection())
        await using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
            cmd.Parameters.AddWithValue("@TrangThai", TrangThaiDonHoanTat);
            cmd.Parameters.AddWithValue("@TuNgay", tuNgay);
            cmd.Parameters.AddWithValue("@DenNgay", denNgayExclusive);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                theoNgay[reader.GetDateTime(0)] = reader.GetDecimal(1);
            }
        }

        var soNgay = (int)(denNgay - tuNgay).TotalDays + 1;
        var ketQua = new List<DoanhThuNgay>();
        for (var i = 0; i < soNgay; i++)
        {
            var ngay = tuNgay.AddDays(i);
            ketQua.Add(new DoanhThuNgay { Ngay = ngay, DoanhThu = theoNgay.GetValueOrDefault(ngay, 0) });
        }
        return ketQua;
    }

    // Ngày của đơn hoàn tất sớm nhất tại chi nhánh - dùng làm mốc bắt đầu biểu đồ khi chọn "Tất cả".
    public async Task<DateTime?> GetNgayDonHoanTatSomNhatAsync(int maChiNhanh)
    {
        const string sql = @"
            SELECT MIN(CAST(NgayDat AS DATE))
            FROM DonHang
            WHERE MaChiNhanh = @MaChiNhanh AND TrangThaiDon = @TrangThai";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
        cmd.Parameters.AddWithValue("@TrangThai", TrangThaiDonHoanTat);

        await conn.OpenAsync();
        var result = await cmd.ExecuteScalarAsync();
        return result is DateTime dt ? dt : null;
    }

    public async Task<List<DonHangGanDay>> GetDonHangGanDayAsync(int maChiNhanh, int soLuong, string? trangThaiLoc = null)
    {
        // Doanh thu chỉ liệt kê đơn đã có kết quả cuối (giao hoàn tất hoặc giao thất bại/đã hủy).
        // Đơn còn đang xử lý (Chờ xử lý, Đang giao) được quản lý bên phân hệ Đơn hàng, không hiện ở đây.
        var sql = @"
            SELECT TOP (@SoLuong) MaDonHang, NgayDat, TongThanhToan, TrangThaiDon, PhuongThucThanhToan
            FROM DonHang
            WHERE MaChiNhanh = @MaChiNhanh AND TrangThaiDon IN (@HoanTat, @DaHuy)";
        if (!string.IsNullOrWhiteSpace(trangThaiLoc)) sql += " AND TrangThaiDon = @TrangThaiLoc";
        sql += " ORDER BY NgayDat DESC";

        var list = new List<DonHangGanDay>();
        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
        cmd.Parameters.AddWithValue("@SoLuong", soLuong);
        cmd.Parameters.AddWithValue("@HoanTat", TrangThaiDonOptions.HoanTat);
        cmd.Parameters.AddWithValue("@DaHuy", TrangThaiDonOptions.DaHuy);
        if (!string.IsNullOrWhiteSpace(trangThaiLoc)) cmd.Parameters.AddWithValue("@TrangThaiLoc", trangThaiLoc);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new DonHangGanDay
            {
                MaDonHang = reader.GetInt32(0),
                NgayDat = reader.GetDateTime(1),
                TongThanhToan = reader.GetDecimal(2),
                TrangThaiDon = reader.GetString(3),
                PhuongThucThanhToan = reader.GetString(4)
            });
        }
        return list;
    }

    private static string PhuongThucLabel(string raw) => raw switch
    {
        "COD" => "Tiền mặt",
        _ => "Chuyển khoản"
    };

    public async Task<List<DonHangListItem>> GetDonHangTheoChiNhanhAsync(int maChiNhanh, string? trangThaiLoc)
    {
        var sql = @"
            SELECT dh.MaDonHang, dh.NgayDat, kh.HoTen, dh.TongThanhToan, dh.TrangThaiDon, sp.HoTen, dh.PhuongThucThanhToan
            FROM DonHang dh
            JOIN TaiKhoan kh ON kh.MaTaiKhoan = dh.MaKhachHang
            LEFT JOIN TaiKhoan sp ON sp.MaTaiKhoan = dh.MaShipper
            WHERE dh.MaChiNhanh = @MaChiNhanh";
        if (!string.IsNullOrWhiteSpace(trangThaiLoc)) sql += " AND dh.TrangThaiDon = @TrangThai";
        sql += " ORDER BY dh.NgayDat DESC";

        var list = new List<DonHangListItem>();
        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
        if (!string.IsNullOrWhiteSpace(trangThaiLoc)) cmd.Parameters.AddWithValue("@TrangThai", trangThaiLoc);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new DonHangListItem
            {
                MaDonHang = reader.GetInt32(0),
                NgayDat = reader.GetDateTime(1),
                TenKhachHang = reader.GetString(2),
                TongThanhToan = reader.GetDecimal(3),
                TrangThaiDon = reader.GetString(4),
                TenShipper = reader.IsDBNull(5) ? null : reader.GetString(5),
                PhuongThucLabel = PhuongThucLabel(reader.GetString(6))
            });
        }
        return list;
    }

    public async Task<DonHangChiTiet?> GetChiTietDonHangAsync(int maDonHang, int maChiNhanh)
    {
        const string sql = @"
            SELECT dh.MaDonHang, dh.NgayDat, dh.TrangThaiDon, kh.HoTen AS TenKhachHang,
                   dh.DiaChiGiaoHang, dh.SoDienThoaiNhan, dh.PhuongThucThanhToan, dh.HinhAnhXacNhanGiao, dh.LyDoThatBaiHoacHuy,
                   dh.DanhGiaSao, dh.PhanHoiKhachHang,
                   dh.TongTienHang, dh.PhiShip, dh.TienGiamVoucher, dh.TongThanhToan, dh.DaDoiSoatCOD,
                   sp.HoTen, sp.SoDienThoai, sp.BienSoXe, sp.CCCD, sp.SanSangNhanDon, sp.DiemTichLuy
            FROM DonHang dh
            JOIN TaiKhoan kh ON kh.MaTaiKhoan = dh.MaKhachHang
            LEFT JOIN TaiKhoan sp ON sp.MaTaiKhoan = dh.MaShipper
            WHERE dh.MaDonHang = @MaDonHang AND dh.MaChiNhanh = @MaChiNhanh";

        DonHangChiTiet? donHang;
        await using (var conn = CreateConnection())
        await using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@MaDonHang", maDonHang);
            cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            donHang = new DonHangChiTiet
            {
                MaDonHang = reader.GetInt32(0),
                NgayDat = reader.GetDateTime(1),
                TrangThaiDon = reader.GetString(2),
                TenKhachHang = reader.GetString(3),
                DiaChiGiaoHang = reader.GetString(4),
                SoDienThoaiNhan = reader.GetString(5),
                PhuongThucThanhToan = reader.GetString(6),
                HinhAnhXacNhanGiao = reader.IsDBNull(7) ? null : reader.GetString(7),
                LyDoThatBaiHoacHuy = reader.IsDBNull(8) ? null : reader.GetString(8),
                DanhGiaSao = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                PhanHoiKhachHang = reader.IsDBNull(10) ? null : reader.GetString(10),
                TongTienHang = reader.GetDecimal(11),
                PhiShip = reader.GetDecimal(12),
                TienGiamVoucher = reader.GetDecimal(13),
                TongThanhToan = reader.GetDecimal(14),
                DaDoiSoatCOD = reader.GetBoolean(15),
                TenShipper = reader.IsDBNull(16) ? null : reader.GetString(16),
                SdtShipper = reader.IsDBNull(17) ? null : reader.GetString(17),
                BienSoXeShipper = reader.IsDBNull(18) ? null : reader.GetString(18),
                CccdShipper = reader.IsDBNull(19) ? null : reader.GetString(19),
                SanSangNhanDonShipper = reader.IsDBNull(20) ? null : reader.GetBoolean(20),
                DiemTichLuyShipper = reader.IsDBNull(21) ? null : reader.GetInt32(21)
            };
        }

        const string sqlChiTiet = @"
            SELECT tp.TenThucPham, ct.SoLuong, ct.DonGiaLucMua
            FROM ChiTietDonHang ct
            JOIN ThucPham tp ON tp.MaThucPham = ct.MaThucPham
            WHERE ct.MaDonHang = @MaDonHang";

        await using (var conn = CreateConnection())
        await using (var cmd = new SqlCommand(sqlChiTiet, conn))
        {
            cmd.Parameters.AddWithValue("@MaDonHang", maDonHang);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                donHang.SanPham.Add(new SanPhamTrongDon
                {
                    TenThucPham = reader.GetString(0),
                    SoLuong = reader.GetInt32(1),
                    DonGiaLucMua = reader.GetDecimal(2)
                });
            }
        }

        return donHang;
    }

    // Danh sách người giao hàng thuộc chi nhánh, ưu tiên hiển thị người đang sẵn sàng nhận đơn lên trước.
    public async Task<List<ShipperOption>> GetShipperTheoChiNhanhAsync(int maChiNhanh)
    {
        const string sql = @"
            SELECT MaTaiKhoan, HoTen, SoDienThoai, SanSangNhanDon
            FROM TaiKhoan
            WHERE VaiTro = 'Shipper' AND MaChiNhanh = @MaChiNhanh AND TrangThaiKhoa = 0
            ORDER BY SanSangNhanDon DESC, HoTen";

        var list = new List<ShipperOption>();
        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new ShipperOption
            {
                MaTaiKhoan = reader.GetInt32(0),
                HoTen = reader.GetString(1),
                SoDienThoai = reader.GetString(2),
                SanSangNhanDon = reader.GetBoolean(3)
            });
        }
        return list;
    }

    // Toàn bộ hồ sơ shipper thuộc chi nhánh (kể cả đã khóa), hiển thị ở trang Thông tin chi nhánh.
    public async Task<List<NhanVienGiaoHang>> GetNhanVienGiaoHangAsync(int maChiNhanh)
    {
        const string sql = @"
            SELECT MaTaiKhoan, HoTen, SoDienThoai, BienSoXe, CCCD
            FROM TaiKhoan
            WHERE VaiTro = 'Shipper' AND MaChiNhanh = @MaChiNhanh
            ORDER BY TrangThaiKhoa, SanSangNhanDon DESC, HoTen";

        var list = new List<NhanVienGiaoHang>();
        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new NhanVienGiaoHang
            {
                MaTaiKhoan = reader.GetInt32(0),
                HoTen = reader.GetString(1),
                SoDienThoai = reader.GetString(2),
                BienSoXe = reader.IsDBNull(3) ? null : reader.GetString(3),
                CCCD = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }
        return list;
    }

    // Nhân viên đóng gói/thanh toán tại quầy (VaiTro = NhanVien) thuộc chi nhánh, khác với Shipper (giao hàng).
    public async Task<List<NhanVienDongGoi>> GetNhanVienDongGoiAsync(int maChiNhanh)
    {
        const string sql = @"
            SELECT MaTaiKhoan, HoTen, Email, SoDienThoai, CCCD
            FROM TaiKhoan
            WHERE VaiTro = 'NhanVien' AND MaChiNhanh = @MaChiNhanh
            ORDER BY HoTen";

        var list = new List<NhanVienDongGoi>();
        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new NhanVienDongGoi
            {
                MaTaiKhoan = reader.GetInt32(0),
                HoTen = reader.GetString(1),
                Email = reader.GetString(2),
                SoDienThoai = reader.GetString(3),
                CCCD = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }
        return list;
    }

    // Tài khoản Quản lý (VaiTro = Manager) đang phụ trách chi nhánh.
    public async Task<QuanLyChiNhanh?> GetQuanLyChiNhanhAsync(int maChiNhanh)
    {
        const string sql = @"
            SELECT TOP (1) MaTaiKhoan, HoTen, Email, SoDienThoai, CCCD
            FROM TaiKhoan
            WHERE VaiTro = 'Manager' AND MaChiNhanh = @MaChiNhanh
            ORDER BY MaTaiKhoan";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new QuanLyChiNhanh
        {
            MaTaiKhoan = reader.GetInt32(0),
            HoTen = reader.GetString(1),
            Email = reader.GetString(2),
            SoDienThoai = reader.GetString(3),
            CCCD = reader.IsDBNull(4) ? null : reader.GetString(4)
        };
    }

    // Xử lý đơn: gán người giao hàng và chuyển sang "Đang giao". Chỉ áp dụng được khi đơn còn ở "Chờ xử lý"
    // (điều kiện trong WHERE), tránh xử lý trùng một đơn đã được giao trước đó.
    public async Task<bool> XuLyDonHangAsync(int maDonHang, int maChiNhanh, int maShipper)
    {
        const string sql = @"
            UPDATE DonHang
            SET MaShipper = @MaShipper, TrangThaiDon = @TrangThaiMoi
            WHERE MaDonHang = @MaDonHang AND MaChiNhanh = @MaChiNhanh AND TrangThaiDon = @TrangThaiCu";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaShipper", maShipper);
        cmd.Parameters.AddWithValue("@TrangThaiMoi", TrangThaiDonOptions.DangGiao);
        cmd.Parameters.AddWithValue("@MaDonHang", maDonHang);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
        cmd.Parameters.AddWithValue("@TrangThaiCu", TrangThaiDonOptions.ChoXuLy);

        await conn.OpenAsync();
        var soDongAnhHuong = await cmd.ExecuteNonQueryAsync();
        return soDongAnhHuong > 0;
    }

    // Yêu cầu nhập hàng của chi nhánh luôn có tối đa 1 bản "Nháp" đang soạn tại một thời điểm; lấy ra, hoặc tạo mới nếu chưa có.
    private async Task<int> LayHoacTaoYeuCauDangSoanAsync(int maChiNhanh)
    {
        const string sqlTim = "SELECT MaYeuCau FROM YeuCauNhapHang WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = N'Nháp'";
        await using (var conn = CreateConnection())
        await using (var cmd = new SqlCommand(sqlTim, conn))
        {
            cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
            await conn.OpenAsync();
            var ketQua = await cmd.ExecuteScalarAsync();
            if (ketQua is int maYeuCauCo) return maYeuCauCo;
        }

        const string sqlTao = @"
            INSERT INTO YeuCauNhapHang (MaChiNhanh, TrangThai) VALUES (@MaChiNhanh, N'Nháp');
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connTao = CreateConnection();
        await using var cmdTao = new SqlCommand(sqlTao, connTao);
        cmdTao.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
        await connTao.OpenAsync();
        return (int)(await cmdTao.ExecuteScalarAsync())!;
    }

    // Danh sách các dòng đang soạn (bản Nháp) của chi nhánh, hiển thị ở khung bên dưới trang Yêu cầu hàng.
    public async Task<List<YeuCauNhapHangDong>> GetChiTietYeuCauDangSoanAsync(int maChiNhanh)
    {
        const string sql = @"
            SELECT ct.MaChiTiet, ct.MaThucPham, tp.TenThucPham, tp.DonViTinh, ct.SoLuongCan
            FROM ChiTietYeuCauNhapHang ct
            JOIN YeuCauNhapHang yc ON yc.MaYeuCau = ct.MaYeuCau
            JOIN ThucPham tp ON tp.MaThucPham = ct.MaThucPham
            WHERE yc.MaChiNhanh = @MaChiNhanh AND yc.TrangThai = N'Nháp'
            ORDER BY tp.TenThucPham";

        var list = new List<YeuCauNhapHangDong>();
        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new YeuCauNhapHangDong
            {
                MaChiTiet = reader.GetInt32(0),
                MaThucPham = reader.GetInt32(1),
                TenThucPham = reader.GetString(2),
                DonViTinh = reader.GetString(3),
                SoLuongCan = reader.GetInt32(4)
            });
        }
        return list;
    }

    // Thêm (hoặc cập nhật số lượng nếu món đó đã có trong bản Nháp) một dòng vào yêu cầu nhập hàng đang soạn.
    public async Task ThemVaoYeuCauNhapHangAsync(int maChiNhanh, int maThucPham, int soLuong)
    {
        var maYeuCau = await LayHoacTaoYeuCauDangSoanAsync(maChiNhanh);

        const string sql = @"
            UPDATE ChiTietYeuCauNhapHang SET SoLuongCan = @SoLuong
            WHERE MaYeuCau = @MaYeuCau AND MaThucPham = @MaThucPham;

            IF @@ROWCOUNT = 0
                INSERT INTO ChiTietYeuCauNhapHang (MaYeuCau, MaThucPham, SoLuongCan)
                VALUES (@MaYeuCau, @MaThucPham, @SoLuong);";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaYeuCau", maYeuCau);
        cmd.Parameters.AddWithValue("@MaThucPham", maThucPham);
        cmd.Parameters.AddWithValue("@SoLuong", soLuong);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    // Bỏ 1 món khỏi yêu cầu đang soạn (chỉ xóa được khi vẫn còn ở trạng thái Nháp và đúng chi nhánh).
    public async Task XoaDongYeuCauNhapHangAsync(int maChiTiet, int maChiNhanh)
    {
        const string sql = @"
            DELETE ct FROM ChiTietYeuCauNhapHang ct
            JOIN YeuCauNhapHang yc ON yc.MaYeuCau = ct.MaYeuCau
            WHERE ct.MaChiTiet = @MaChiTiet AND yc.MaChiNhanh = @MaChiNhanh AND yc.TrangThai = N'Nháp'";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaChiTiet", maChiTiet);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    // Gửi yêu cầu đang soạn lên kho tổng (chuyển Nháp -> Chờ duyệt). Không làm gì nếu chưa có món nào trong yêu cầu.
    public async Task<bool> GuiYeuCauNhapHangAsync(int maChiNhanh)
    {
        const string sql = @"
            UPDATE YeuCauNhapHang
            SET TrangThai = N'Chờ duyệt', NgayGui = GETDATE()
            WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = N'Nháp'
                  AND EXISTS (SELECT 1 FROM ChiTietYeuCauNhapHang ct WHERE ct.MaYeuCau = YeuCauNhapHang.MaYeuCau)";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);

        await conn.OpenAsync();
        var soDong = await cmd.ExecuteNonQueryAsync();
        return soDong > 0;
    }
}
