using GHLearning.EasyGmailSmtp.Domain.EmailMessages;
using Xunit;

namespace GHLearning.EasyGmailSmtp.Domain.Tests.EmailMessages;

public class EmailMessageTests
{
    /// <summary>
    /// 驗證以完整的三個值物件建立 <see cref="EmailMessage"/> 時，是否正確設定各屬性。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：合法的收件者、主旨與內文值物件。</description></item>
    /// <item><description>When：建立 <see cref="EmailMessage"/>。</description></item>
    /// <item><description>Then：<c>To</c>、<c>Subject</c>、<c>Body</c> 皆等於傳入的值物件。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "建立信件 - 完整欄位應正確設定屬性")]
    public void Constructor_WithAllParts_SetsProperties()
    {
        // Arrange
        var to = new EmailAddress("user@example.com");
        var subject = new EmailSubject("Hello");
        var body = new EmailBody("World");

        // Act
        var message = new EmailMessage(to, subject, body);

        // Assert
        Assert.Equal(to, message.To);
        Assert.Equal(subject, message.Subject);
        Assert.Equal(body, message.Body);
    }

    /// <summary>
    /// 驗證收件者為 null 時，建立 <see cref="EmailMessage"/> 是否拋出 <see cref="ArgumentNullException"/>。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：收件者為 null，主旨與內文合法。</description></item>
    /// <item><description>When：建立 <see cref="EmailMessage"/>。</description></item>
    /// <item><description>Then：預期拋出 <see cref="ArgumentNullException"/>。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "建立信件 - 收件者為 null 應拋出異常")]
    public void Constructor_WithNullTo_Throws()
    {
        // Arrange
        var subject = new EmailSubject("Hello");
        var body = new EmailBody("World");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EmailMessage(null!, subject, body));
    }

    /// <summary>
    /// 驗證主旨為 null 時，建立 <see cref="EmailMessage"/> 是否拋出 <see cref="ArgumentNullException"/>。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：主旨為 null，收件者與內文合法。</description></item>
    /// <item><description>When：建立 <see cref="EmailMessage"/>。</description></item>
    /// <item><description>Then：預期拋出 <see cref="ArgumentNullException"/>。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "建立信件 - 主旨為 null 應拋出異常")]
    public void Constructor_WithNullSubject_Throws()
    {
        // Arrange
        var to = new EmailAddress("user@example.com");
        var body = new EmailBody("World");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EmailMessage(to, null!, body));
    }

    /// <summary>
    /// 驗證內文為 null 時，建立 <see cref="EmailMessage"/> 是否拋出 <see cref="ArgumentNullException"/>。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：內文為 null，收件者與主旨合法。</description></item>
    /// <item><description>When：建立 <see cref="EmailMessage"/>。</description></item>
    /// <item><description>Then：預期拋出 <see cref="ArgumentNullException"/>。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "建立信件 - 內文為 null 應拋出異常")]
    public void Constructor_WithNullBody_Throws()
    {
        // Arrange
        var to = new EmailAddress("user@example.com");
        var subject = new EmailSubject("Hello");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EmailMessage(to, subject, null!));
    }
}
