using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OLMS.Api.Data;
using OLMS.Api.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // <-- Added for logging

namespace OLMS.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly OLMSDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger; // <-- Add logger

        public AuthService(OLMSDbContext context, IEmailService emailService, IConfiguration config, ILogger<AuthService> logger)
        {
            _context = context;
            _emailService = emailService;
            _config = config;
            _logger = logger; // <-- Initialize logger
        }

        public async Task RegisterAsync(User user, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                throw new ApplicationException("Email already registered.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.EmailConfirmationToken = Guid.NewGuid().ToString();
            user.EmailConfirmed = false;
            user.RegisteredAt = DateTime.UtcNow;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Send email with token only
            try
            {
                string token = user.EmailConfirmationToken;

                string emailBody = $@"
Hi {user.FullName},

Thank you for registering!

🔐 Your email confirmation token is:

**{token}**

Please copy and paste this token into the confirmation page in the app.

If you did not register, please ignore this email.";

                await _emailService.SendEmailAsync(user.Email, "Your Email Confirmation Token", emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
            }
        }


        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new ApplicationException("Invalid email or password.");

            if (!user.EmailConfirmed)
                throw new ApplicationException("Email not confirmed.");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new ApplicationException("JWT key is not configured.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:ExpireMinutes"] ?? "60")),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task ConfirmEmailAsync(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
            if (user == null)
                throw new ApplicationException("Invalid token.");

            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;
            await _context.SaveChangesAsync();
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null) return; // Don't reveal user existence

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetTokenExpiration = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            try
            {
                string resetUrl = $"{_config["FrontendUrl"]}/reset-password?token={user.PasswordResetToken}";
                await _emailService.SendEmailAsync(email, "Reset your password", $"Reset here: {resetUrl}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            }
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.PasswordResetToken == token && u.PasswordResetTokenExpiration > DateTime.UtcNow);
            if (user == null)
                throw new ApplicationException("Invalid or expired token.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiration = null;
            await _context.SaveChangesAsync();
        }
    }
}
