using System.ComponentModel.DataAnnotations;

namespace AutoserviceOrders.BLL.DTO
{
    public class UpdateMechanicOrderStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
