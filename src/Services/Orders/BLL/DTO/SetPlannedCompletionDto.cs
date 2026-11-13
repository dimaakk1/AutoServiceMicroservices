using System.ComponentModel.DataAnnotations;

namespace AutoserviceOrders.BLL.DTO;

public sealed class SetPlannedCompletionDto
{
    [Required]
    public DateTimeOffset? PlannedCompletionAt { get; set; }
}

public enum PlannedCompletionResult
{
    Saved,
    NotFound,
    OrderAlreadyStartedOrClosed,
    BeforeOrAtAppointment
}
