using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceUsers.BLL.DTO
{
    public class AuthResponseDto { public string AccessToken { get; set; } = null!; public string RefreshToken { get; set; } = null!; }
}
