using MailKit.Net.Smtp;

namespace GHLearning.EasyGmailSmtp.Infrastructure.Email;

/// <summary>
/// 預設工廠：每次呼叫建立全新的 MailKit <see cref="SmtpClient"/>。
/// </summary>
internal sealed class SmtpClientFactory : ISmtpClientFactory
{
    public ISmtpClient Create() => new SmtpClient();
}
