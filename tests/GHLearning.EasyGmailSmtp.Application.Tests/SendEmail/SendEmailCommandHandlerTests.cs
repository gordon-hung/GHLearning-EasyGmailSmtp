using GHLearning.EasyGmailSmtp.Application.Abstractions;
using GHLearning.EasyGmailSmtp.Application.SendEmail;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;
using NSubstitute;
using Xunit;

namespace GHLearning.EasyGmailSmtp.Application.Tests.SendEmail;

public class SendEmailCommandHandlerTests
{
    /// <summary>
    /// 驗證收到合法命令時，處理器是否正確將字串轉成領域物件並委派給 <see cref="IEmailSender"/>。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：含合法收件者、主旨、內文的 <see cref="SendEmailCommand"/>。</description></item>
    /// <item><description>When：呼叫 <c>HandleAsync</c>。</description></item>
    /// <item><description>Then：<see cref="IEmailSender.SendAsync"/> 被呼叫一次，且傳入的 <see cref="EmailMessage"/> 欄位值與命令一致。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "處理命令 - 合法命令應委派給寄件者")]
    public async Task HandleAsync_WithValidCommand_DelegatesToSender()
    {
        // Arrange
        var sender = Substitute.For<IEmailSender>();
        var handler = new SendEmailCommandHandler(sender);
        var command = new SendEmailCommand("user@example.com", "Hello", "World");

        // Act
        await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await sender.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m =>
                m.To.Value == "user@example.com" &&
                m.Subject.Value == "Hello" &&
                m.Body.Value == "World"),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// 驗證命令含非法電子郵件地址時，處理器是否在領域驗證階段失敗且不呼叫寄件者。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：收件者為非法地址 "not-an-email" 的命令。</description></item>
    /// <item><description>When：呼叫 <c>HandleAsync</c>。</description></item>
    /// <item><description>Then：拋出 <see cref="InvalidEmailAddressException"/>，且 <see cref="IEmailSender.SendAsync"/> 從未被呼叫。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "處理命令 - 非法地址應失敗且不寄送")]
    public async Task HandleAsync_WithInvalidEmail_DoesNotCallSender()
    {
        // Arrange
        var sender = Substitute.For<IEmailSender>();
        var handler = new SendEmailCommandHandler(sender);
        var command = new SendEmailCommand("not-an-email", "Hello", "World");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidEmailAddressException>(
            () => handler.HandleAsync(command, CancellationToken.None));

        await sender.DidNotReceive().SendAsync(
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// 驗證處理器是否將傳入的 <see cref="CancellationToken"/> 原封不動地傳遞給寄件者。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：合法命令與一個具體的 <see cref="CancellationToken"/>。</description></item>
    /// <item><description>When：以該 token 呼叫 <c>HandleAsync</c>。</description></item>
    /// <item><description>Then：<see cref="IEmailSender.SendAsync"/> 收到的正是同一個 token。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "處理命令 - 應原樣傳遞取消權杖")]
    public async Task HandleAsync_PropagatesCancellationToken()
    {
        // Arrange
        var sender = Substitute.For<IEmailSender>();
        var handler = new SendEmailCommandHandler(sender);
        var command = new SendEmailCommand("user@example.com", "Hello", "World");
        using var cts = new CancellationTokenSource();

        // Act
        await handler.HandleAsync(command, cts.Token);

        // Assert
        await sender.Received(1).SendAsync(Arg.Any<EmailMessage>(), cts.Token);
    }
}
