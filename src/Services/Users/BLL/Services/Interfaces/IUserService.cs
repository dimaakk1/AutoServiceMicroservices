using AutoServiceUsers.BLL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceUsers.BLL.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(string userId);
        Task<List<UserDto>> GetAllAsync();
        Task DeleteAsync(string userId);
        Task BlockUserAsync(string userId);
        Task UnblockUserAsync(string userId);
    }
}
