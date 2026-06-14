using GHLearning.EasyGmailSmtp.Application.Abstractions;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages;

namespace GHLearning.EasyGmailSmtp.Application.SendEmail;

public sealed class SendEmailCommandHandler
{
    private readonly IEmailSender _emailSender;

    public SendEmailCommandHandler(IEmailSender emailSender)
    {
        ArgumentNullException.ThrowIfNull(emailSender);
        _emailSender = emailSender;
    }

    public async Task HandleAsync(SendEmailCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var message = new EmailMessage(
            new EmailAddress(command.To),
            new EmailSubject(command.Subject),
            new EmailBody(command.Body));

        await _emailSender.SendAsync(message, cancellationToken).ConfigureAwait(false);
    }
}
