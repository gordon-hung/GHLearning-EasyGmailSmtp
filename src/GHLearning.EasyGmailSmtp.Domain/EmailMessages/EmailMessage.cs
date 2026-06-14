namespace GHLearning.EasyGmailSmtp.Domain.EmailMessages;

public sealed class EmailMessage
{
    public EmailAddress To { get; }
    public EmailSubject Subject { get; }
    public EmailBody Body { get; }

    public EmailMessage(EmailAddress to, EmailSubject subject, EmailBody body)
    {
        ArgumentNullException.ThrowIfNull(to);
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(body);

        To = to;
        Subject = subject;
        Body = body;
    }
}
