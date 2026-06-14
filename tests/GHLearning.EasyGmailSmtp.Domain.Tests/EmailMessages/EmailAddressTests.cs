using GHLearning.EasyGmailSmtp.Domain.EmailMessages;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;
using Xunit;

namespace GHLearning.EasyGmailSmtp.Domain.Tests.EmailMessages;

public class EmailAddressTests
{
    /// <summary>
    /// 驗證以合法電子郵件地址建立 <see cref="EmailAddress"/> 時，是否正確保存原始值。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：合法地址字串 "user@example.com"。</description></item>
    /// <item><description>When：建立 <see cref="EmailAddress"/>。</description></item>
    /// <item><description>Then：<see cref="EmailAddress.Value"/> 等於原始輸入。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "建立地址 - 合法地址應保存原始值")]
    public void Constructor_WithValidAddress_SetsValue()
    {
        // Arrange
        const string input = "user@example.com";

        // Act
        var address = new EmailAddress(input);

        // Assert
        Assert.Equal(input, address.Value);
    }

    /// <summary>
    /// 驗證以空字串或純空白建立 <see cref="EmailAddress"/> 時，是否拋出 <see cref="InvalidEmailAddressException"/>。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：空字串或僅含空白的輸入。</description></item>
    /// <item><description>When：建立 <see cref="EmailAddress"/>。</description></item>
    /// <item><description>Then：預期拋出地址無效異常。</description></item>
    /// </list>
    /// </remarks>
    [Theory(DisplayName = "建立地址 - 空白輸入應拋出異常")]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespace_Throws(string input)
    {
        // Act & Assert
        Assert.Throws<InvalidEmailAddressException>(() => new EmailAddress(input));
    }

    /// <summary>
    /// 驗證以格式錯誤的字串建立 <see cref="EmailAddress"/> 時，是否拋出 <see cref="InvalidEmailAddressException"/>。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：缺少網域、缺少頂級網域或缺少本地部分的非法地址。</description></item>
    /// <item><description>When：建立 <see cref="EmailAddress"/>。</description></item>
    /// <item><description>Then：預期拋出地址無效異常。</description></item>
    /// </list>
    /// </remarks>
    [Theory(DisplayName = "建立地址 - 格式錯誤應拋出異常")]
    [InlineData("not-an-email")]
    [InlineData("missing@tld")]
    [InlineData("@no-local.com")]
    public void Constructor_WithMalformedAddress_Throws(string input)
    {
        // Act & Assert
        Assert.Throws<InvalidEmailAddressException>(() => new EmailAddress(input));
    }

    /// <summary>
    /// 驗證以 null 建立 <see cref="EmailAddress"/> 時，是否拋出 <see cref="InvalidEmailAddressException"/>。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：null 輸入。</description></item>
    /// <item><description>When：建立 <see cref="EmailAddress"/>。</description></item>
    /// <item><description>Then：預期拋出地址無效異常。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "建立地址 - null 應拋出異常")]
    public void Constructor_WithNull_Throws()
    {
        // Act & Assert
        Assert.Throws<InvalidEmailAddressException>(() => new EmailAddress(null!));
    }

    /// <summary>
    /// 驗證兩個值相同的 <see cref="EmailAddress"/> 是否視為相等（record 值相等語義）。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：以相同地址字串建立的兩個 <see cref="EmailAddress"/> 實例。</description></item>
    /// <item><description>When：比較兩者是否相等。</description></item>
    /// <item><description>Then：兩者相等。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "地址相等 - 相同值應視為相等")]
    public void Equality_SameValue_AreEqual()
    {
        // Arrange
        var a = new EmailAddress("user@example.com");
        var b = new EmailAddress("user@example.com");

        // Act & Assert
        Assert.Equal(a, b);
    }
}
