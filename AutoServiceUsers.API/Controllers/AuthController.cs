using AutoServiceUsers.BLL.DTO;
using AutoServiceUsers.BLL.Services.Interfaces;
using AutoServiceUsers.DAL.DB;
using AutoServiceUsers.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AutoServiceUsers.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // -----------------------------
        // REGISTER
        // -----------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                await _authService.RegisterAsync(dto);

                return Ok(new
                {
                    message = "Реєстрація успішна. Перевірте email для підтвердження."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // -----------------------------
        // LOGIN
        // -----------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // -----------------------------
        // REFRESH TOKEN
        // -----------------------------
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // -----------------------------
        // REVOKE TOKEN
        // -----------------------------
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RefreshRequestDto dto)
        {
            try
            {
                await _authService.RevokeRefreshTokenAsync(dto.RefreshToken);
                return Ok(new { message = "Refresh token revoked" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // -----------------------------
        // EMAIL VERIFICATION (ВАЖЛИВО)
        // -----------------------------
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string userId, [FromQuery] string token)
        {
            try
            {
                var dto = new VerifyEmailDto
                {
                    UserId = userId,
                    Token = token
                };

                await _authService.VerifyEmailAsync(dto);

                return Redirect("http://localhost:5173/email-confirmed?status=success");
            }
            catch
            {
                return Redirect("http://localhost:5173/email-confirmed?status=error");
            }
        }

        // -----------------------------
        // TEST
        // -----------------------------
        [HttpGet("ping")]
        [AllowAnonymous]
        public IActionResult Ping()
        {
            return Ok(new { message = "AuthService працює!" });
        }

        // -----------------------------
        // JWT TEST
        // -----------------------------
        [HttpGet("protected")]
        [Authorize]
        public IActionResult Protected()
        {
            var username = User.Identity?.Name;
            return Ok(new { message = $"JWT захист працює! Користувач: {username}" });
        }
    }
}
