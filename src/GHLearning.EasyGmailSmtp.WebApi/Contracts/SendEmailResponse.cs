namespace GHLearning.EasyGmailSmtp.WebApi.Contracts;

public sealed record SendEmailResponse(bool Success, string? Error);
