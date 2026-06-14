using GHLearning.EasyGmailSmtp.Domain.EmailMessages;
using GHLearning.EasyGmailSmtp.Infrastructure.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NSubstitute;
using Xunit;

namespace GHLearning.EasyGmailSmtp.Infrastructure.Tests.Email;

public class GoogleSmtpEmailSenderTests
{
    private static SmtpOptions DefaultOptions() => new()
    {
        Host = "smtp.gmail.com",
        Port = 465,
        UseSsl = true,
        RequireAuthentication = true,
        Username = "sender@gmail.com",
        Password = "app-password",
        FromAddress = "sender@gmail.com",
        FromName = "Sender"
    };

    private static EmailMessage SampleMessage() => new(
        new EmailAddress("recipient@example.com"),
        new EmailSubject("Hello"),
        new EmailBody("World"));

    // 將既有的 mock client 包成工廠，讓每次 Create() 都回傳同一個受測 client。
    private static ISmtpClientFactory FactoryFor(ISmtpClient client)
    {
        var factory = Substitute.For<ISmtpClientFactory>();
        factory.Create().Returns(client);
        return factory;
    }

    /// <summary>
    /// 驗證寄信流程是否依序執行：連線、認證、寄送、斷線。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：預設選項（SSL、需認證）與一個 mock SMTP client。</description></item>
    /// <item><description>When：呼叫 <c>SendAsync</c>。</description></item>
    /// <item><description>Then：依序呼叫 ConnectAsync → AuthenticateAsync → SendAsync → DisconnectAsync。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "寄信 - 應依序連線、認證、寄送、斷線")]
    public async Task SendAsync_ConnectsAuthenticatesSendsAndDisconnects_InOrder()
    {
        // Arrange
        var client = Substitute.For<ISmtpClient>();
        var options = Options.Create(DefaultOptions());
        var sut = new GoogleSmtpEmailSender(options, FactoryFor(client));

        // Act
        await sut.SendAsync(SampleMessage(), CancellationToken.None);

        // Assert
        NSubstitute.Received.InOrder(() =>
        {
            client.ConnectAsync(
                "smtp.gmail.com",
                465,
                SecureSocketOptions.SslOnConnect,
                Arg.Any<CancellationToken>());
            client.AuthenticateAsync(
                "sender@gmail.com",
                "app-password",
                Arg.Any<CancellationToken>());
            client.SendAsync(
                Arg.Any<MimeMessage>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<MailKit.ITransferProgress>());
            client.DisconnectAsync(true, Arg.Any<CancellationToken>());
        });
    }

