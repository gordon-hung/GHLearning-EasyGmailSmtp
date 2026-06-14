using GHLearning.EasyGmailSmtp.Domain.EmailMessages;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;
using Xunit;

namespace GHLearning.EasyGmailSmtp.Domain.Tests.EmailMessages;

public class EmailSubjectTests
{
    /// <summary>
    /// 驗證以合法主旨建立 <see cref="EmailSubject"/> 時，是否正確保存原始值。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：合法主旨字串 "Hello"。</description></item>
    /// <item><description>When：建立 <see cref="EmailSubject"/>。</description></item>
    /// <item><description>Then：<see cref="EmailSubject.Value"/> 等於原始輸入。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "建立主旨 - 合法主旨應保存原始值")]
    public void Constructor_WithValidSubject_SetsValue()
    {
        // Arrange
        const string input = "Hello";

        // Act
        var subject = new EmailSubject(input);

        // Assert
        Assert.Equal(input, subject.Value);
    }

    /// <summary>
    /// 驗證以空字串或純空白建立 <see cref="EmailSubject"/> 時，是否拋出 <see cref="InvalidEmailSubjectException"/>。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：空字串或僅含空白的輸入。</description></item>
    /// <item><description>When：建立 <see cref="EmailSubject"/>。</description></item>
    /// <item><description>Then：預期拋出主旨無效異常。</description></item>
    /// </list>
    /// </remarks>
    [Theory(DisplayName = "建立主旨 - 空白輸入應拋出異常")]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespace_Throws(string input)
    {
        // Act & Assert
        Assert.Throws<InvalidEmailSubjectException>(() => new EmailSubject(input));
    }

    /// <summary>
    /// 驗證以 null 建立 <see cref="EmailSubject"/> 時，是否拋出 <see cref="InvalidEmailSubjectException"/>。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：null 輸入。</description></item>
    /// <item><description>When：建立 <see cref="EmailSubject"/>。</description></item>
    /// <item><description>Then：預期拋出主旨無效異常。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "建立主旨 - null 應拋出異常")]
    public void Constructor_WithNull_Throws()
    {
        // Act & Assert
        Assert.Throws<InvalidEmailSubjectException>(() => new EmailSubject(null!));
    }

    /// <summary>
    /// 驗證主旨長度超過 <see cref="EmailSubject.MaxLength"/> 上限時，是否拋出 <see cref="InvalidEmailSubjectException"/>。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：長度為上限再加 1 的主旨字串。</description></item>
    /// <item><description>When：建立 <see cref="EmailSubject"/>。</description></item>
    /// <item><description>Then：預期拋出主旨無效異常。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "建立主旨 - 超過長度上限應拋出異常")]
    public void Constructor_ExceedingMaxLength_Throws()
    {
        // Arrange
        var tooLong = new string('a', EmailSubject.MaxLength + 1);

        // Act & Assert
        Assert.Throws<InvalidEmailSubjectException>(() => new EmailSubject(tooLong));
    }

    /// <summary>
    /// 驗證主旨長度恰為 <see cref="EmailSubject.MaxLength"/> 上限（邊界值）時，是否成功建立。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：長度恰等於上限的主旨字串。</description></item>
    /// <item><description>When：建立 <see cref="EmailSubject"/>。</description></item>
    /// <item><description>Then：成功建立，且值長度等於上限。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "建立主旨 - 長度等於上限應成功建立")]
    public void Constructor_AtMaxLength_Succeeds()
    {
        // Arrange
        var atLimit = new string('a', EmailSubject.MaxLength);

        // Act
        var subject = new EmailSubject(atLimit);

        // Assert
        Assert.Equal(EmailSubject.MaxLength, subject.Value.Length);
    }
}
