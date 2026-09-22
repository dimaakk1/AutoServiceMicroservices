using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceUsers.BLL.DTO
{
    public class VerifyEmailDto
    {
        public string UserId { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
