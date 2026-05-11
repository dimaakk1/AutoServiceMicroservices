using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceUsers.BLL.DTO
{
    public class UserDto
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsBlocked { get; set; } 

    }
}
