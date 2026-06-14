using GHLearning.EasyGmailSmtp.Application.Abstractions;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GHLearning.EasyGmailSmtp.Infrastructure.Email;

public sealed class GoogleSmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ISmtpClientFactory _clientFactory;

    public GoogleSmtpEmailSender(IOptions<SmtpOptions> options, ISmtpClientFactory clientFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(clientFactory);
        _options = options.Value;
        _clientFactory = clientFactory;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        var mime = BuildMimeMessage(message);
        var secureOption = _options.UseSsl
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        // 每次寄信建立並釋放專屬的 SmtpClient：client 是有狀態的連線資源，
        // 使用獨立實例可避免連線狀態跨呼叫殘留，using 則確保資源被確定性釋放。
        using var client = _clientFactory.Create();
        try
        {
            await client.ConnectAsync(_options.Host, _options.Port, secureOption, cancellationToken).ConfigureAwait(false);

            if (_options.RequireAuthentication)
            {
                await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken).ConfigureAwait(false);
            }

            await client.SendAsync(mime, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            // 斷線不可受業務取消影響：即使請求已取消也必須關閉連線，
            // 故傳入 CancellationToken.None，避免連線洩漏並防止原始例外被遮蔽。
            await client.DisconnectAsync(true, CancellationToken.None).ConfigureAwait(false);
        }
    }

    private MimeMessage BuildMimeMessage(EmailMessage message)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        mime.To.Add(MailboxAddress.Parse(message.To.Value));
        mime.Subject = message.Subject.Value;
        mime.Body = new TextPart("plain") { Text = message.Body.Value };
        return mime;
    }
}
