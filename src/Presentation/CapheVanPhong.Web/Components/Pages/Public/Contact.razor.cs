using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace CapheVanPhong.Web.Components.Pages.Public;

public class ContactBase : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<ContactBase> Logger { get; set; } = default!;

    protected ContactFormModel ContactModel { get; private set; } = new();
    protected bool IsSubmitting { get; private set; } = false;
    protected bool IsMessageSent { get; private set; } = false;
    protected string? ErrorMessage { get; private set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeMapAsync();
        }
    }

    protected async Task HandleContactSubmit()
    {
        IsSubmitting = true;
        ErrorMessage = null;
        StateHasChanged();

        try
        {
            // TODO: Replace with actual Application layer command via MediatR
            // e.g., await Mediator.Send(new SendContactMessageCommand { ... });

            // Simulate async processing
            await Task.Delay(500);

            Logger.LogInformation(
                "Contact message submitted: {Name}, {Email}, Subject: {Subject}",
                ContactModel.Name,
                ContactModel.Email,
                ContactModel.Subject);

            IsMessageSent = true;
            ContactModel = new ContactFormModel();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to send contact message.");
            ErrorMessage = "Có lỗi xảy ra khi gửi tin nhắn. Vui lòng thử lại sau hoặc liên hệ trực tiếp qua điện thoại.";
        }
        finally
        {
            IsSubmitting = false;
            StateHasChanged();
        }
    }

    private async Task InitializeMapAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("eval", @"
                if (typeof $ !== 'undefined') {
                    // Any contact page specific JS
                }
            ");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not initialize contact page.");
        }
    }

    // Form Model
    public class ContactFormModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự.")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung tin nhắn.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Nội dung phải từ 10 đến 2000 ký tự.")]
        public string Message { get; set; } = string.Empty;
    }
}
