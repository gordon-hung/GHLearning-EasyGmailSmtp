using System.Net;
using System.Net.Http.Json;
using GHLearning.EasyGmailSmtp.Application.Abstractions;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace GHLearning.EasyGmailSmtp.WebApi.Tests;

public class EmailEndpointsTests
{
    private static WebApplicationFactory<Program> CreateFactory(IEmailSender sender) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => sender);
            });
        });

    private static readonly object ValidRequest = new
    {
        to = "user@example.com",
        subject = "Hello",
        body = "World"
    };

    /// <summary>
    /// 驗證寄件者成功寄送時，端點是否回傳 <see cref="HttpStatusCode.OK"/>。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：一個會成功寄送的 mock <see cref="IEmailSender"/> 與合法請求。</description></item>
    /// <item><description>When：POST /api/emails。</description></item>
    /// <item><description>Then：回應狀態碼為 200 OK。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "POST 寄信 - 寄送成功應回傳 200")]
    public async Task Post_WhenSenderSucceeds_Returns200()
    {
        // Arrange
        var sender = Substitute.For<IEmailSender>();
        using var factory = CreateFactory(sender);
        using var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/emails", ValidRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// 驗證寄件者拋出基礎設施例外時，端點是否回傳 <see cref="HttpStatusCode.BadGateway"/>（而非未處理錯誤）。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：SendAsync 會拋出 <see cref="TimeoutException"/> 的 mock 寄件者與合法請求。</description></item>
    /// <item><description>When：POST /api/emails。</description></item>
    /// <item><description>Then：例外不外洩，回應狀態碼為 502 Bad Gateway。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "POST 寄信 - 基礎設施錯誤應回傳 502")]
    public async Task Post_WhenSenderThrowsInfrastructureError_Returns502_NotUnhandled()
    {
        // Arrange
        var sender = Substitute.For<IEmailSender>();
        sender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new TimeoutException("Operation timed out after 120000 milliseconds"));
        using var factory = CreateFactory(sender);
        using var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/emails", ValidRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    /// <summary>
    /// 驗證請求含非法電子郵件地址時，端點是否回傳 <see cref="HttpStatusCode.BadRequest"/>。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Given：收件者為非法地址 "not-an-email" 的請求。</description></item>
    /// <item><description>When：POST /api/emails。</description></item>
    /// <item><description>Then：回應狀態碼為 400 Bad Request。</description></item>
    /// </list>
    /// </remarks>
    [Fact(DisplayName = "POST 寄信 - 非法地址應回傳 400")]
    public async Task Post_WithInvalidEmail_Returns400()
    {
        // Arrange
        var sender = Substitute.For<IEmailSender>();
        using var factory = CreateFactory(sender);
        using var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/emails",
            new { to = "not-an-email", subject = "x", body = "y" },
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
