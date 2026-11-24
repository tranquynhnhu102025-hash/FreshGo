using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace FreshGo.Pages.Customer.DonHang
{
    public class CustomerOrderSummary
    {
        public int IdDonHang { get; set; }
        public string MaDonHienThi { get; set; } = string.Empty;
        public DateTime NgayDat { get; set; }
        public string TrangThai { get; set; } = "Chờ duyệt";
        public decimal TongTien { get; set; }
        public string DiaChiGiao { get; set; } = string.Empty;
        public string TenSanPhamDauTien { get; set; } = "Đơn hàng FreshGo";
        public string HinhAnhDauTien { get; set; } = string.Empty;
        public int SoLuongDauTien { get; set; } = 1;
        public string DonViTinhDauTien { get; set; } = "Kg";
        public int TongSoMon { get; set; } = 1;
    }

    public class LichSuMuaHangModel : PageModel
    {
        private readonly string _customerConnStr;

        public LichSuMuaHangModel(IConfiguration config)
        {
            _customerConnStr = config.GetConnectionString("CustomerConnection")
                ?? @"Server=localhost\SQLEXPRESS;Database=FreshGoCustomerDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public List<CustomerOrderSummary> DanhSachDon { get; set; } = new();
        public string TenKhachHang { get; set; } = "Khách Trần B";

        public async Task OnGetAsync()
        {
            using var conn = new SqlConnection(_customerConnStr);
            await conn.OpenAsync();

            string? orderTable = await FindTableAsync(conn, "DonHang", "DonHangs");
            string? detailTable = await FindTableAsync(conn, "ChiTietDonHang", "ChiTietDonHangs");
            string? prodTable = await FindTableAsync(conn, "SanPham", "SanPhams");
            string? cusTable = await FindTableAsync(conn, "KhachHang", "KhachHangs");

            if (cusTable != null)
            {
                var cmdCus = new SqlCommand($"SELECT TOP 1 * FROM [{cusTable}] ORDER BY 1 ASC", conn);
                using var rCus = await cmdCus.ExecuteReaderAsync();
                if (await rCus.ReadAsync())
                {
                    for (int i = 0; i < rCus.FieldCount; i++)
                    {
                        var col = rCus.GetName(i);
                        if (col.Contains("Ten", StringComparison.OrdinalIgnoreCase) && !rCus.IsDBNull(i))
                        {
                            TenKhachHang = rCus.GetValue(i).ToString() ?? TenKhachHang;
                            break;
                        }
                    }
                }
            }

            if (orderTable == null) return;

            // Đọc chi tiết sản phẩm + tính tổng tiền từng đơn
            var detailMap = new Dictionary<int, List<(string Ten, string Anh, int Qty, string Dvt, decimal ThanhTien)>>();
            if (detailTable != null && prodTable != null)
            {
                var dCols = await GetColumnsAsync(conn, detailTable);
                var pCols = await GetColumnsAsync(conn, prodTable);

                string fkOrder = dCols.Values.FirstOrDefault(c => c.Contains("DonHang", StringComparison.OrdinalIgnoreCase)) ?? "ID_DonHang";
                string fkProd = dCols.Values.FirstOrDefault(c => c.Contains("SanPham", StringComparison.OrdinalIgnoreCase)) ?? "ID_SanPham";
                string dQty = dCols.Values.FirstOrDefault(c => c.Contains("SoLuong", StringComparison.OrdinalIgnoreCase)) ?? "SoLuong";
                string dPrice = dCols.Values.FirstOrDefault(c => c.Contains("Gia", StringComparison.OrdinalIgnoreCase)) ?? "";

                string pPk = pCols.Values.First();
                string pName = pCols.Values.FirstOrDefault(c => c.Contains("Ten", StringComparison.OrdinalIgnoreCase)) ?? "TenSanPham";
                string pPrice = pCols.Values.FirstOrDefault(c => c.Contains("Gia", StringComparison.OrdinalIgnoreCase)) ?? "GiaBan";
                string pImg = pCols.TryGetValue("hinhanh", out var imgActual) ? $"ISNULL(p.[{imgActual}], '')" : "''";
                string pDvt = pCols.TryGetValue("donvitinh", out var dvtActual) ? $"ISNULL(p.[{dvtActual}], N'Kg')" : "N'Kg'";
                string priceExpr = !string.IsNullOrEmpty(dPrice) ? $"ISNULL(d.[{dPrice}], p.[{pPrice}])" : $"p.[{pPrice}]";

                var cmdD = new SqlCommand($@"
                    SELECT d.[{fkOrder}], p.[{pName}], {pImg}, d.[{dQty}], {pDvt}, {priceExpr}
                    FROM [{detailTable}] d
                    JOIN [{prodTable}] p ON d.[{fkProd}] = p.[{pPk}]", conn);
                using var rD = await cmdD.ExecuteReaderAsync();
                while (await rD.ReadAsync())
                {
                    int oid = Convert.ToInt32(rD.GetValue(0));
                    int qty = Convert.ToInt32(rD.GetValue(3));
                    decimal gia = Convert.ToDecimal(rD.GetValue(5));
                    if (!detailMap.ContainsKey(oid)) detailMap[oid] = new();
                    detailMap[oid].Add((rD.GetString(1), rD.GetString(2), qty, rD.GetString(4), gia * qty));
                }
            }

            var cmdO = new SqlCommand($"SELECT * FROM [{orderTable}] ORDER BY 1 DESC", conn);
            using var rO = await cmdO.ExecuteReaderAsync();
            while (await rO.ReadAsync())
            {
                var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < rO.FieldCount; i++)
                    row[rO.GetName(i)] = rO.IsDBNull(i) ? null! : rO.GetValue(i);

                int id = Convert.ToInt32(GetVal(row, "ID_DonHang", "Id", "MaDonHang") ?? 0);
                string maDon = GetVal(row, "MaDon", "MaDonCode")?.ToString() ?? $"KH{id:D3}";
                string st = GetVal(row, "TrangThai", "TrangThaiDon")?.ToString() ?? "Chờ duyệt";

                // Dò mọi biến thể tên cột tổng tiền trong DonHang
                decimal tong = 0m;
                foreach (var kv in row)
                {
                    if (kv.Value != null && (kv.Key.Contains("Tong", StringComparison.OrdinalIgnoreCase) || kv.Key.Contains("ThanhTien", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (decimal.TryParse(kv.Value.ToString(), out var parsed) && parsed > 0)
                        {
                            tong = parsed;
                            break;
                        }
                    }
                }

                var items = detailMap.ContainsKey(id) ? detailMap[id] : new();
                if (tong == 0m && items.Any())
                {
                    decimal sub = items.Sum(x => x.ThanhTien);
                    tong = sub >= 200000 ? sub : sub + 15000;
                }

                string diaChi = GetVal(row, "DiaChiGiao", "DiaChiGiaoHang", "DiaChi")?.ToString() ?? "";
                DateTime ngay = GetVal(row, "NgayDat", "NgayTao") is DateTime dt ? dt : DateTime.Now;
                var first = items.FirstOrDefault();

                DanhSachDon.Add(new CustomerOrderSummary
                {
                    IdDonHang = id,
                    MaDonHienThi = maDon.StartsWith("#") ? maDon : $"#{maDon}",
                    NgayDat = ngay,
                    TrangThai = st,
                    TongTien = tong,
                    DiaChiGiao = diaChi,
                    TenSanPhamDauTien = string.IsNullOrEmpty(first.Ten) ? $"Đơn hàng FreshGo #{id}" : first.Ten,
                    HinhAnhDauTien = first.Anh ?? "",
                    SoLuongDauTien = first.Qty > 0 ? first.Qty : 1,
                    DonViTinhDauTien = string.IsNullOrEmpty(first.Dvt) ? "Kg" : first.Dvt,
                    TongSoMon = items.Count > 0 ? items.Count : 1
                });
            }
        }

        private static object? GetVal(Dictionary<string, object> row, params string[] keys)
        {
            foreach (var k in keys)
                if (row.TryGetValue(k, out var v) && v != null) return v;
            return null;
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