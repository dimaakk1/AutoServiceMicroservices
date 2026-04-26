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

        // 🔹 GET PROFILE
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.UserName,
                user.Email,
                user.FullName
            });
        }

        // 🔹 UPDATE PROFILE (username + fullname)
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            // USERNAME (Identity way)
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

        // 🔹 CHANGE PASSWORD
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
    }
}
