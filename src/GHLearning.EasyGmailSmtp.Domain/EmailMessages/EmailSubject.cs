using GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;

namespace GHLearning.EasyGmailSmtp.Domain.EmailMessages;

public sealed record EmailSubject
{
    public const int MaxLength = 200;

    public string Value { get; }

    public EmailSubject(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidEmailSubjectException("Email subject cannot be empty.");

        if (value.Length > MaxLength)
            throw new InvalidEmailSubjectException(
                $"Email subject must not exceed {MaxLength} characters.");

        Value = value;
    }

    public override string ToString() => Value;
}
