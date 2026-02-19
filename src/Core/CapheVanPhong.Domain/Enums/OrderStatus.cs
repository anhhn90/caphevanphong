namespace CapheVanPhong.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,        // Chờ xác nhận
    Confirmed = 1,      // Đã xác nhận
    Preparing = 2,      // Đang chuẩn bị
    Ready = 3,          // Sẵn sàng
    Delivering = 4,     // Đang giao hàng
    Completed = 5,      // Hoàn thành
    Cancelled = 6       // Đã hủy
}
