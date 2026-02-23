using CapheVanPhong.Application.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace CapheVanPhong.Web.Components.Pages.Public;

public class HomeBase : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<HomeBase> Logger { get; set; } = default!;
    [Inject] private IHotNewsService HotNewsService { get; set; } = default!;
    [Inject] private ICustomerService CustomerService { get; set; } = default!;

    protected List<MenuItemViewModel> HotCoffeeItems { get; private set; } = new();
    protected List<MenuItemViewModel> ColdCoffeeItems { get; private set; } = new();
    protected List<TestimonialViewModel> Testimonials { get; private set; } = new();
    protected List<HotNewsItem> ActiveHotNews { get; private set; } = new();
    protected List<GoldCustomerViewModel> GoldCustomers { get; private set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadMenuItemsAsync();
        await LoadTestimonialsAsync();
        await LoadActiveHotNewsAsync();
        await LoadGoldCustomersAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeCarouselAsync();
            await InitializeTestimonialCarouselAsync();
        }
    }

    private async Task LoadActiveHotNewsAsync()
    {
        try
        {
            var news = await HotNewsService.GetActiveAsync();
            ActiveHotNews = news.Select(n => new HotNewsItem(
                Id: n.Id,
                Title: n.Title,
                Content: n.Content,
                ImageUrl: string.IsNullOrEmpty(n.ImageName) ? null : $"/public/img/hotnews/{n.ImageName}"
            )).ToList();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not load active hot news.");
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

    private async Task LoadTestimonialsAsync()
    {
        try
        {
            var representatives = await CustomerService.GetTestimonialsForHomepageAsync();
            Testimonials = representatives.Select(r => new TestimonialViewModel(
                Name: $"{r.Title} {r.DisplayName}",
                Profession: $"{r.Position} — {r.Customer.Name}",
                Content: r.Comment ?? string.Empty,
                ImageUrl: string.IsNullOrEmpty(r.AvatarName)
                    ? "public/img/testimonial-1.jpg"
                    : $"/public/img/representatives/{r.AvatarName}",
                StarRating: r.StarRating
            )).ToList();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not load testimonials.");
        }
    }

    private async Task LoadGoldCustomersAsync()
    {
        try
        {
            var customers = await CustomerService.GetGoldCustomersForHomepageAsync();
            GoldCustomers = customers.Select(c => new GoldCustomerViewModel(
                Name: c.Name,
                LogoUrl: string.IsNullOrEmpty(c.LogoName) ? null : $"/public/img/customers/{c.LogoName}",
                Description: c.Description
            )).ToList();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not load gold customers.");
        }
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

    // View Models
    protected record MenuItemViewModel(string Name, string Description, decimal Price, string ImageUrl);
    protected record TestimonialViewModel(string Name, string Profession, string Content, string ImageUrl, int StarRating = 5);
    protected record HotNewsItem(int Id, string Title, string Content, string? ImageUrl);
    protected record GoldCustomerViewModel(string Name, string? LogoUrl, string? Description);
}
