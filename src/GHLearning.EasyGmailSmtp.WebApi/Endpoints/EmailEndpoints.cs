using GHLearning.EasyGmailSmtp.Application.SendEmail;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;
using GHLearning.EasyGmailSmtp.WebApi.Contracts;

namespace GHLearning.EasyGmailSmtp.WebApi.Endpoints;

public static class EmailEndpoints
{
    public static IEndpointRouteBuilder MapEmailEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/emails").WithTags("Emails");

        group.MapPost("/", async (
            SendEmailRequest request,
            SendEmailCommandHandler handler,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await handler.HandleAsync(
                    new SendEmailCommand(request.To, request.Subject, request.Body),
                    cancellationToken).ConfigureAwait(false);

                return Results.Ok(new SendEmailResponse(true, null));
            }
            catch (EmailValidationException ex)
            {
                // 任何欄位驗證失敗（地址 / 主旨 / 內文）統一回傳 400；
                // 新增驗證例外只要繼承 EmailValidationException 即自動納入，不會被誤判為 502
                return Results.BadRequest(new SendEmailResponse(false, ex.Message));
            }
            catch (OperationCanceledException)
            {
                // 用戶端中斷請求；交由框架處理，不視為伺服器錯誤
                throw;
            }
            catch (Exception ex)
            {
                // 寄信基礎設施失敗（SMTP 逾時、認證失敗、連線中斷等）
                // 不讓例外外洩成未處理錯誤，回傳乾淨的 502 並記錄完整細節
                loggerFactory
                    .CreateLogger(typeof(EmailEndpoints))
                    .LogError(ex, "Failed to send email to {Recipient}", request.To);

                return Results.Json(
                    new SendEmailResponse(false, "Failed to send the email. Please check the server logs."),
                    statusCode: StatusCodes.Status502BadGateway);
            }
        })
        .WithName("SendEmail")
        .Produces<SendEmailResponse>(StatusCodes.Status200OK)
        .Produces<SendEmailResponse>(StatusCodes.Status400BadRequest)
        .Produces<SendEmailResponse>(StatusCodes.Status502BadGateway);

        return app;
    }
}
