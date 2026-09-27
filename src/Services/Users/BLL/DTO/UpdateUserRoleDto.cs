using System.ComponentModel.DataAnnotations;

namespace AutoServiceUsers.BLL.DTO
{
    public class UpdateUserRoleDto
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
