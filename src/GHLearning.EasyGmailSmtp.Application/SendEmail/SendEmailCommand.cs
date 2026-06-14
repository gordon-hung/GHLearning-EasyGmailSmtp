namespace GHLearning.EasyGmailSmtp.Application.SendEmail;

public sealed record SendEmailCommand(string To, string Subject, string Body);
