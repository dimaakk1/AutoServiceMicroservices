using AutoServiceUsers.BLL.DTO;
using AutoServiceUsers.BLL.Services.Interfaces;
using AutoServiceUsers.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceUsers.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtService;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtService,
            IEmailService emailService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        // -------------------------
        // REGISTER
        // -------------------------
        public async Task<object> RegisterAsync(RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email,

                // ❌ ВАЖЛИВО: НЕ true
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "User");

            // -------------------------
            // EMAIL CONFIRM TOKEN
            // -------------------------
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);

            var link =
                $"http://localhost:5000/api/auth/verify-email?userId={user.Id}&token={encodedToken}";

            // -------------------------
            // SEND EMAIL
            // -------------------------
            await _emailService.SendEmailAsync(
    user.Email!,
    "Підтвердження Email",
    $@"
    <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>

        <div style='background:#f97316;padding:25px;text-align:center;'>
            <h1 style='color:white;margin:0;'>
                🚗 Auto Service
            </h1>
        </div>

        <div style='background:#f9fafb;padding:30px;'>

            <h2 style='color:#111827;'>
                Вітаємо, {user.UserName}!
            </h2>

            <p style='font-size:16px;color:#374151;line-height:1.6;'>
                Дякуємо за реєстрацію в Auto Service.
                Для завершення створення акаунта необхідно підтвердити вашу електронну пошту.
            </p>

            <div style='text-align:center;margin:35px 0;'>

                <a href='{link}'
                   style='
                        background:#f97316;
                        color:white;
                        text-decoration:none;
                        padding:14px 28px;
                        border-radius:8px;
                        font-weight:bold;
                        display:inline-block;'>
                    Підтвердити Email
                </a>

            </div>

            <p style='color:#6b7280;font-size:14px;'>
                Якщо кнопка не працює, скопіюйте та відкрийте це посилання:
            </p>

            <p style='word-break:break-all;font-size:14px;'>
                <a href='{link}'>{link}</a>
            </p>

        </div>

        <div style='background:#111827;color:white;padding:15px;text-align:center;'>
            © 2026 Auto Service
        </div>

    </div>"
);

            return new
            {
                message = "User registered. Check email to confirm account."
            };
        }

        // -------------------------
        // LOGIN
        // -------------------------
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);

            if (user == null)
                throw new Exception("Invalid credentials");

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new Exception("Invalid credentials");

            // ❌ email verification check
            if (!user.EmailConfirmed)
                throw new Exception("Email not verified");

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
                throw new Exception("User is blocked");

            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken(user.Id);

            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        // -------------------------
        // REFRESH TOKEN
        // -------------------------
        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u =>
                    u.RefreshTokens.Any(r => r.Token == refreshToken));

            if (user == null)
                throw new Exception("Refresh token not found");

            var token = user.RefreshTokens.First(r => r.Token == refreshToken);

            if (token.Revoked != null || token.Expires <= DateTime.UtcNow)
                throw new Exception("Refresh token invalid");

            token.Revoked = DateTime.UtcNow;

            var roles = await _userManager.GetRolesAsync(user);

            var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
            var newRefreshToken = _jwtService.GenerateRefreshToken(user.Id);

            user.RefreshTokens.Add(newRefreshToken);

            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        // -------------------------
        // REVOKE TOKEN
        // -------------------------
        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u =>
                    u.RefreshTokens.Any(r => r.Token == refreshToken));

            if (user == null)
                throw new Exception("Refresh token not found");

            var token = user.RefreshTokens.First(r => r.Token == refreshToken);
            token.Revoked = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
        }

        // -------------------------
        // VERIFY EMAIL
        // -------------------------
        public async Task VerifyEmailAsync(VerifyEmailDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (user == null)
                throw new Exception("User not found");

            if (user.EmailConfirmed)
                throw new Exception("Email already confirmed");

            var decodedToken = Uri.UnescapeDataString(dto.Token);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
                throw new Exception("Invalid or expired token");

            // optional but good practice
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }
    }
}
