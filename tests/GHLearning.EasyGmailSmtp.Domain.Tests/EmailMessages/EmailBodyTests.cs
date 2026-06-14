using GHLearning.EasyGmailSmtp.Domain.EmailMessages;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;
using Xunit;

namespace GHLearning.EasyGmailSmtp.Domain.Tests.EmailMessages;

public class EmailBodyTests
{
    /// <summary>
    /// 驗證以合法內文建立 <see cref="EmailBody"/> 時，是否正確保存原始值。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：合法內文字串 "Hello world"。</description></item>
    /// <item><description>When：建立 <see cref="EmailBody"/>。</description></item>
    /// <item><description>Then：<see cref="EmailBody.Value"/> 等於原始輸入。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "建立內文 - 合法內文應保存原始值")]
    public void Constructor_WithValidBody_SetsValue()
    {
        // Arrange
        const string input = "Hello world";

        // Act
        var body = new EmailBody(input);

        // Assert
        Assert.Equal(input, body.Value);
    }

    /// <summary>
    /// 驗證以空字串或純空白建立 <see cref="EmailBody"/> 時，是否拋出 <see cref="InvalidEmailBodyException"/>。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：空字串或僅含空白的輸入。</description></item>
    /// <item><description>When：建立 <see cref="EmailBody"/>。</description></item>
    /// <item><description>Then：預期拋出內文無效異常。</description></item>
    /// </list>
    /// </remarks>
    [Theory(DisplayName = "建立內文 - 空白輸入應拋出異常")]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespace_Throws(string input)
    {
        // Act & Assert
        Assert.Throws<InvalidEmailBodyException>(() => new EmailBody(input));
    }

    /// <summary>
    /// 驗證以 null 建立 <see cref="EmailBody"/> 時，是否拋出 <see cref="InvalidEmailBodyException"/>。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：null 輸入。</description></item>
    /// <item><description>When：建立 <see cref="EmailBody"/>。</description></item>
    /// <item><description>Then：預期拋出內文無效異常。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "建立內文 - null 應拋出異常")]
    public void Constructor_WithNull_Throws()
    {
        // Act & Assert
        Assert.Throws<InvalidEmailBodyException>(() => new EmailBody(null!));
    }
}
