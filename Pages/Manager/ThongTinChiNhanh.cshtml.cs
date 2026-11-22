using System.ComponentModel.DataAnnotations;
using FreshGo.Pages.Manager.Models;
using FreshGo.Pages.Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshGo.Pages.Manager;

public class ThongTinChiNhanhModel : PageModel
{
    private readonly ManagerRepository _repo;

    public ThongTinChiNhanhModel(IConfiguration configuration)
    {
        _repo = new ManagerRepository(configuration);
    }

    [BindProperty]
    public ChiNhanhForm Form { get; set; } = new();

    public bool KhongTimThay { get; set; }

    public List<NhanVienGiaoHang> DanhSachNhanVien { get; set; } = new();
    public List<NhanVienDongGoi> DanhSachNhanVienDongGoi { get; set; } = new();
    public QuanLyChiNhanh? QuanLy { get; set; }

    public async Task OnGetAsync()
    {
        var chiNhanh = await _repo.GetChiNhanhAsync(BranchContext.CurrentBranchId);
        if (chiNhanh is null)
        {
            KhongTimThay = true;
            return;
        }

        Form = new ChiNhanhForm
        {
            TenChiNhanh = chiNhanh.TenChiNhanh,
            KhuVucQuan = chiNhanh.KhuVucQuan,
            DiaChi = chiNhanh.DiaChi
        };

        QuanLy = await _repo.GetQuanLyChiNhanhAsync(BranchContext.CurrentBranchId);
        DanhSachNhanVienDongGoi = await _repo.GetNhanVienDongGoiAsync(BranchContext.CurrentBranchId);
        DanhSachNhanVien = await _repo.GetNhanVienGiaoHangAsync(BranchContext.CurrentBranchId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            QuanLy = await _repo.GetQuanLyChiNhanhAsync(BranchContext.CurrentBranchId);
            DanhSachNhanVienDongGoi = await _repo.GetNhanVienDongGoiAsync(BranchContext.CurrentBranchId);
            DanhSachNhanVien = await _repo.GetNhanVienGiaoHangAsync(BranchContext.CurrentBranchId);
            return Page();
        }

        var hienTai = await _repo.GetChiNhanhAsync(BranchContext.CurrentBranchId);
        if (hienTai is null)
        {
            KhongTimThay = true;
            return Page();
        }

        await _repo.UpdateChiNhanhAsync(new ChiNhanhInfo
        {
            MaChiNhanh = BranchContext.CurrentBranchId,
            TenChiNhanh = Form.TenChiNhanh,
            KhuVucQuan = Form.KhuVucQuan,
            DiaChi = Form.DiaChi,
            PhiShipCoDinh = hienTai.PhiShipCoDinh,
            TrangThaiHoatDong = hienTai.TrangThaiHoatDong
        });

        TempData["Thanhcong"] = "Đã cập nhật thông tin chi nhánh.";
        return RedirectToPage();
    }

    public class ChiNhanhForm
    {
        [Required(ErrorMessage = "Vui lòng nhập tên chi nhánh")]
        [StringLength(150)]
        public string TenChiNhanh { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập khu vực/quận")]
        [StringLength(100)]
        public string KhuVucQuan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(255)]
        public string DiaChi { get; set; } = string.Empty;
    }
}
