# FreshGo – ASP.NET Core Razor Pages + SQL Server

FreshGo là bộ mã nguồn khởi đầu cho website quản lý chuỗi cửa hàng thực phẩm và giao hàng, dùng **C# ASP.NET Core Razor Pages, .NET 8, Entity Framework Core 8, SQL Server và Bootstrap 5**.

## 1. Chức năng đã có trong bộ mã nguồn

### Admin
- Dashboard tổng hợp doanh thu, số đơn, chi nhánh và sản phẩm.
- Quản lý sản phẩm và danh mục.
- Quản lý chi nhánh, tự tạo kho chi nhánh.
- Quản lý tài khoản quản lý / shipper, khóa tài khoản và đặt lại mật khẩu.
- Duyệt yêu cầu nhập hàng từ chi nhánh; khi duyệt sẽ trừ kho tổng và cộng kho chi nhánh trong transaction.
- Quản lý voucher và sự kiện.
- Xác nhận hoặc từ chối báo cáo hàng lỗi; xác nhận sẽ trừ tồn kho chi nhánh.
- Quản lý quà đổi điểm.

### Quản lý chi nhánh
- Dashboard doanh thu, đơn hàng, số lượng tồn và nhân sự thuộc chi nhánh.
- Xem tồn kho, cảnh báo tồn thấp.
- Tạo yêu cầu nhập hàng từ kho tổng.
- Xử lý đơn hàng: chờ xác nhận → đang chuẩn bị → chờ shipper.
- Quản lý tài khoản shipper thuộc chi nhánh.
- Ghi nhận chấm công vào ca / tan ca.
- Báo hàng lỗi, hư và lý do.
- Xem, phản hồi khiếu nại khách hàng thuộc chi nhánh.

### Shipper
- Xem đơn chờ giao tại chi nhánh được phân công.
- Nhận đơn, xem địa chỉ và mở vị trí bằng Google Maps.
- Cập nhật giao thành công hoặc giao thất bại kèm lý do.

### Khách hàng
- Đăng ký, đăng nhập, sửa thông tin cá nhân.
- Xem / tìm kiếm sản phẩm, giỏ hàng và đặt hàng.
- Chọn chi nhánh xử lý đơn; hệ thống kiểm tra và giữ tồn kho trước khi xác nhận đặt hàng.
- Sử dụng voucher theo thời hạn, số lượt, giá trị đơn tối thiểu và mức giảm tối đa.
- Theo dõi lịch sử đơn hàng và xác nhận đã nhận hàng.
- Tích điểm theo quy tắc mẫu: mỗi 10.000đ của đơn hoàn tất = 1 điểm.
- Đổi quà bằng điểm, xem lịch sử điểm và lịch sử đổi quà.
- Xem chi nhánh, mở bản đồ, xem sự kiện và gửi khiếu nại.

## 2. Yêu cầu môi trường

- Visual Studio 2022 (có workload **ASP.NET and web development**).
- .NET 8 SDK.
- SQL Server Express / SQL Server Developer và SQL Server Management Studio (SSMS).
- Kết nối internet để tải NuGet và Bootstrap CDN.

> Dự án target `net8.0` để phù hợp với Visual Studio 2022. Lưu ý: .NET 8 đã hết hỗ trợ từ ngày 10/11/2026; Microsoft khuyến nghị .NET 10 LTS cho dự án mới cần cập nhật bảo mật. Bản này ưu tiên tương thích môi trường VS2022 của bạn và phù hợp để học / chạy đồ án cục bộ, không nên đưa nguyên trạng lên production. Nếu máy bạn đang dùng SQL Server instance khác, hãy sửa connection string trong `appsettings.json`.

## 3. Cách chạy trên Visual Studio 2022

1. Giải nén thư mục `FreshGo`.
2. Mở `FreshGo.sln` bằng Visual Studio 2022.
3. Mở `appsettings.json`, sửa `DefaultConnection` đúng tên SQL Server trên máy.
4. Có hai cách tạo database:
   - **Cách A:** Chạy `Database/FreshGo_Database.sql` trong SSMS để tạo database và các bảng.
   - **Cách B:** Không chạy file SQL; ứng dụng sẽ dùng `EnsureCreated()` để tạo database + schema lần đầu.
5. Trong Visual Studio, chọn **Build → Restore NuGet Packages**, sau đó **Build Solution**.
6. Chọn profile `FreshGo` và nhấn F5.
7. Khi khởi chạy lần đầu, `DbInitializer` tự tạo role, tài khoản demo, chi nhánh, danh mục, sản phẩm, kho và tồn kho mẫu.

