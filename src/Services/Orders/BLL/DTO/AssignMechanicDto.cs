using System.ComponentModel.DataAnnotations;

namespace AutoserviceOrders.BLL.DTO
{
    public class AssignMechanicDto
    {
        [Required]
        public string MechanicId { get; set; } = string.Empty;

        [Required]
        public string MechanicName { get; set; } = string.Empty;
    }
}
