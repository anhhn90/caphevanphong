using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace CapheVanPhong.Web.Components.Pages.Public;

public class ServiceBase : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<ServiceBase> Logger { get; set; } = default!;

    protected List<ServiceViewModel> Services { get; private set; } = new();
    protected List<FeatureViewModel> Features { get; private set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadServicesAsync();
        await LoadFeaturesAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeAnimationsAsync();
        }
    }

    private Task LoadServicesAsync()
    {
        // TODO: Replace with actual Application layer query via MediatR
        Services = new List<ServiceViewModel>
        {
            new("Giao Hàng Tận Nơi", "Dịch vụ giao cà phê tận nhà nhanh chóng trong vòng 30 phút. Cà phê luôn nóng hổi và thơm ngon khi đến tay bạn.", "fa fa-truck", "public/img/service-1.jpg"),
            new("Hạt Cà Phê Tươi", "Hạt cà phê được tuyển chọn từ các vùng trồng nổi tiếng Tây Nguyên và Đà Lạt, rang xay tươi mỗi ngày để giữ nguyên hương vị.", "fa fa-coffee", "public/img/service-2.jpg"),
            new("Chất Lượng Hàng Đầu", "Mỗi tách cà phê đều được kiểm soát chất lượng nghiêm ngặt theo tiêu chuẩn quốc tế, đảm bảo hương vị tốt nhất cho khách hàng.", "fa fa-award", "public/img/service-3.jpg"),
            new("Đặt Bàn Trực Tuyến", "Đặt bàn trực tuyến dễ dàng chỉ với vài thao tác. Nhận ngay ưu đãi 30% cho lần đặt bàn đầu tiên qua website.", "fa fa-table", "public/img/service-4.jpg"),
            new("Không Gian Làm Việc", "Không gian yên tĩnh, WiFi tốc độ cao, ổ cắm điện tại mỗi bàn — lý tưởng cho dân văn phòng và freelancer.", "fa fa-laptop", "public/img/service-1.jpg"),
            new("Tổ Chức Sự Kiện", "Cho thuê không gian tổ chức các buổi họp nhóm, sinh nhật, hội thảo nhỏ với sức chứa tối đa 50 người.", "fa fa-calendar", "public/img/service-2.jpg"),
        };

        return Task.CompletedTask;
    }

    private Task LoadFeaturesAsync()
    {
        // TODO: Replace with actual data
        Features = new List<FeatureViewModel>
        {
            new("Nguyên Liệu Sạch", "100% nguyên liệu tự nhiên, không phụ gia độc hại.", "fa-leaf"),
            new("Giá Cả Hợp Lý", "Chất lượng cao với mức giá phù hợp mọi tầng lớp.", "fa-tag"),
            new("Phục Vụ Nhanh", "Thời gian pha chế và phục vụ trung bình dưới 5 phút.", "fa-bolt"),
            new("Hài Lòng 100%", "Cam kết hoàn tiền nếu bạn không hài lòng với sản phẩm.", "fa-thumbs-up"),
        };

        return Task.CompletedTask;
    }

    private async Task InitializeAnimationsAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("eval", @"
                if (typeof $ !== 'undefined') {
                    $('[data-toggle=""tooltip""]').tooltip();
                }
            ");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not initialize service page animations.");
        }
    }

    // View Models
    protected record ServiceViewModel(string Title, string Description, string IconClass, string ImageUrl);
    protected record FeatureViewModel(string Title, string Description, string IconClass);
}
