using System.Threading.Tasks;

namespace OLMS.Api.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