    /// <summary>
    /// 驗證未啟用 SSL 時，是否以 <see cref="SecureSocketOptions.StartTls"/> 連線。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：UseSsl 為 false、Port 為 587 的選項。</description></item>
    /// <item><description>When：呼叫 <c>SendAsync</c>。</description></item>
    /// <item><description>Then：以 host、587、StartTls 連線一次。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "寄信 - 未啟用 SSL 時應使用 StartTls")]
    public async Task SendAsync_WithoutSsl_UsesStartTls()
    {
        // Arrange
        var client = Substitute.For<ISmtpClient>();
        var opts = DefaultOptions();
        opts.UseSsl = false;
        opts.Port = 587;
        var sut = new GoogleSmtpEmailSender(Options.Create(opts), FactoryFor(client));

        // Act
        await sut.SendAsync(SampleMessage(), CancellationToken.None);

        // Assert
        await client.Received(1).ConnectAsync(
            "smtp.gmail.com",
            587,
            SecureSocketOptions.StartTls,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// 驗證 <c>RequireAuthentication</c> 為 false 時，是否略過認證步驟。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：RequireAuthentication 為 false 的選項。</description></item>
    /// <item><description>When：呼叫 <c>SendAsync</c>。</description></item>
    /// <item><description>Then：AuthenticateAsync 從未被呼叫。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "寄信 - 不需認證時應略過認證")]
    public async Task SendAsync_WithRequireAuthenticationFalse_SkipsAuthenticate()
    {
        // Arrange
        var client = Substitute.For<ISmtpClient>();
        var opts = DefaultOptions();
        opts.RequireAuthentication = false;
        var sut = new GoogleSmtpEmailSender(Options.Create(opts), FactoryFor(client));

        // Act
        await sut.SendAsync(SampleMessage(), CancellationToken.None);

        // Assert
        await client.DidNotReceive().AuthenticateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// 驗證是否正確地從領域 <see cref="EmailMessage"/> 組裝出 <see cref="MimeMessage"/>。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：含寄件者選項的設定與一封樣本信件。</description></item>
    /// <item><description>When：呼叫 <c>SendAsync</c> 並攔截傳給 client 的 MimeMessage。</description></item>
    /// <item><description>Then：主旨、寄件者、收件者與內文皆對應領域物件的值。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "寄信 - 應由領域物件正確組裝 MimeMessage")]
    public async Task SendAsync_BuildsMimeMessageFromDomainMessage()
    {
        // Arrange
        var client = Substitute.For<ISmtpClient>();
        var sut = new GoogleSmtpEmailSender(Options.Create(DefaultOptions()), FactoryFor(client));
        MimeMessage? captured = null;

        await client.SendAsync(
            Arg.Do<MimeMessage>(m => captured = m),
            Arg.Any<CancellationToken>(),
            Arg.Any<MailKit.ITransferProgress>());

        // Act
        await sut.SendAsync(SampleMessage(), CancellationToken.None);

        // Assert
        Assert.NotNull(captured);
        Assert.Equal("Hello", captured!.Subject);
        Assert.Equal("sender@gmail.com", ((MailboxAddress)captured.From[0]).Address);
        Assert.Equal("recipient@example.com", ((MailboxAddress)captured.To[0]).Address);
        Assert.Contains("World", captured.TextBody);
    }

    /// <summary>
    /// 驗證即使寄送過程拋出例外，仍會在 finally 中斷線（不洩漏連線）。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：SendAsync 會拋出 <see cref="InvalidOperationException"/> 的 mock client。</description></item>
    /// <item><description>When：呼叫 <c>SendAsync</c>。</description></item>
    /// <item><description>Then：例外向外傳播，但 DisconnectAsync 仍被呼叫一次。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "寄信 - 寄送拋出例外時仍應斷線")]
    public async Task SendAsync_DisconnectsEvenWhenSendThrows()
    {
        // Arrange
        var client = Substitute.For<ISmtpClient>();
        client.SendAsync(
            Arg.Any<MimeMessage>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<MailKit.ITransferProgress>())
            .Returns<Task<string>>(_ => throw new InvalidOperationException("smtp boom"));

        var sut = new GoogleSmtpEmailSender(Options.Create(DefaultOptions()), FactoryFor(client));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.SendAsync(SampleMessage(), CancellationToken.None));

        await client.Received(1).DisconnectAsync(true, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// 驗證每次寄信都向工廠索取一個全新的 client，並於用畢後釋放（using）。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：一個會回傳受測 client 的工廠。</description></item>
    /// <item><description>When：連續呼叫 <c>SendAsync</c> 兩次。</description></item>
    /// <item><description>Then：工廠的 Create 被呼叫兩次，且 client 被 Dispose 兩次。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "寄信 - 每次呼叫應建立並釋放全新 client")]
    public async Task SendAsync_CreatesAndDisposesAFreshClientPerCall()
    {
        // Arrange
        var client = Substitute.For<ISmtpClient>();
        var factory = FactoryFor(client);
        var sut = new GoogleSmtpEmailSender(Options.Create(DefaultOptions()), factory);

        // Act
        await sut.SendAsync(SampleMessage(), CancellationToken.None);
        await sut.SendAsync(SampleMessage(), CancellationToken.None);

        // Assert
        factory.Received(2).Create();
        client.Received(2).Dispose();
    }
}
