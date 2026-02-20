using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace CapheVanPhong.Web.Components.Pages.Public;

public class TestimonialBase : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<TestimonialBase> Logger { get; set; } = default!;

    protected List<TestimonialViewModel> Testimonials { get; private set; } = new();
    protected List<StatViewModel> Stats { get; private set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadTestimonialsAsync();
        await LoadStatsAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeTestimonialCarouselAsync();
        }
    }

    private Task LoadTestimonialsAsync()
    {
        // TODO: Replace with actual Application layer query via MediatR
        Testimonials = new List<TestimonialViewModel>
        {
            new("Nguyễn Văn An", "Kỹ Sư Phần Mềm", "Cà phê ở đây rất ngon, không gian yên tĩnh rất phù hợp để làm việc. Tôi thường xuyên ghé đây mỗi sáng trước giờ làm.", 5, "public/img/testimonial-1.jpg"),
            new("Trần Thị Bích", "Giáo Viên", "Mình rất thích không gian ấm cúng và cà phê thơm ngon tại đây. Nhân viên rất thân thiện và nhiệt tình, luôn phục vụ chu đáo.", 5, "public/img/testimonial-2.jpg"),
            new("Lê Hoàng Nam", "Doanh Nhân", "Địa điểm lý tưởng để gặp gỡ đối tác. Cà phê ngon, không gian đẹp và dịch vụ rất chuyên nghiệp. Tôi thường đặt bàn trước mỗi lần đến.", 4, "public/img/testimonial-3.jpg"),
            new("Phạm Thị Lan", "Nhà Thiết Kế", "Tôi đã thử nhiều quán cà phê nhưng Cà Phê Văn Phòng vẫn là nơi tôi quay lại nhiều nhất. Không gian sáng tạo, cà phê tuyệt vời!", 5, "public/img/testimonial-4.jpg"),
            new("Hoàng Đức Minh", "Sinh Viên", "Giá cả hợp lý, không gian thoáng mát, WiFi nhanh — quán cà phê hoàn hảo cho sinh viên học bài.", 4, "public/img/testimonial-1.jpg"),
            new("Vũ Thị Hương", "Bác Sĩ", "Sau một ngày làm việc mệt mỏi, tôi thường ghé Cà Phê Văn Phòng để thư giãn. Tách cà phê trứng ở đây ngon nhất tôi từng uống.", 5, "public/img/testimonial-2.jpg"),
        };

        return Task.CompletedTask;
    }

    private Task LoadStatsAsync()
    {
        // TODO: Replace with actual data from Application layer
        Stats = new List<StatViewModel>
        {
            new("10,000+", "Khách Hàng Hài Lòng"),
            new("50+", "Loại Đồ Uống"),
            new("5", "Chi Nhánh"),
            new("15+", "Năm Kinh Nghiệm"),
        };

        return Task.CompletedTask;
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

    // View Models
    protected record TestimonialViewModel(string Name, string Profession, string Content, int Rating, string ImageUrl);
    protected record StatViewModel(string Value, string Label);
}
