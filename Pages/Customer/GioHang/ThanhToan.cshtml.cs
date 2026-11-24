using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace FreshGo.Pages.Customer.GioHang
{
    public class CartCheckoutItem
    {
        public int IdSanPham { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public string HinhAnh { get; set; } = string.Empty;
        public string DonViTinh { get; set; } = "Kg";
        public decimal GiaBan { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => GiaBan * SoLuong;
    }

    public class ThanhToanModel : PageModel
    {
        private readonly string _customerConnStr;

        public ThanhToanModel(IConfiguration config)
        {
            _customerConnStr = config.GetConnectionString("CustomerConnection")
                ?? @"Server=localhost\SQLEXPRESS;Database=FreshGoCustomerDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        [BindProperty]
        public string HoTenNhan { get; set; } = "Khách Trần B";

        [BindProperty]
        public string SoDienThoai { get; set; } = "0909 888 777";

        [BindProperty]
        public string KhuVuc { get; set; } = "Quận 12";

        [BindProperty]
        public string DiaChiChiTiet { get; set; } = "331 Quốc lộ 1A, Phường An Phú Đông";

        [BindProperty]
        public string? GhiChu { get; set; }

        [BindProperty]
        public string PhuongThucThanhToan { get; set; } = "COD";

        public List<CartCheckoutItem> DanhSachSanPham { get; set; } = new();
        public decimal TamTinh { get; set; }
        public decimal PhiGiaoHang { get; set; }
        public decimal TongCong => TamTinh + PhiGiaoHang;

        public int? MaDonVuaDat { get; set; }
        public string? ThongBaoLoi { get; set; }

        public async Task OnGetAsync()
        {
            if (Request.Cookies.TryGetValue("FreshGo_KhuVuc", out var kv) && !string.IsNullOrWhiteSpace(kv))
            {
                KhuVuc = kv;
            }

            await LoadCartAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadCartAsync();

            if (!DanhSachSanPham.Any())
            {
                ThongBaoLoi = "Giỏ hàng của bạn đang trống, vui lòng chọn thêm sản phẩm.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(HoTenNhan) || string.IsNullOrWhiteSpace(SoDienThoai) || string.IsNullOrWhiteSpace(DiaChiChiTiet))
            {
                ThongBaoLoi = "Vui lòng nhập đầy đủ Họ tên, Số điện thoại và Địa chỉ giao hàng.";
                return Page();
            }

            using var conn = new SqlConnection(_customerConnStr);
            await conn.OpenAsync();

            int customerId = await ResolveCustomerIdAsync(conn);
            string? orderTable = await FindTableAsync(conn, "DonHang", "DonHangs");
            string? detailTable = await FindTableAsync(conn, "ChiTietDonHang", "ChiTietDonHangs");
            string? cartTable = await FindTableAsync(conn, "GioHang", "GioHangs");

            if (orderTable == null)
            {
                ThongBaoLoi = "Không tìm thấy bảng Đơn hàng trong cơ sở dữ liệu.";
                return Page();
            }

            var schema = await GetTableSchemaAsync(conn, orderTable);
            string diaChiDayDu = $"{DiaChiChiTiet.Trim()}, {KhuVuc}, TP. Hồ Chí Minh";
            string thongTinLienHe = $"{HoTenNhan.Trim()} - SĐT: {SoDienThoai.Trim()} | {diaChiDayDu}";
            if (!string.IsNullOrWhiteSpace(GhiChu))
            {
                thongTinLienHe += $" (Ghi chú: {GhiChu.Trim()})";
            }

            var insertCols = new List<string>();
            var insertVals = new List<string>();
            var cmdIns = new SqlCommand { Connection = conn };
            int paramIdx = 0;

            void AddIfCol(string colLower, object val)
            {
                if (schema.TryGetValue(colLower, out var info) && !info.IsIdentity)
                {
                    string pName = $"@p{paramIdx++}";
                    insertCols.Add($"[{info.ActualName}]");
                    insertVals.Add(pName);
                    cmdIns.Parameters.AddWithValue(pName, val);
                }
            }

            string maDonCode = $"FG{DateTime.Now:yyyyMMddHHmm}";
            bool hasSeparateRecipientCols = schema.ContainsKey("tennguoinhan") || schema.ContainsKey("hotennhan") || schema.ContainsKey("sodienthoai") || schema.ContainsKey("sodienthoainhan");

            AddIfCol("madon", maDonCode);
            AddIfCol("madoncode", maDonCode);
            AddIfCol("id_khachhang", customerId);
            AddIfCol("idkhachhang", customerId);
            AddIfCol("makhachhang", customerId);
            AddIfCol("khachhangid", customerId);
            AddIfCol("tennguoinhan", HoTenNhan.Trim());
            AddIfCol("hoten", HoTenNhan.Trim());
            AddIfCol("hotennhan", HoTenNhan.Trim());
            AddIfCol("tenkhachhang", HoTenNhan.Trim());
            AddIfCol("sodienthoai", SoDienThoai.Trim());
            AddIfCol("sodienthoainhan", SoDienThoai.Trim());
            AddIfCol("sdt", SoDienThoai.Trim());
            AddIfCol("diachigiao", hasSeparateRecipientCols ? diaChiDayDu : thongTinLienHe);
            AddIfCol("diachigiaohang", hasSeparateRecipientCols ? diaChiDayDu : thongTinLienHe);
            AddIfCol("diachi", hasSeparateRecipientCols ? diaChiDayDu : thongTinLienHe);
            AddIfCol("ghichu", (object?)GhiChu?.Trim() ?? "");
            AddIfCol("tongtien", TongCong);
            AddIfCol("tongtienhang", TamTinh);
            AddIfCol("phiship", PhiGiaoHang);
            AddIfCol("phigiaohang", PhiGiaoHang);
            AddIfCol("tongthanhtoan", TongCong);
            AddIfCol("phuongthucthanhtoan", PhuongThucThanhToan);
            AddIfCol("trangthai", "Chờ duyệt");
            AddIfCol("trangthaidon", "Chờ duyệt");
            AddIfCol("ngaydat", DateTime.Now);
            AddIfCol("ngaytao", DateTime.Now);

            var addedSet = new HashSet<string>(insertCols.Select(c => c.Trim('[', ']').ToLower()));
            foreach (var kv in schema)
            {
                var info = kv.Value;
                if (info.IsIdentity || info.IsNullable || addedSet.Contains(kv.Key)) continue;

                string pName = $"@p{paramIdx++}";
                insertCols.Add($"[{info.ActualName}]");
                insertVals.Add(pName);

                object fallback = info.DataType switch
                {
                    "int" or "bigint" or "smallint" or "tinyint" => 1,
                    "decimal" or "numeric" or "money" or "float" => 0m,
                    "bit" => false,
                    "datetime" or "datetime2" or "date" => DateTime.Now,
                    _ => ""
                };
                cmdIns.Parameters.AddWithValue(pName, fallback);
            }

            cmdIns.CommandText = $"INSERT INTO [{orderTable}] ({string.Join(", ", insertCols)}) OUTPUT INSERTED.* VALUES ({string.Join(", ", insertVals)})";

            int newOrderId = 0;
            using (var r = await cmdIns.ExecuteReaderAsync())
            {
                if (await r.ReadAsync())
                {
                    newOrderId = Convert.ToInt32(r.GetValue(0));
                }
            }

            if (newOrderId > 0 && detailTable != null)
            {
                var dSchema = await GetTableSchemaAsync(conn, detailTable);
                foreach (var sp in DanhSachSanPham)
                {
                    var dCols = new List<string>();
                    var dVals = new List<string>();
                    var cmdD = new SqlCommand { Connection = conn };
                    int dIdx = 0;

                    void AddDetail(string colLower, object val)
                    {
                        if (dSchema.TryGetValue(colLower, out var info) && !info.IsIdentity)
                        {
                            string pName = $"@d{dIdx++}";
                            dCols.Add($"[{info.ActualName}]");
                            dVals.Add(pName);
                            cmdD.Parameters.AddWithValue(pName, val);
                        }
                    }

                    AddDetail("id_donhang", newOrderId);
                    AddDetail("iddonhang", newOrderId);
                    AddDetail("madonhang", newOrderId);
                    AddDetail("donhangid", newOrderId);
                    AddDetail("id_sanpham", sp.IdSanPham);
                    AddDetail("idsanpham", sp.IdSanPham);
                    AddDetail("masanpham", sp.IdSanPham);
                    AddDetail("sanphamid", sp.IdSanPham);
                    AddDetail("mathucpham", sp.IdSanPham);
                    AddDetail("tensanpham", sp.TenSanPham);
                    AddDetail("soluong", sp.SoLuong);
                    AddDetail("dongia", sp.GiaBan);
                    AddDetail("giaban", sp.GiaBan);
                    AddDetail("thanhtien", sp.ThanhTien);

                    var dAddedSet = new HashSet<string>(dCols.Select(c => c.Trim('[', ']').ToLower()));
                    foreach (var kv in dSchema)
                    {
                        var info = kv.Value;
                        if (info.IsIdentity || info.IsNullable || dAddedSet.Contains(kv.Key)) continue;

                        string pName = $"@d{dIdx++}";
                        dCols.Add($"[{info.ActualName}]");
                        dVals.Add(pName);

                        object fallback = info.DataType switch
                        {
                            "int" or "bigint" or "smallint" or "tinyint" => 1,
                            "decimal" or "numeric" or "money" or "float" => sp.GiaBan,
                            "bit" => false,
                            "datetime" or "datetime2" or "date" => DateTime.Now,
                            _ => sp.TenSanPham
                        };
                        cmdD.Parameters.AddWithValue(pName, fallback);
                    }

                    cmdD.CommandText = $"INSERT INTO [{detailTable}] ({string.Join(", ", dCols)}) VALUES ({string.Join(", ", dVals)})";
                    await cmdD.ExecuteNonQueryAsync();
                }
            }

            if (cartTable != null)
            {
                var cmdDel = new SqlCommand($"DELETE FROM [{cartTable}]", conn);
                await cmdDel.ExecuteNonQueryAsync();
            }

            MaDonVuaDat = newOrderId;
            DanhSachSanPham.Clear();
            return Page();
        }

private async Task<int> ResolveCustomerIdAsync(SqlConnection conn)
        {
            string? cusTable = await FindTableAsync(conn, "KhachHang", "KhachHangs");
            if (cusTable != null)
            {
                var cmd = new SqlCommand($"SELECT TOP 1 * FROM [{cusTable}] ORDER BY 1 ASC", conn);
                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    return Convert.ToInt32(r.GetValue(0));
                }
            }
            return 1;
        }

        private async Task LoadCartAsync()
        {
            DanhSachSanPham.Clear();
            using var conn = new SqlConnection(_customerConnStr);
            await conn.OpenAsync();

            string? cartTable = await FindTableAsync(conn, "GioHang", "GioHangs");
            string? prodTable = await FindTableAsync(conn, "SanPham", "SanPhams");
            if (cartTable == null || prodTable == null) return;

            var cSchema = await GetTableSchemaAsync(conn, cartTable);
            var pSchema = await GetTableSchemaAsync(conn, prodTable);

            string cartProdFk = cSchema.Values
                .FirstOrDefault(c => !c.IsIdentity && (c.ActualName.Contains("SanPham", StringComparison.OrdinalIgnoreCase) || c.ActualName.Contains("ThucPham", StringComparison.OrdinalIgnoreCase)))?.ActualName
                ?? cSchema.Values.First(c => !c.IsIdentity).ActualName;

            string cartQtyCol = cSchema.Values
                .FirstOrDefault(c => c.ActualName.Contains("SoLuong", StringComparison.OrdinalIgnoreCase) || c.ActualName.Contains("Qty", StringComparison.OrdinalIgnoreCase))?.ActualName
                ?? "SoLuong";

            string prodPk = pSchema.Values.FirstOrDefault(c => c.IsIdentity)?.ActualName
                ?? pSchema.Values.First().ActualName;

            string prodName = pSchema.Values
                .FirstOrDefault(c => c.ActualName.Contains("Ten", StringComparison.OrdinalIgnoreCase))?.ActualName
                ?? "TenSanPham";

            string prodPrice = pSchema.Values
                .FirstOrDefault(c => c.ActualName.Contains("Gia", StringComparison.OrdinalIgnoreCase))?.ActualName
                ?? "GiaBan";

            string imgCol = pSchema.TryGetValue("hinhanh", out var imgInfo) ? $"ISNULL(p.[{imgInfo.ActualName}], '')" : "''";
            string dvtCol = pSchema.TryGetValue("donvitinh", out var dvtInfo) ? $"ISNULL(p.[{dvtInfo.ActualName}], N'Kg')" : "N'Kg'";

            var cmd = new SqlCommand($@"
                SELECT g.[{cartProdFk}], p.[{prodName}], {imgCol}, {dvtCol}, p.[{prodPrice}], g.[{cartQtyCol}]
                FROM [{cartTable}] g
                JOIN [{prodTable}] p ON g.[{cartProdFk}] = p.[{prodPk}]", conn);

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                DanhSachSanPham.Add(new CartCheckoutItem
                {
                    IdSanPham = Convert.ToInt32(r.GetValue(0)),
                    TenSanPham = r.GetString(1),
                    HinhAnh = r.GetString(2),
                    DonViTinh = r.GetString(3),
                    GiaBan = Convert.ToDecimal(r.GetValue(4)),
                    SoLuong = Convert.ToInt32(r.GetValue(5))
                });
            }

            TamTinh = DanhSachSanPham.Sum(x => x.ThanhTien);
            PhiGiaoHang = (TamTinh >= 200000 || TamTinh == 0) ? 0 : 15000;
        }

        private sealed class ColInfo
        {
            public string ActualName { get; set; } = string.Empty;
            public string DataType { get; set; } = string.Empty;
            public bool IsNullable { get; set; }
            public bool IsIdentity { get; set; }
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

        private static async Task<Dictionary<string, ColInfo>> GetTableSchemaAsync(SqlConnection conn, string tableName)
        {
            var dict = new Dictionary<string, ColInfo>(StringComparer.OrdinalIgnoreCase);
            var cmd = new SqlCommand(@"
                SELECT c.COLUMN_NAME, c.DATA_TYPE, c.IS_NULLABLE,
                       COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity')
                FROM INFORMATION_SCHEMA.COLUMNS c
                WHERE c.TABLE_NAME = @T", conn);
            cmd.Parameters.AddWithValue("@T", tableName);

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                var name = r.GetString(0);
                dict[name.ToLower()] = new ColInfo
                {
                    ActualName = name,
                    DataType = r.GetString(1).ToLower(),
                    IsNullable = r.GetString(2).Equals("YES", StringComparison.OrdinalIgnoreCase),
                    IsIdentity = !r.IsDBNull(3) && Convert.ToInt32(r.GetValue(3)) == 1
                };
            }
            return dict;
        }
    }
}