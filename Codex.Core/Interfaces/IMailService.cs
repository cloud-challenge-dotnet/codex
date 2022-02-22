using Codex.Core.Models.Mail;
using System.Threading.Tasks;

namespace Codex.Core.Interfaces;

public interface IMailService
{
    Task SendEmailAsync(Message message);
}