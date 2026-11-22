using FreshGo.Pages.Manager.Models;
using FreshGo.Pages.Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshGo.Pages.Manager.DoanhThu;

public class IndexModel : PageModel
{
    private readonly ManagerRepository _repo;

    // Giới hạn an toàn cho biểu đồ theo ngày khi chọn "Tất cả"/"Tùy chọn" với khoảng quá rộng.
    private const int SoNgayToiDaTrenBieuDo = 90;

    public IndexModel(IConfiguration configuration)
    {
        _repo = new ManagerRepository(configuration);
    }

    public string KhoangThoiGian { get; set; } = "7ngay";
    public DateTime TuNgayLoc { get; set; }
    public DateTime DenNgayLoc { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? TrangThaiLoc { get; set; }

    public DoanhThuSummary Summary { get; set; } = new();
    public List<DoanhThuNgay> TheoNgay { get; set; } = new();
    public List<DonHangGanDay> DonGanDay { get; set; } = new();

    public async Task OnGetAsync(string? khoangThoiGian, DateTime? tuNgay, DateTime? denNgay)
    {
        var homNay = DateTime.Today;
        KhoangThoiGian = khoangThoiGian is "homnay" or "7ngay" or "thangnay" or "tatca" or "tuychon" ? khoangThoiGian : "7ngay";

        DateTime tuNgayLoc, denNgayLoc;
        switch (KhoangThoiGian)
        {
            case "homnay":
                tuNgayLoc = homNay;
                denNgayLoc = homNay;
                break;
            case "thangnay":
                tuNgayLoc = new DateTime(homNay.Year, homNay.Month, 1);
                denNgayLoc = homNay;
                break;
            case "tatca":
                tuNgayLoc = await _repo.GetNgayDonHoanTatSomNhatAsync(BranchContext.CurrentBranchId) ?? homNay;
                denNgayLoc = homNay;
                break;
            case "tuychon":
                tuNgayLoc = (tuNgay ?? homNay.AddDays(-6)).Date;
                denNgayLoc = (denNgay ?? homNay).Date;
                if (tuNgayLoc > denNgayLoc) (tuNgayLoc, denNgayLoc) = (denNgayLoc, tuNgayLoc);
                break;
            default: // "7ngay"
                KhoangThoiGian = "7ngay";
                tuNgayLoc = homNay.AddDays(-6);
                denNgayLoc = homNay;
                break;
        }

        TuNgayLoc = tuNgayLoc;
        DenNgayLoc = denNgayLoc;

        Summary = await _repo.GetDoanhThuSummaryAsync(BranchContext.CurrentBranchId, tuNgayLoc, denNgayLoc.AddDays(1));

        var tuNgayBieuDo = tuNgayLoc;
        if ((denNgayLoc - tuNgayBieuDo).TotalDays + 1 > SoNgayToiDaTrenBieuDo)
        {
            tuNgayBieuDo = denNgayLoc.AddDays(-(SoNgayToiDaTrenBieuDo - 1));
        }
        TheoNgay = await _repo.GetDoanhThuTheoNgayAsync(BranchContext.CurrentBranchId, tuNgayBieuDo, denNgayLoc);

        DonGanDay = await _repo.GetDonHangGanDayAsync(BranchContext.CurrentBranchId, 10, TrangThaiLoc);
    }
}
