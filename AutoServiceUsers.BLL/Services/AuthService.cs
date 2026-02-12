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


        public AuthService(UserManager<ApplicationUser> userManager, IJwtTokenService jwtService, IEmailService emailService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _emailService = emailService;
        }


        public async Task<object> RegisterAsync(RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "User");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            

            await _emailService.SendEmailAsync(
                user.Email!,
                "Verify your email",
                $"userId={user.Id}&token={Uri.EscapeDataString(token)}"
            );

            return new
            {
                message = "User registered. Please verify your email."
            };
        }


        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user == null)
                throw new Exception("Invalid credentials");

            if (!user.EmailConfirmed)
                throw new Exception("Email not verified");

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new Exception("Invalid credentials");


            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken(user.Id);


            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);


            return new AuthResponseDto { AccessToken = accessToken, RefreshToken = refreshToken.Token };
        }


        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(r => r.Token == refreshToken));

            if (user == null) throw new Exception("Refresh токен не знайдено");

            var token = user.RefreshTokens.First(r => r.Token == refreshToken);

            // Перевірка без IsActive
            if (token.Revoked != null || token.Expires <= DateTime.UtcNow)
                throw new Exception("Refresh токен недійсний");

            // Відкликаємо старий токен
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



        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(r => r.Token == refreshToken));


            if (user == null) throw new Exception("Refresh токен не знайдено");


            var token = user.RefreshTokens.First(r => r.Token == refreshToken);
            token.Revoked = DateTime.UtcNow;


            await _userManager.UpdateAsync(user);
        }

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
        }
    }
}
