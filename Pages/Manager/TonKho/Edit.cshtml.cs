using System.ComponentModel.DataAnnotations;
using FreshGo.Pages.Manager.Models;
using FreshGo.Pages.Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshGo.Pages.Manager.TonKho;

public class EditModel : PageModel
{
    private readonly ManagerRepository _repo;

    public EditModel(IConfiguration configuration)
    {
        _repo = new ManagerRepository(configuration);
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public TonKhoForm Form { get; set; } = new();

    public TonKhoItem? Item { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Item = await _repo.GetTonKhoItemAsync(Id, BranchContext.CurrentBranchId);
        if (Item is null) return NotFound();

        Form = new TonKhoForm
        {
            GiaBan = (int)Item.GiaBan,
            SoLuongTon = Item.SoLuongTon,
            SoLuongLoiHu = Item.SoLuongLoiHu,
            LyDoLoiHu = Item.LyDoLoiHu,
            HanSuDung = Item.HanSuDung,
            TrangThaiKinhDoanh = Item.TrangThaiKinhDoanh
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Item = await _repo.GetTonKhoItemAsync(Id, BranchContext.CurrentBranchId);
        if (Item is null) return NotFound();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _repo.UpdateTonKhoAsync(Id, BranchContext.CurrentBranchId, Item.MaThucPham, Form.SoLuongTon, Form.SoLuongLoiHu,
            Form.LyDoLoiHu, Form.HanSuDung, Form.TrangThaiKinhDoanh, Form.GiaBan);
        TempData["Thanhcong"] = "Đã cập nhật tồn kho.";
        return RedirectToPage("Index");
    }

    public class TonKhoForm
    {
        [Range(0, 100000000, ErrorMessage = "Đơn giá không hợp lệ")]
        public int GiaBan { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn không được âm")]
        public int SoLuongTon { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng lỗi/hư không được âm")]
        public int SoLuongLoiHu { get; set; }

        [StringLength(255)]
        public string? LyDoLoiHu { get; set; }

        public DateTime? HanSuDung { get; set; }

        [Required]
        public string TrangThaiKinhDoanh { get; set; } = TrangThaiOptions.ConHang;
    }
}
