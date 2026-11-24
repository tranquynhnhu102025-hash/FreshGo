using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace FreshGo.Pages.Shipper
{
    public class ShipperProfile
    {
        public int MaTaiKhoan { get; set; } = 3;
        public string HoTen { get; set; } = "Gia Kiệt";
        public string SoDienThoai { get; set; } = "0903333333";
        public string BienSoXe { get; set; } = "59-G1 123.45";
        public int MaChiNhanh { get; set; } = 1;
        public string TenChiNhanh { get; set; } = "FreshGo Quận 12";
        public string DiaChiChiNhanh { get; set; } = "331 Quốc lộ 1A, Quận 12";
        public bool SanSangNhanDon { get; set; } = true;
    }

    public class DonHangShipItem
    {
        public int MaDonHang { get; set; }
        public string NguonDb { get; set; } = "CUSTOMER";
        public string TenKhachHang { get; set; } = string.Empty;
        public string SoDienThoaiNhan { get; set; } = string.Empty;
        public string DiaChiGiaoHang { get; set; } = string.Empty;
        public decimal TongTienHang { get; set; }
        public decimal PhiShip { get; set; }
        public decimal TongThanhToan { get; set; }
        public string PhuongThucThanhToan { get; set; } = "COD";
        public string TrangThaiDon { get; set; } = string.Empty;
        public string? LyDoThatBaiHoacHuy { get; set; }
        public bool DaDoiSoatCOD { get; set; }
        public int? DanhGiaSao { get; set; }
        public string? PhanHoiKhachHang { get; set; }
        public DateTime NgayDat { get; set; }
        public List<string> DanhSachMon { get; set; } = new();
    }

    public class IndexModel : PageModel
    {
        private readonly string _mainConnStr;
        private readonly string _customerConnStr;
        private const int DefaultShipperId = 3;

        public IndexModel(IConfiguration config)
        {
            _mainConnStr = config.GetConnectionString("DefaultConnection")
                ?? @"Server=localhost\SQLEXPRESS;Database=FreshGoDB;Trusted_Connection=True;TrustServerCertificate=True;";
            _customerConnStr = config.GetConnectionString("CustomerConnection")
                ?? @"Server=localhost\SQLEXPRESS;Database=FreshGoCustomerDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public ShipperProfile Shipper { get; set; } = new();
        public List<DonHangShipItem> DonDangGiao { get; set; } = new();
        public List<DonHangShipItem> DonChoNhan { get; set; } = new();
        public List<DonHangShipItem> LichSuDon { get; set; } = new();
        public decimal TongTienCODChuaNop { get; set; }
        public int SoMonTrongGioKhach { get; set; }
        public List<string> ChiTietGioKhach { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Tab { get; set; } = "cho-nhan";

        [TempData]
        public string? ThongBao { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostToggleTrangThaiAsync()
        {
            try
            {
                using var conn = new SqlConnection(_mainConnStr);
                await conn.OpenAsync();
                var cmd = new SqlCommand(
                    "UPDATE TaiKhoan SET SanSangNhanDon = CASE WHEN SanSangNhanDon = 1 THEN 0 ELSE 1 END WHERE MaTaiKhoan = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", DefaultShipperId);
                await cmd.ExecuteNonQueryAsync();
                ThongBao = "Đã cập nhật trạng thái hoạt động của Shipper.";
            }
            catch { }
            return RedirectToPage(new { tab = Tab });
        }

        public IActionResult OnPostTaoDonTuGioHangAsync()
        {
            return RedirectToPage("/Customer/GioHang/ThanhToan");
        }

        public async Task<IActionResult> OnPostNhanDonAsync(int maDonHang, string nguonDb)
        {
            if (nguonDb == "CUSTOMER")
            {
                await UpdateCustomerOrderStatusAsync(maDonHang, "Đang giao");
                ThongBao = $"✅ Đã nhận đơn khách #KH{maDonHang:D3}! Trạng thái bên Khách hàng đã đổi sang 'Đang giao'.";
            }
            else
            {
                using var conn = new SqlConnection(_mainConnStr);
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    UPDATE DonHang 
                    SET MaShipper = @ShipperId, TrangThaiDon = N'Đang giao' 
                    WHERE MaDonHang = @MaDon", conn);
                cmd.Parameters.AddWithValue("@ShipperId", DefaultShipperId);
                cmd.Parameters.AddWithValue("@MaDon", maDonHang);
                await cmd.ExecuteNonQueryAsync();
                ThongBao = $"✅ Đã nhận đơn kho #DH{maDonHang:D3}!";
            }

            return RedirectToPage(new { tab = "dang-giao" });
        }

        public async Task<IActionResult> OnPostHoanTatDonAsync(int maDonHang, string nguonDb)
        {
            if (nguonDb == "CUSTOMER")
            {
                await UpdateCustomerOrderStatusAsync(maDonHang, "Đã giao");
                ThongBao = $"🎉 Đã giao xong đơn #KH{maDonHang:D3}! Bên trang Khách hàng đã cập nhật 'Đã giao'.";
            }
            else
            {
                using var conn = new SqlConnection(_mainConnStr);
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    UPDATE DonHang 
                    SET TrangThaiDon = N'Hoàn tất', HinhAnhXacNhanGiao = N'Đã giao tận tay khách hàng'
                    WHERE MaDonHang = @MaDon", conn);
                cmd.Parameters.AddWithValue("@MaDon", maDonHang);
                await cmd.ExecuteNonQueryAsync();
                ThongBao = $"🎉 Đã hoàn tất giao đơn #DH{maDonHang:D3}!";
            }

            return RedirectToPage(new { tab = "dang-giao" });
        }

        public async Task<IActionResult> OnPostThatBaiDonAsync(int maDonHang, string nguonDb, string lyDo)
        {
            var lyDoLuu = string.IsNullOrWhiteSpace(lyDo) ? "Khách hàng không nghe máy." : lyDo.Trim();
            if (nguonDb == "CUSTOMER")
            {
                await UpdateCustomerOrderStatusAsync(maDonHang, "Đã hủy");
                ThongBao = $"❌ Đã hủy đơn #KH{maDonHang:D3} ({lyDoLuu}) và đồng bộ về trang Khách hàng.";
            }
            else
            {
                using var conn = new SqlConnection(_mainConnStr);
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    UPDATE DonHang 
                    SET TrangThaiDon = N'Đã hủy', LyDoThatBaiHoacHuy = @LyDo
                    WHERE MaDonHang = @MaDon", conn);
                cmd.Parameters.AddWithValue("@MaDon", maDonHang);
                cmd.Parameters.AddWithValue("@LyDo", lyDoLuu);
                await cmd.ExecuteNonQueryAsync();
                ThongBao = $"❌ Đã cập nhật đơn #DH{maDonHang:D3} sang Giao thất bại.";
            }

            return RedirectToPage(new { tab = "dang-giao" });
        }

        public async Task<IActionResult> OnPostDoiSoatCodAsync()
        {
            using var conn = new SqlConnection(_mainConnStr);
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"
                UPDATE DonHang SET DaDoiSoatCOD = 1 
                WHERE MaShipper = @ShipperId AND TrangThaiDon = N'Hoàn tất' AND PhuongThucThanhToan = N'COD' AND DaDoiSoatCOD = 0", conn);
            cmd.Parameters.AddWithValue("@ShipperId", DefaultShipperId);
            await cmd.ExecuteNonQueryAsync();

            ThongBao = "💰 Đã xác nhận đối soát tiền COD về chi nhánh thành công!";
            return RedirectToPage(new { tab = "lich-su" });
        }

        private async Task UpdateCustomerOrderStatusAsync(int orderId, string newStatus)
        {
            using var conn = new SqlConnection(_customerConnStr);
            await conn.OpenAsync();
            string? orderTable = await FindTableAsync(conn, "DonHang", "DonHangs");
            if (orderTable == null) return;

            var cols = await GetColumnsAsync(conn, orderTable);
            string idCol = cols.Values.First();
            string statusCol = PickCol(cols, "trangthai", "trangthaidon");

            var cmd = new SqlCommand($"UPDATE [{orderTable}] SET [{statusCol}] = @St WHERE [{idCol}] = @Id", conn);
            cmd.Parameters.AddWithValue("@St", newStatus);
            cmd.Parameters.AddWithValue("@Id", orderId);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using var connCus = new SqlConnection(_customerConnStr);
                await connCus.OpenAsync();

                string? cartTable = await FindTableAsync(connCus, "GioHang", "GioHangs");
                string? prodTable = await FindTableAsync(connCus, "SanPham", "SanPhams");
                string? orderTable = await FindTableAsync(connCus, "DonHang", "DonHangs");
                string? detailTable = await FindTableAsync(connCus, "ChiTietDonHang", "ChiTietDonHangs");
                string? cusTable = await FindTableAsync(connCus, "KhachHang", "KhachHangs");

                if (cartTable != null && prodTable != null)
                {
                    var cCols = await GetColumnsAsync(connCus, cartTable);
                    var pCols = await GetColumnsAsync(connCus, prodTable);

                    string cProdFk = cCols.Values.FirstOrDefault(c => c.Contains("SanPham", StringComparison.OrdinalIgnoreCase)) ?? "ID_SanPham";
                    string cQty = cCols.Values.FirstOrDefault(c => c.Contains("SoLuong", StringComparison.OrdinalIgnoreCase)) ?? "SoLuong";
                    string pPk = pCols.Values.First();
                    string pName = pCols.Values.FirstOrDefault(c => c.Contains("Ten", StringComparison.OrdinalIgnoreCase)) ?? "TenSanPham";

                    var cmdCart = new SqlCommand($@"
                        SELECT p.[{pName}], g.[{cQty}]
                        FROM [{cartTable}] g
                        JOIN [{prodTable}] p ON g.[{cProdFk}] = p.[{pPk}]", connCus);
                    using var rC = await cmdCart.ExecuteReaderAsync();
                    while (await rC.ReadAsync())
                    {
                        int qty = Convert.ToInt32(rC.GetValue(1));
                        SoMonTrongGioKhach += qty;
                        ChiTietGioKhach.Add($"{rC.GetString(0)} (x{qty})");
                    }
                }

                var cusDetailMap = new Dictionary<int, List<string>>();
                if (detailTable != null && prodTable != null)
                {
                    var dCols = await GetColumnsAsync(connCus, detailTable);
                    var pCols = await GetColumnsAsync(connCus, prodTable);

                    string fkOrder = dCols.Values.FirstOrDefault(c => c.Contains("DonHang", StringComparison.OrdinalIgnoreCase)) ?? "ID_DonHang";
                    string fkProd = dCols.Values.FirstOrDefault(c => c.Contains("SanPham", StringComparison.OrdinalIgnoreCase)) ?? "ID_SanPham";
                    string dQty = dCols.Values.FirstOrDefault(c => c.Contains("SoLuong", StringComparison.OrdinalIgnoreCase)) ?? "SoLuong";

                    string pPk = pCols.Values.First();
                    string pName = pCols.Values.FirstOrDefault(c => c.Contains("Ten", StringComparison.OrdinalIgnoreCase)) ?? "TenSanPham";
                    string pDvt = pCols.ContainsKey("donvitinh") ? $"ISNULL(p.[{pCols["donvitinh"]}], N'Kg')" : "N'Kg'";

                    var cmdCd = new SqlCommand($@"
                        SELECT d.[{fkOrder}], p.[{pName}], d.[{dQty}], {pDvt}
                        FROM [{detailTable}] d
                        JOIN [{prodTable}] p ON d.[{fkProd}] = p.[{pPk}]", connCus);
                    using var rCd = await cmdCd.ExecuteReaderAsync();
                    while (await rCd.ReadAsync())
                    {
                        int oid = Convert.ToInt32(rCd.GetValue(0));
                        if (!cusDetailMap.ContainsKey(oid)) cusDetailMap[oid] = new List<string>();
                        cusDetailMap[oid].Add($"{rCd.GetString(1)} (x{Convert.ToInt32(rCd.GetValue(2))} {rCd.GetString(3)})");
                    }
                }

                string defaultCusName = "Khách Trần B";
                string defaultCusPhone = "0909 888 777";
                if (cusTable != null)
                {
                    var cmdCus = new SqlCommand($"SELECT TOP 1 * FROM [{cusTable}] ORDER BY 1 ASC", connCus);
                    using var rCus = await cmdCus.ExecuteReaderAsync();
                    if (await rCus.ReadAsync())
                    {
                        var cRow = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        for (int i = 0; i < rCus.FieldCount; i++)
                            cRow[rCus.GetName(i)] = rCus.IsDBNull(i) ? null! : rCus.GetValue(i);

                        defaultCusName = GetVal(cRow, "HoTen", "TenKhachHang", "Ten")?.ToString() ?? defaultCusName;
                        defaultCusPhone = GetVal(cRow, "SoDienThoai", "SDT", "DienThoai")?.ToString() ?? defaultCusPhone;
                    }
                }

                if (orderTable != null)
                {
                    var cmdOrders = new SqlCommand($"SELECT * FROM [{orderTable}] ORDER BY 1 DESC", connCus);
                    using var rO = await cmdOrders.ExecuteReaderAsync();
                    while (await rO.ReadAsync())
                    {
                        var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        for (int i = 0; i < rO.FieldCount; i++)
                        {
                            row[rO.GetName(i)] = rO.IsDBNull(i) ? null! : rO.GetValue(i);
                        }

                        int id = Convert.ToInt32(GetVal(row, "ID_DonHang", "Id", "MaDonHang", "MaDon") ?? 0);
                        string st = GetVal(row, "TrangThai", "TrangThaiDon")?.ToString() ?? "Chờ duyệt";
                        decimal tong = Convert.ToDecimal(GetVal(row, "TongThanhToan", "TongTien") ?? 0m);
                        decimal shipFee = Convert.ToDecimal(GetVal(row, "PhiShip", "PhiGiaoHang") ?? 0m);
                        string diaChi = GetVal(row, "DiaChiGiao", "DiaChiGiaoHang", "DiaChi")?.ToString() ?? "Quận 12, TP. Hồ Chí Minh";
                        string tenNhan = GetVal(row, "TenNguoiNhan", "HoTenNhan", "HoTen", "TenKhachHang")?.ToString() ?? "";
                        string sdtNhan = GetVal(row, "SoDienThoai", "SoDienThoaiNhan", "SDT")?.ToString() ?? "";

                        if (string.IsNullOrWhiteSpace(tenNhan) && diaChi.Contains(" - SĐT:"))
                        {
                            var parts = diaChi.Split('|', 2);
                            var head = parts[0].Split(" - SĐT:");
                            tenNhan = head[0].Trim();
                            if (head.Length > 1) sdtNhan = head[1].Trim();
                            if (parts.Length > 1) diaChi = parts[1].Trim();
                        }

                        var item = new DonHangShipItem
                        {
                            MaDonHang = id,
                            NguonDb = "CUSTOMER",
                            TenKhachHang = string.IsNullOrWhiteSpace(tenNhan) ? defaultCusName : tenNhan,
                            SoDienThoaiNhan = string.IsNullOrWhiteSpace(sdtNhan) ? defaultCusPhone : sdtNhan,
                            DiaChiGiaoHang = diaChi,
                            TongTienHang = tong,
                            PhiShip = shipFee,
                            TongThanhToan = tong,
                            PhuongThucThanhToan = GetVal(row, "PhuongThucThanhToan")?.ToString() ?? "COD",
                            TrangThaiDon = st,
                            NgayDat = GetVal(row, "NgayDat", "NgayTao") is DateTime dt ? dt : DateTime.Now,
                            DanhSachMon = cusDetailMap.ContainsKey(id) ? cusDetailMap[id] : new List<string>()
                        };

                        bool isNew = st.Contains("Chờ") || st.Contains("Mới") || st == "Đang xử lý";
                        PhanLoaiDon(item, isNew);
                    }
                }
            }
            catch { }

            try
            {
                using var connMain = new SqlConnection(_mainConnStr);
                await connMain.OpenAsync();

                var cmdShipper = new SqlCommand(@"
                    SELECT t.MaTaiKhoan, t.HoTen, t.SoDienThoai, ISNULL(t.BienSoXe, N'59-G1 123.45'),
                           ISNULL(t.MaChiNhanh, 1), ISNULL(c.TenChiNhanh, N'FreshGo Quận 12'),
                           ISNULL(c.DiaChi, N'331 Quốc lộ 1A, Quận 12'), t.SanSangNhanDon
                    FROM TaiKhoan t
                    LEFT JOIN ChiNhanh c ON t.MaChiNhanh = c.MaChiNhanh
                    WHERE t.MaTaiKhoan = @Id", connMain);
                cmdShipper.Parameters.AddWithValue("@Id", DefaultShipperId);

                using (var r = await cmdShipper.ExecuteReaderAsync())
                {
                    if (await r.ReadAsync())
                    {
                        Shipper = new ShipperProfile
                        {
                            MaTaiKhoan = r.GetInt32(0),
                            HoTen = r.GetString(1),
                            SoDienThoai = r.GetString(2),
                            BienSoXe = r.GetString(3),
                            MaChiNhanh = r.GetInt32(4),
                            TenChiNhanh = r.GetString(5),
                            DiaChiChiNhanh = r.GetString(6),
                            SanSangNhanDon = r.GetBoolean(7)
                        };
                    }
                }

                var chiTietMap = new Dictionary<int, List<string>>();
                var cmdCt = new SqlCommand(@"
                    SELECT ct.MaDonHang, tp.TenThucPham, ct.SoLuong, tp.DonViTinh
                    FROM ChiTietDonHang ct
                    JOIN ThucPham tp ON ct.MaThucPham = tp.MaThucPham", connMain);
                using (var rCt = await cmdCt.ExecuteReaderAsync())
                {
                    while (await rCt.ReadAsync())
                    {
                        int maDon = rCt.GetInt32(0);
                        if (!chiTietMap.ContainsKey(maDon)) chiTietMap[maDon] = new List<string>();
                        chiTietMap[maDon].Add($"{rCt.GetString(1)} (x{rCt.GetInt32(2)} {rCt.GetString(3)})");
                    }
                }

                var cmdDon = new SqlCommand(@"
                    SELECT d.MaDonHang, ISNULL(kh.HoTen, N'Khách hàng'), d.SoDienThoaiNhan, d.DiaChiGiaoHang,
                           d.TongTienHang, d.PhiShip, d.TongThanhToan, d.PhuongThucThanhToan,
                           d.TrangThaiDon, d.LyDoThatBaiHoacHuy, d.DaDoiSoatCOD, d.DanhGiaSao,
                           d.PhanHoiKhachHang, d.NgayDat, d.MaShipper
                    FROM DonHang d
                    LEFT JOIN TaiKhoan kh ON d.MaKhachHang = kh.MaTaiKhoan
                    WHERE d.MaShipper = @ShipperId OR (d.MaShipper IS NULL AND d.TrangThaiDon = N'Chờ xử lý')
                    ORDER BY d.NgayDat DESC", connMain);
                cmdDon.Parameters.AddWithValue("@ShipperId", DefaultShipperId);

                using var rDon = await cmdDon.ExecuteReaderAsync();
                while (await rDon.ReadAsync())
                {
                    int maDon = rDon.GetInt32(0);
                    int? maShipper = rDon.IsDBNull(14) ? null : rDon.GetInt32(14);
                    string st = rDon.GetString(8);

                    var item = new DonHangShipItem
                    {
                        MaDonHang = maDon,
                        NguonDb = "MAIN",
                        TenKhachHang = rDon.GetString(1),
                        SoDienThoaiNhan = rDon.GetString(2),
                        DiaChiGiaoHang = rDon.GetString(3),
                        TongTienHang = rDon.GetDecimal(4),
                        PhiShip = rDon.GetDecimal(5),
                        TongThanhToan = rDon.GetDecimal(6),
                        PhuongThucThanhToan = rDon.GetString(7),
                        TrangThaiDon = st,
                        LyDoThatBaiHoacHuy = rDon.IsDBNull(9) ? null : rDon.GetString(9),
                        DaDoiSoatCOD = rDon.GetBoolean(10),
                        DanhGiaSao = rDon.IsDBNull(11) ? null : rDon.GetInt32(11),
                        PhanHoiKhachHang = rDon.IsDBNull(12) ? null : rDon.GetString(12),
                        NgayDat = rDon.GetDateTime(13),
                        DanhSachMon = chiTietMap.ContainsKey(maDon) ? chiTietMap[maDon] : new List<string>()
                    };

                    PhanLoaiDon(item, maShipper == null);
                }
            }
            catch { }
        }

        private void PhanLoaiDon(DonHangShipItem item, bool isWaiting)
        {
            if (isWaiting)
            {
                DonChoNhan.Add(item);
            }
            else if (item.TrangThaiDon.Contains("Đang giao"))
            {
                DonDangGiao.Add(item);
            }
            else
            {
                LichSuDon.Add(item);
                if ((item.TrangThaiDon.Contains("Hoàn") || item.TrangThaiDon.Contains("Đã giao"))
                    && item.PhuongThucThanhToan == "COD" && !item.DaDoiSoatCOD)
                {
                    TongTienCODChuaNop += item.TongThanhToan;
                }
            }
        }

        private static object? GetVal(Dictionary<string, object> row, params string[] keys)
        {
            foreach (var k in keys)
                if (row.TryGetValue(k, out var v) && v != null) return v;
            return null;
        }

        private static string PickCol(Dictionary<string, string> cols, params string[] candidates)
        {
            foreach (var c in candidates)
                if (cols.TryGetValue(c.ToLower(), out var actual)) return actual;
            return cols.Values.First();
        }

        private static async Task<string?> FindTableAsync(SqlConnection conn, params string[] candidates)
        {
            foreach (var name in candidates)
            {
                var cmd = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @T", conn);
                cmd.Parameters.AddWithValue("@T", name);
                if (Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0) > 0) return name;
            }
            return null;
        }

        private static async Task<Dictionary<string, string>> GetColumnsAsync(SqlConnection conn, string tableName)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var cmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @T", conn);
            cmd.Parameters.AddWithValue("@T", tableName);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                var col = r.GetString(0);
                dict[col.ToLower()] = col;
            }
            return dict;
        }
    }
}