using AutoServiceUsers.BLL.DTO;
using AutoServiceUsers.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoServiceUsers.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // =========================
        // 🔹 PROFILE (current user)
        // =========================

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FullName
            });
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.UserName) &&
                dto.UserName != user.UserName)
            {
                var result = await _userManager.SetUserNameAsync(user, dto.UserName);

                if (!result.Succeeded)
                    return BadRequest(result.Errors);
            }

            user.FullName = dto.FullName;

            var update = await _userManager.UpdateAsync(user);

            if (!update.Succeeded)
                return BadRequest(update.Errors);

            return Ok(new { message = "Profile updated" });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(
                user,
                dto.CurrentPassword,
                dto.NewPassword
            );

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Password changed successfully" });
        }

        // =========================
        // 🔥 ADMIN PANEL
        // =========================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            var users = _userManager.Users
                .Select(u => new UserDto
                {
                    UserId = u.Id,
                    Username = u.UserName,
                    Email = u.Email,

                    IsBlocked =
                        u.LockoutEnd != null &&
                        u.LockoutEnd > DateTimeOffset.UtcNow
                })
                .ToList();

            return Ok(users);
        }

        // 🔹 GET USER BY ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            return Ok(new UserDto
            {
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email,

                IsBlocked =
        user.LockoutEnd != null &&
        user.LockoutEnd > DateTimeOffset.UtcNow
            });
        }

        // 🔹 DELETE USER
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "User deleted" });
        }

        // 🔹 BLOCK USER
        [HttpPost("block/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Block(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

            await _userManager.UpdateAsync(user);

            return Ok(new { message = "User blocked" });
        }

        // 🔹 UNBLOCK USER
        [HttpPost("unblock/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unblock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            user.LockoutEnd = null;

            await _userManager.UpdateAsync(user);

            return Ok(new { message = "User unblocked" });
        }
    }
}