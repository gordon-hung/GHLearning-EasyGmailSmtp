using MailKit.Net.Smtp;

namespace GHLearning.EasyGmailSmtp.Infrastructure.Email;

/// <summary>
/// 建立 SMTP 用戶端的工廠。
/// SmtpClient 是有狀態的連線資源，透過工廠在每次寄信時取得獨立實例，
/// 可避免連線狀態跨呼叫殘留，並讓資源生命週期由呼叫端確定性掌控。
/// </summary>
public interface ISmtpClientFactory
{
    ISmtpClient Create();
}
