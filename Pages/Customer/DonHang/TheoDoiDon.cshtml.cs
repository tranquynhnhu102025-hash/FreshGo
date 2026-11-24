using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace FreshGo.Pages.Customer.DonHang
{
    public class OrderDetailLine
    {
        public string TenSanPham { get; set; } = string.Empty;
        public string HinhAnh { get; set; } = string.Empty;
        public string DonViTinh { get; set; } = "Kg";
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => DonGia * SoLuong;
    }

    public class TheoDoiDonModel : PageModel
    {
        private readonly string _customerConnStr;

        public TheoDoiDonModel(IConfiguration config)
        {
            _customerConnStr = config.GetConnectionString("CustomerConnection")
                ?? @"Server=localhost\SQLEXPRESS;Database=FreshGoCustomerDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public string MaDonHienThi { get; set; } = string.Empty;
        public DateTime NgayDat { get; set; } = DateTime.Now;
        public string TrangThai { get; set; } = "Chờ duyệt";
        public string DiaChiGiao { get; set; } = string.Empty;
        public decimal TongTien { get; set; }
        public List<OrderDetailLine> DanhSachMon { get; set; } = new();

        public async Task OnGetAsync()
        {
            using var conn = new SqlConnection(_customerConnStr);
            await conn.OpenAsync();

            string? orderTable = await FindTableAsync(conn, "DonHang", "DonHangs");
            string? detailTable = await FindTableAsync(conn, "ChiTietDonHang", "ChiTietDonHangs");
            string? prodTable = await FindTableAsync(conn, "SanPham", "SanPhams");

            if (orderTable == null) return;

            var oCols = await GetColumnsAsync(conn, orderTable);
            string oPk = oCols.Values.First();

            string sqlOrder = Id > 0
                ? $"SELECT * FROM [{orderTable}] WHERE [{oPk}] = @Id"
                : $"SELECT TOP 1 * FROM [{orderTable}] ORDER BY 1 DESC";

            var cmdO = new SqlCommand(sqlOrder, conn);
            if (Id > 0) cmdO.Parameters.AddWithValue("@Id", Id);

            using (var rO = await cmdO.ExecuteReaderAsync())
            {
                if (await rO.ReadAsync())
                {
                    var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < rO.FieldCount; i++)
                        row[rO.GetName(i)] = rO.IsDBNull(i) ? null! : rO.GetValue(i);

                    Id = Convert.ToInt32(row[oPk]);
                    string ma = GetVal(row, "MaDon", "MaDonCode")?.ToString() ?? $"KH{Id:D3}";
                    MaDonHienThi = ma.StartsWith("#") ? ma : $"#{ma}";
                    TrangThai = GetVal(row, "TrangThai", "TrangThaiDon")?.ToString() ?? "Chờ duyệt";
                    DiaChiGiao = GetVal(row, "DiaChiGiao", "DiaChiGiaoHang", "DiaChi")?.ToString() ?? "";
                    TongTien = Convert.ToDecimal(GetVal(row, "TongThanhToan", "TongTien") ?? 0m);
                    NgayDat = GetVal(row, "NgayDat", "NgayTao") is DateTime dt ? dt : DateTime.Now;
                }
            }

            if (Id > 0 && detailTable != null && prodTable != null)
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
                    SELECT p.[{pName}], {pImg}, {pDvt}, d.[{dQty}], {priceExpr}
                    FROM [{detailTable}] d
                    JOIN [{prodTable}] p ON d.[{fkProd}] = p.[{pPk}]
                    WHERE d.[{fkOrder}] = @Id", conn);
                cmdD.Parameters.AddWithValue("@Id", Id);

                using var rD = await cmdD.ExecuteReaderAsync();
                while (await rD.ReadAsync())
                {
                    DanhSachMon.Add(new OrderDetailLine
                    {
                        TenSanPham = rD.GetString(0),
                        HinhAnh = rD.GetString(1),
                        DonViTinh = rD.GetString(2),
                        SoLuong = Convert.ToInt32(rD.GetValue(3)),
                        DonGia = Convert.ToDecimal(rD.GetValue(4))
                    });
                }
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