using CapheVanPhong.Application.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace CapheVanPhong.Web.Components.Pages.Public;

public class AboutBase : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<AboutBase> Logger { get; set; } = default!;
    [Inject] private ICommercialServiceService CommercialServiceService { get; set; } = default!;

    protected List<TeamMemberViewModel> TeamMembers { get; private set; } = new();
    protected List<FeatureViewModel> Features { get; private set; } = new();
    protected List<CapheVanPhong.Domain.Entities.CommercialService> CommercialServices { get; private set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadCommercialServicesAsync();
        await LoadFeaturesAsync();
        await LoadTeamMembersAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeWaypointsAsync();
            await InitializeAnimationsAsync();
        }
    }

    private async Task InitializeWaypointsAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("eval", @"
                if (typeof $ !== 'undefined' && typeof Waypoint !== 'undefined') {
                    new Waypoint({ element: document.body, handler: function() { } });
                }
            ");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not initialize waypoints.");
        }
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

    private Task LoadTeamMembersAsync()
    {
        // TODO: Replace with actual Application layer query via MediatR
        TeamMembers = new List<TeamMemberViewModel>
        {
            new("Nguyễn Minh Tuấn", "Giám Đốc Điều Hành", "Với hơn 15 năm kinh nghiệm trong ngành F&B, anh Tuấn là người sáng lập và dẫn dắt Cà Phê Văn Phòng từ những ngày đầu.", "public/img/testimonial-1.jpg"),
            new("Trần Thị Mai", "Trưởng Barista", "Chị Mai đã từng đoạt giải nhất cuộc thi pha chế cà phê toàn quốc năm 2018, mang đến những công thức độc đáo cho thực đơn.", "public/img/testimonial-2.jpg"),
            new("Lê Văn Hùng", "Quản Lý Chuỗi", "Anh Hùng phụ trách vận hành và đảm bảo chất lượng dịch vụ đồng đều tại tất cả các chi nhánh.", "public/img/testimonial-3.jpg"),
            new("Phạm Ngọc Linh", "Trưởng Phòng Marketing", "Chị Linh xây dựng và phát triển thương hiệu Cà Phê Văn Phòng, kết nối với cộng đồng yêu cà phê trên khắp Việt Nam.", "public/img/testimonial-4.jpg"),
        };

        return Task.CompletedTask;
    }

    private async Task LoadCommercialServicesAsync()
    {
        try
        {
            CommercialServices = (await CommercialServiceService.GetActiveAsync()).ToList();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not load commercial services for About page.");
            CommercialServices = new List<CapheVanPhong.Domain.Entities.CommercialService>();
        }
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

    // View Models
    protected record FeatureViewModel(string Title, string Description, string IconClass);
    protected record TeamMemberViewModel(string Name, string Role, string Description, string ImageUrl);
}
