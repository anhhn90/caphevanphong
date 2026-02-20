using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace CapheVanPhong.Web.Components.Pages.Public;

public class ReservationBase : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<ReservationBase> Logger { get; set; } = default!;

    protected ReservationFormModel ReservationModel { get; private set; } = new();
    protected bool IsSubmitting { get; private set; } = false;
    protected bool IsSubmitted { get; private set; } = false;
    protected string? ErrorMessage { get; private set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeDateTimePickerAsync();
        }
    }

    protected async Task HandleReservationSubmit()
    {
        IsSubmitting = true;
        ErrorMessage = null;
        StateHasChanged();

        try
        {
            // TODO: Replace with actual Application layer command via MediatR
            // e.g., await Mediator.Send(new CreateReservationCommand { ... });

            // Simulate async processing
            await Task.Delay(500);

            Logger.LogInformation(
                "Reservation submitted: {Name}, {Email}, {Date}, {Guests}",
                ReservationModel.FullName,
                ReservationModel.Email,
                ReservationModel.ReservationDate,
                ReservationModel.NumberOfGuests);

            IsSubmitted = true;
            ReservationModel = new ReservationFormModel();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to process reservation.");
            ErrorMessage = "Có lỗi xảy ra khi đặt bàn. Vui lòng thử lại sau hoặc liên hệ trực tiếp qua điện thoại.";
        }
        finally
        {
            IsSubmitting = false;
            StateHasChanged();
        }
    }

    private async Task InitializeDateTimePickerAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("eval", @"
                if (typeof $ !== 'undefined' && typeof $.fn.datetimepicker !== 'undefined') {
                    $('#date').datetimepicker({ format: 'DD/MM/YYYY' });
                    $('#time').datetimepicker({ format: 'HH:mm' });
                }
            ");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not initialize date/time pickers.");
        }
    }

    // Form Model
    public class ReservationFormModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày đặt bàn.")]
        public DateOnly ReservationDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        [Required(ErrorMessage = "Vui lòng nhập giờ đặt bàn.")]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ không hợp lệ. Định dạng: HH:mm (VD: 14:30)")]
        public string ReservationTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn số khách.")]
        [Range(1, 50, ErrorMessage = "Số khách phải từ 1 đến 50 người.")]
        public int NumberOfGuests { get; set; } = 0;

        [StringLength(500, ErrorMessage = "Yêu cầu đặc biệt không được vượt quá 500 ký tự.")]
        public string? SpecialRequests { get; set; }
    }
}
