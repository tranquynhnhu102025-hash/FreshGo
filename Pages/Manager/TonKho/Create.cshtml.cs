using System.ComponentModel.DataAnnotations;
using FreshGo.Pages.Manager.Models;
using FreshGo.Pages.Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshGo.Pages.Manager.TonKho;

public class CreateModel : PageModel
{
    private readonly ManagerRepository _repo;

    public CreateModel(IConfiguration configuration)
    {
        _repo = new ManagerRepository(configuration);
    }

    [BindProperty]
    public CreateForm Form { get; set; } = new();

    public List<DanhMucOption> DanhMucList { get; set; } = new();

    public async Task OnGetAsync()
    {
        DanhMucList = await _repo.GetDanhMucAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        DanhMucList = await _repo.GetDanhMucAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _repo.AddThucPhamMoiVaoKhoAsync(BranchContext.CurrentBranchId, Form.TenThucPhamMoi.Trim(), Form.MaDanhMuc,
            Form.DonViTinhMoi.Trim(), Form.GiaBanMoi, Form.SoLuongTon, Form.HanSuDung, Form.TrangThaiKinhDoanh);
        TempData["Thanhcong"] = "Đã tạo sản phẩm mới và thêm vào kho chi nhánh.";

        return RedirectToPage("Index");
    }

    public class CreateForm
    {
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(150)]
        public string TenThucPhamMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int MaDanhMuc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đơn vị tính")]
        [StringLength(50)]
        public string DonViTinhMoi { get; set; } = string.Empty;

        [Range(0, 100000000, ErrorMessage = "Đơn giá không hợp lệ")]
        public int GiaBanMoi { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn không được âm")]
        public int SoLuongTon { get; set; }

        public DateTime? HanSuDung { get; set; }

        [Required]
        public string TrangThaiKinhDoanh { get; set; } = TrangThaiOptions.ConHang;
    }
}
