using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace CapheVanPhong.Web.Components.Pages.Public;

public class HomeBase : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<HomeBase> Logger { get; set; } = default!;

    protected List<MenuItemViewModel> HotCoffeeItems { get; private set; } = new();
    protected List<MenuItemViewModel> ColdCoffeeItems { get; private set; } = new();
    protected List<TestimonialViewModel> Testimonials { get; private set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadMenuItemsAsync();
        await LoadTestimonialsAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeCarouselAsync();
            await InitializeTestimonialCarouselAsync();
        }
    }

    private Task LoadMenuItemsAsync()
    {
        // TODO: Replace with actual Application layer query via MediatR
        HotCoffeeItems = new List<MenuItemViewModel>
        {
            new("Cà Phê Đen", "Cà phê đen nguyên chất, đậm đà hương vị Tây Nguyên.", 25_000, "public/img/menu-1.jpg"),
            new("Cà Phê Sữa", "Cà phê pha với sữa đặc thơm béo, ngọt vừa phải.", 30_000, "public/img/menu-2.jpg"),
            new("Bạc Xỉu", "Cà phê nhẹ pha nhiều sữa, phù hợp cho người mới thử cà phê.", 32_000, "public/img/menu-3.jpg"),
        };

        ColdCoffeeItems = new List<MenuItemViewModel>
        {
            new("Cà Phê Đen Đá", "Cà phê đen pha đá, mát lạnh và sảng khoái.", 25_000, "public/img/menu-1.jpg"),
            new("Cà Phê Sữa Đá", "Cà phê sữa với đá viên giòn tan, giải nhiệt tuyệt vời.", 30_000, "public/img/menu-2.jpg"),
            new("Bạc Xỉu Đá", "Bạc xỉu lạnh mát, thích hợp cho những ngày hè.", 32_000, "public/img/menu-3.jpg"),
        };

        return Task.CompletedTask;
    }

    private Task LoadTestimonialsAsync()
    {
        // TODO: Replace with actual Application layer query via MediatR
        Testimonials = new List<TestimonialViewModel>
        {
            new("Nguyễn Văn An", "Kỹ Sư Phần Mềm", "Cà phê ở đây rất ngon, không gian yên tĩnh rất phù hợp để làm việc. Tôi thường xuyên ghé đây mỗi sáng.", "public/img/testimonial-1.jpg"),
            new("Trần Thị Bích", "Giáo Viên", "Mình rất thích không gian ấm cúng và cà phê thơm ngon tại đây. Nhân viên rất thân thiện và nhiệt tình.", "public/img/testimonial-2.jpg"),
            new("Lê Hoàng Nam", "Doanh Nhân", "Địa điểm lý tưởng để gặp gỡ đối tác. Cà phê ngon, không gian đẹp và dịch vụ chuyên nghiệp.", "public/img/testimonial-3.jpg"),
            new("Phạm Thị Lan", "Nhà Thiết Kế", "Tôi đã thử nhiều quán cà phê nhưng Cà Phê Văn Phòng vẫn là nơi tôi quay lại nhiều nhất.", "public/img/testimonial-4.jpg"),
        };

        return Task.CompletedTask;
    }

    private async Task InitializeCarouselAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("eval", @"
                if (typeof $ !== 'undefined' && $('#blog-carousel').length) {
                    $('#blog-carousel').carousel({ interval: 5000, ride: 'carousel' });
                }
            ");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not initialize carousel.");
        }
    }

    private async Task InitializeTestimonialCarouselAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("eval", @"
                if (typeof $ !== 'undefined' && $('.testimonial-carousel').length && typeof $.fn.owlCarousel !== 'undefined') {
                    $('.testimonial-carousel').owlCarousel({
                        autoplay: true,
                        smartSpeed: 1500,
                        dots: false,
                        loop: true,
                        nav: true,
                        navText: [
                            '<i class=""fa fa-angle-left"" aria-hidden=""true""></i>',
                            '<i class=""fa fa-angle-right"" aria-hidden=""true""></i>'
                        ],
                        responsive: {
                            0: { items: 1 },
                            576: { items: 1 },
                            768: { items: 2 },
                            992: { items: 3 }
                        }
                    });
                }
            ");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not initialize testimonial carousel.");
        }
    }

    // View Models (inner records for simplicity — replace with DTOs from Application layer later)
    protected record MenuItemViewModel(string Name, string Description, decimal Price, string ImageUrl);
    protected record TestimonialViewModel(string Name, string Profession, string Content, string ImageUrl);
}
