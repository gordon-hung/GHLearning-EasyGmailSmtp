using GHLearning.EasyGmailSmtp.Domain.EmailMessages;

namespace GHLearning.EasyGmailSmtp.Application.Abstractions;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
