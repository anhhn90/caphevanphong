using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace CapheVanPhong.Web.Components.Pages.Public;

public class MenuBase : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<MenuBase> Logger { get; set; } = default!;

    protected List<MenuCategoryViewModel> MenuCategories { get; private set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadMenuCategoriesAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeAnimationsAsync();
        }
    }

    private Task LoadMenuCategoriesAsync()
    {
        // TODO: Replace with actual Application layer query via MediatR
        MenuCategories = new List<MenuCategoryViewModel>
        {
            new("Cà Phê Nóng", new List<MenuItemViewModel>
            {
                new("Cà Phê Đen Nóng", "Cà phê đen nguyên chất, đậm đà hương vị Tây Nguyên, pha bằng phin truyền thống.", 25_000, "public/img/menu-1.jpg"),
                new("Cà Phê Sữa Nóng", "Cà phê pha với sữa đặc thơm béo, ngọt vừa phải — hương vị quen thuộc của người Việt.", 30_000, "public/img/menu-2.jpg"),
                new("Bạc Xỉu Nóng", "Cà phê nhẹ pha nhiều sữa, phù hợp cho người mới thử hoặc thích vị nhẹ hơn.", 32_000, "public/img/menu-3.jpg"),
                new("Cà Phê Trứng", "Đặc sản Hà Nội, cà phê phủ lớp kem trứng đánh bông mịn, béo ngậy và thơm lừng.", 45_000, "public/img/menu-1.jpg"),
            }),
            new("Cà Phê Đá", new List<MenuItemViewModel>
            {
                new("Cà Phê Đen Đá", "Cà phê đen pha đá, mát lạnh và sảng khoái, thích hợp cho những ngày nóng bức.", 25_000, "public/img/menu-1.jpg"),
                new("Cà Phê Sữa Đá", "Cà phê sữa với đá viên giòn tan, giải nhiệt tuyệt vời và đầy năng lượng.", 30_000, "public/img/menu-2.jpg"),
                new("Bạc Xỉu Đá", "Bạc xỉu lạnh mát, thích hợp cho những ngày hè oi ả.", 32_000, "public/img/menu-3.jpg"),
                new("Cold Brew", "Cà phê ủ lạnh 12 tiếng, hương thơm đậm đà, vị ngọt tự nhiên, ít chua và ít đắng.", 55_000, "public/img/menu-2.jpg"),
            }),
            new("Đồ Uống Khác", new List<MenuItemViewModel>
            {
                new("Trà Đào Cam Sả", "Trà đào kết hợp cam tươi và sả thơm, vị chua ngọt thanh mát.", 35_000, "public/img/menu-3.jpg"),
                new("Sinh Tố Bơ", "Sinh tố bơ Đà Lạt nguyên chất, béo ngậy và bổ dưỡng.", 40_000, "public/img/menu-1.jpg"),
                new("Nước Ép Cam", "Nước ép cam tươi 100%, giàu vitamin C, không đường, không phẩm màu.", 30_000, "public/img/menu-2.jpg"),
                new("Matcha Latte", "Matcha Nhật Bản cao cấp pha với sữa tươi nguyên kem, thơm béo và bổ dưỡng.", 45_000, "public/img/menu-3.jpg"),
            }),
            new("Bánh & Snack", new List<MenuItemViewModel>
            {
                new("Bánh Mì Bơ Kẹp Trứng", "Bánh mì giòn tan, bơ béo, trứng ốp la, ăn kèm cà phê buổi sáng cực ngon.", 25_000, "public/img/menu-1.jpg"),
                new("Croissant Bơ Pháp", "Croissant nhập khẩu từ Pháp, lớp vỏ giòn vàng, ruột mềm xốp và thơm bơ.", 35_000, "public/img/menu-2.jpg"),
                new("Bánh Tiramisu", "Bánh Tiramisu truyền thống, tầng lớp mascarpone mịn mượt, cà phê đậm đà.", 45_000, "public/img/menu-3.jpg"),
            }),
        };

        return Task.CompletedTask;
    }

    private async Task InitializeAnimationsAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("eval", @"
                if (typeof $ !== 'undefined') {
                    // Any menu-specific JS initialization
                }
            ");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not initialize menu page animations.");
        }
    }

    // View Models
    protected record MenuItemViewModel(string Name, string Description, decimal Price, string ImageUrl);
    protected record MenuCategoryViewModel(string Name, List<MenuItemViewModel> Items);
}
