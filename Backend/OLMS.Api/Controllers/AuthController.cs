using Microsoft.AspNetCore.Mvc;
using OLMS.Api.Models;
using OLMS.Api.Services;
using System;
using System.Threading.Tasks;

namespace OLMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var user = new User
                {
                    Email = dto.Email,
                    FullName = dto.FullName,
                    Role = UserRole.User,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
                };

                await _authService.RegisterAsync(user, dto.Password);
                return Ok(new { message = "Registration successful. Check your email for the confirmation link." });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Registration failed. Please try again later." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var token = await _authService.LoginAsync(dto.Email, dto.Password);
                return Ok(new { message = "Login successful.", token });
            }
            catch (ApplicationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Login failed. Please try again later." });
            }
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            try
            {
                await _authService.ConfirmEmailAsync(token);
                return Ok(new { message = "Email confirmed successfully. You can now log in." });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Email confirmation failed. Please try again later." });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            try
            {
                await _authService.ForgotPasswordAsync(dto.Email);
                return Ok(new { message = "If the email exists, a password reset link has been sent." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Password reset request failed. Please try again later." });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
                return Ok(new { message = "Password reset successful. You can now log in with your new password." });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Password reset failed. Please try again later." });
            }
        }
    }

    // DTO classes for clarity
    public class RegisterDto
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string Password { get; set; }
    }

    public class LoginDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class ForgotPasswordDto
    {
        public required string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }
}
