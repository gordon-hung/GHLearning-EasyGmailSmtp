using GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;

namespace GHLearning.EasyGmailSmtp.Domain.EmailMessages;

public sealed record EmailBody
{
    public string Value { get; }

    public EmailBody(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidEmailBodyException("Email body cannot be empty.");

        Value = value;
    }

    public override string ToString() => Value;
}
