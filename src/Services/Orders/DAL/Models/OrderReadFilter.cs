namespace AutoserviceOrders.DAL.Models;

public sealed record OrderReadFilter(string? UserId, string? MechanicId, string? Status,
    DateTime? Date, DateTime? FromDate, DateTime? ToDate, int Offset = 0, int? Limit = null);