Connection string mặc định:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS01;Database=FreshGoDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

Nếu sử dụng SQL Express instance mặc định, đổi `Server=.\\SQLEXPRESS01` thành `Server=.\\SQLEXPRESS`. Nếu sử dụng LocalDB, có thể dùng `Server=(localdb)\\MSSQLLocalDB`.

## 4. Tài khoản demo

| Vai trò | Email | Mật khẩu |
|---|---|---|
| Admin | `admin@freshgo.local` | `Admin@123` |
| Quản lý | `manager@freshgo.local` | `Manager@123` |
| Shipper | `shipper@freshgo.local` | `Shipper@123` |
| Khách hàng | `customer@freshgo.local` | `Customer@123` |

Các tài khoản này chỉ dùng để chạy thử đồ án. Hãy đổi mật khẩu trước khi đưa ứng dụng lên môi trường thật.

## 5. Kiểm tra luồng nghiệp vụ mẫu

1. Đăng nhập `manager@freshgo.local`, tạo yêu cầu nhập thêm sản phẩm.
2. Đăng xuất, đăng nhập Admin, vào **Duyệt nhập kho** và duyệt yêu cầu.
3. Đăng nhập khách hàng, thêm sản phẩm vào giỏ và đặt hàng tại chi nhánh Quận 12.
4. Đăng nhập Quản lý, vào **Đơn hàng**, xác nhận đơn và bấm **Đã chuẩn bị xong**.
5. Đăng nhập Shipper, nhận đơn, cập nhật trạng thái **Đã giao**.
6. Đăng nhập khách hàng, vào **Đơn hàng** và bấm **Tôi đã nhận hàng** để hoàn tất và nhận điểm.
7. Vào **Điểm thưởng** để đổi quà hoặc xem lịch sử tích điểm.

## 6. Cấu trúc chính

```text
FreshGo/
├── Pages/                  # Razor Pages theo 4 phân hệ
│   ├── Admin/
│   ├── Manager/
│   ├── Shipper/
│   ├── Customer/
│   ├── Account/
│   └── Shared/
├── Models/                 # Entity và role
├── Data/                   # DbContext và khởi tạo dữ liệu
├── Services/               # CartService, AuditService
├── Database/               # SQL khởi tạo database
├── wwwroot/                # CSS, JavaScript, ảnh
├── Program.cs
├── appsettings.json
└── FreshGo.csproj
```

## 7. Những phần cần cấu hình / phát triển thêm trước khi triển khai thật

- **Thanh toán trực tuyến:** hiện có COD và lựa chọn chuyển khoản xác nhận thủ công; chưa kết nối VNPay, MoMo hoặc cổng ngân hàng.
- **Đăng nhập Google / Zalo:** chưa bật vì cần Client ID, Client Secret và URL callback do chủ ứng dụng đăng ký.
- **Bản đồ:** mở Google Maps bằng địa chỉ, chưa có định vị GPS trực tiếp hoặc tối ưu tuyến đường.
- **Email / SMS / Zalo OA:** chưa cấu hình nhà cung cấp gửi OTP, email quên mật khẩu hoặc thông báo tự động.
- **Ảnh sản phẩm:** đang nhận URL ảnh; chưa có module upload file và lưu trữ cloud.
- **Quy trình kho:** hiện là luồng duyệt và điều chuyển mẫu; cần bổ sung phiếu giao nhận, kiểm kê, lô hàng, hạn sử dụng, nhà cung cấp và nhật ký chi tiết.
- **Doanh thu / lợi nhuận:** dashboard đang tổng hợp tổng tiền đơn đã giao / hoàn tất; chưa trừ phí vận chuyển, giá vốn, chi phí vận hành hoặc hoàn tiền.
- **Bảo mật & vận hành:** cần thêm kiểm thử, rate limiting, logging, backup, cấu hình secrets qua environment variables, chính sách chống giả mạo / kiểm soát tồn kho đồng thời và triển khai migrations cho môi trường production.

## 8. Lưu ý về database

Project dùng `Database.EnsureCreated()` để thuận tiện chạy demo lần đầu. Package EF Core và Identity cố định ở bản 8.0.34. `EnsureCreated()` không thay thế Entity Framework Migrations và không tự cập nhật schema khi bạn thay đổi model sau này. Khi bước sang giai đoạn phát triển lâu dài, hãy tạo migration và chuyển quy trình khởi tạo sang `Database.Migrate()`; không trộn lẫn hai cách.
