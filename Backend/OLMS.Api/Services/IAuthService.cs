using OLMS.Api.Models;
using System.Threading.Tasks;

namespace OLMS.Api.Services
{
    public interface IAuthService
    {
        Task RegisterAsync(User user, string password);
        Task<string> LoginAsync(string email, string password);
        Task ConfirmEmailAsync(string token);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(string token, string newPassword);
    }
}