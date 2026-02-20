using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace CapheVanPhong.Web.Components.Pages.Public;

public class AboutBase : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<AboutBase> Logger { get; set; } = default!;

    protected List<TeamMemberViewModel> TeamMembers { get; private set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadTeamMembersAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeWaypointsAsync();
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

    // View Models
    protected record TeamMemberViewModel(string Name, string Role, string Description, string ImageUrl);
}
