using AutoServiceUsers.BLL.DTO;
using AutoServiceUsers.BLL.Services.Interfaces;
using AutoServiceUsers.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceUsers.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserDto> GetByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            return new UserDto
            {
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email
            };
        }
    }
}
