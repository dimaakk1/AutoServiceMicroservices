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

        // =======================
        // GET BY ID
        // =======================
        public async Task<UserDto> GetByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            return new UserDto
            {
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email,

                // 🔥 BLOCK STATUS
                IsBlocked =
                    user.LockoutEnd != null &&
                    user.LockoutEnd > DateTimeOffset.UtcNow
            };
        }

        // =======================
        // GET ALL USERS (ADMIN)
        // =======================
        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = _userManager.Users.ToList();

            return users.Select(user => new UserDto
            {
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email,

                // 🔥 BLOCK STATUS
                IsBlocked =
                    user.LockoutEnd != null &&
                    user.LockoutEnd > DateTimeOffset.UtcNow

            }).ToList();
        }

        // =======================
        // DELETE USER (ADMIN)
        // =======================
        public async Task DeleteAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                throw new Exception("Failed to delete user");
        }

        // =======================
        // BLOCK USER
        // =======================
        public async Task BlockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            user.LockoutEnabled = true;

            user.LockoutEnd =
                DateTimeOffset.UtcNow.AddYears(100);

            await _userManager.UpdateAsync(user);
        }

        // =======================
        // UNBLOCK USER
        // =======================
        public async Task UnblockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            user.LockoutEnd = null;

            await _userManager.UpdateAsync(user);
        }
    }
}
