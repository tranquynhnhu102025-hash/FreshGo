using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshGo.Pages.Customer.BaiViet
{
    public class BaiVietItem
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string ChuyenMuc { get; set; } = string.Empty;
        public string TomTat { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string HinhAnh { get; set; } = string.Empty;
        public string TacGia { get; set; } = string.Empty;
        public string NgayDang { get; set; } = string.Empty;
        public int PhutDoc { get; set; }
    }

    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? ChuyenMuc { get; set; }

        public List<BaiVietItem> DanhSachBaiViet { get; set; } = new();
        public BaiVietItem? BaiNoiBat { get; set; }

        public void OnGet()
        {
            var tatCa = LayDanhSachBaiViet();
            BaiNoiBat = tatCa.FirstOrDefault();

            if (!string.IsNullOrEmpty(ChuyenMuc) && ChuyenMuc != "Tất cả")
            {
                DanhSachBaiViet = tatCa.Where(b => b.ChuyenMuc == ChuyenMuc).ToList();
            }
            else
            {
                DanhSachBaiViet = tatCa;
            }
        }

        public static List<BaiVietItem> LayDanhSachBaiViet() => new()
        {
            new BaiVietItem
            {
                Id = 1,
                TieuDe = "5 Mẹo Chọn Rau Củ Hữu Cơ Tươi Ngon Chuẩn VietGAP Không Dư Lượng Thuốc",
                ChuyenMuc = "Mẹo chọn đồ tươi",
                TomTat = "Nhận biết rau ăn lá, cà chua và củ quả tươi mới thu hoạch trong ngày chỉ qua màu sắc, độ giòn cuống và mùi hương tự nhiên.",
                NoiDung = @"<p>Rau củ sạch đạt chuẩn VietGAP luôn giữ được độ tươi tự nhiên nhờ quy trình thu hoạch và bảo quản lạnh đúng chuẩn ngay tại nông trại.</p>
                            <h5 class='fw-bold text-success mt-4'>1. Quan sát phần cuống và lá</h5>
                            <p>Rau ăn lá như rau muống, cải ngọt tươi mới sẽ có phần cuống giòn, vết cắt còn nhựa mới và lá xanh tự nhiên, không bóng loáng bất thường.</p>
                            <h5 class='fw-bold text-success mt-4'>2. Kiểm tra độ đàn hồi của quả</h5>
                            <p>Với cà chua Đà Lạt hoặc ớt chuông, quả chín cây tự nhiên cầm chắc tay, vỏ căng mịn và có mùi thơm dịu đặc trưng ở phần đài xanh.</p>
                            <h5 class='fw-bold text-success mt-4'>3. Bảo quản đúng nhiệt độ trong tủ lạnh</h5>
                            <p>Nên bọc rau bằng giấy thấm ẩm trước khi cho vào ngăn mát 4°C–8°C để giữ độ giòn ngọt suốt 3–5 ngày mà không bị úng nước.</p>",
                HinhAnh = "https://images.unsplash.com/photo-1540420773420-3366772f4999?w=900",
                TacGia = "Chuyên gia Nông sản FreshGo",
                NgayDang = "20/11/2026",
                PhutDoc = 4
            },
            new BaiVietItem
            {
                Id = 2,
                TieuDe = "Thực Đơn 7 Ngày Eat Clean Với Ức Gà Ta & Cá Hồi Nauy Giàu Dinh Dưỡng",
                ChuyenMuc = "Dinh dưỡng sạch",
                TomTat = "Gợi ý cách kết hợp protein nạc cùng rau xanh thủy canh giúp giữ dáng, đủ năng lượng làm việc mà chế biến chưa tới 20 phút.",
                NoiDung = @"<p>Chế độ ăn sạch (Eat Clean) không hề nhạt nhẽo nếu bạn chọn đúng nguyên liệu tươi trong ngày và ưu tiên các phương pháp áp chảo, hấp hoặc trộn salad.</p>
                            <h5 class='fw-bold text-success mt-4'>Bữa trưa năng lượng: Ức gà áp chảo măng tây</h5>
                            <p>Ức gà ta ướp cùng tiêu đen, tỏi băm và một thìa dầu ô liu trong 15 phút, áp chảo lửa vừa mỗi mặt 4 phút giúp thịt mềm mọng nước.</p>
                            <h5 class='fw-bold text-success mt-4'>Bữa tối nhẹ bụng: Cá hồi phi lê sốt chanh leo</h5>
                            <p>Cá hồi Nauy giàu Omega-3 kết hợp cùng súp lơ xanh hấp chín tới giúp bữa tối thanh đạm và dễ tiêu hóa.</p>",
                HinhAnh = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=900",
                TacGia = "Bếp trưởng Minh Anh",
                NgayDang = "18/11/2026",
                PhutDoc = 5
            },
            new BaiVietItem
            {
                Id = 3,
                TieuDe = "Bí Quyết Kho Thịt Ba Chỉ Mềm Tan, Lên Màu Cánh Gián Đẹp Mắt",
                ChuyenMuc = "Công thức món ngon",
                TomTat = "Cách sơ chế thịt heo tươi trong ngày để khử hoàn toàn mùi hôi, giữ phần mỡ trong giòn và phần nạc thấm vị đậm đà.",
                NoiDung = @"<p>Một nồi thịt ba chỉ kho chuẩn vị cần chọn miếng thịt có tỷ lệ nạc – mỡ xen kẽ đều nhau, lớp bì mỏng và phần thịt hồng tươi.</p>
                            <h5 class='fw-bold text-success mt-4'>Bước 1: Chần sơ khử mùi</h5>
                            <p>Chần nhanh thịt ba chỉ qua nước sôi có vài lát gừng và hành tím đập dập trong 2 phút, sau đó rửa lại bằng nước lạnh để thịt săn chắc.</p>
                            <h5 class='fw-bold text-success mt-4'>Bước 2: Thắng nước màu tự nhiên</h5>
                            <p>Thắng đường phèn với lửa nhỏ tới khi chuyển màu cánh gián thì cho thịt vào đảo săn trước khi thêm nước dừa tươi để kho liu riu.</p>",
                HinhAnh = "https://images.unsplash.com/photo-1544025162-d76694265947?w=900",
                TacGia = "Ẩm thực Gia đình",
                NgayDang = "15/11/2026",
                PhutDoc = 3
            },
            new BaiVietItem
            {
                Id = 4,
                TieuDe = "Phân Biệt Táo Envy Nhập Khẩu Giòn Ngọt Và Cách Giữ Táo Không Bị Thâm",
                ChuyenMuc = "Mẹo chọn đồ tươi",
                TomTat = "Mẹo nhỏ giúp bạn chọn được những trái táo New Zealand giòn rụm, mọng nước và giữ màu trắng đẹp sau khi gọt vỏ.",
                NoiDung = @"<p>Táo Envy nổi tiếng nhờ độ giòn cao, vị ngọt thanh và hương thơm đặc trưng rất được ưa chuộng làm trái cây tráng miệng.</p>
                            <h5 class='fw-bold text-success mt-4'>Cách chọn táo ngon</h5>
                            <p>Chọn quả có vỏ đỏ sọc vàng tự nhiên, cầm nặng tay, ấn nhẹ thấy cứng chắc và phần cuống còn tươi.</p>
                            <h5 class='fw-bold text-success mt-4'>Mẹo giữ táo trắng sau khi cắt</h5>
                            <p>Ngâm miếng táo vừa cắt vào nước lọc pha vài giọt nước cốt chanh trong 1 phút rồi vớt ra, táo sẽ giữ màu trắng tươi suốt nhiều giờ.</p>",
                HinhAnh = "https://images.unsplash.com/photo-1568702846914-96b305d2aaeb?w=900",
                TacGia = "Chuyên gia Nông sản FreshGo",
                NgayDang = "12/11/2026",
                PhutDoc = 3
            }
        };
    }
}