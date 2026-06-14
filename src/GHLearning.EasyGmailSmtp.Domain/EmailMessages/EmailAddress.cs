using System.Net.Mail;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;

namespace GHLearning.EasyGmailSmtp.Domain.EmailMessages;

public sealed record EmailAddress
{
    public string Value { get; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidEmailAddressException("Email address cannot be empty.");

        try
        {
            var parsed = new MailAddress(value);
            if (!string.Equals(parsed.Address, value, StringComparison.OrdinalIgnoreCase)
                || !parsed.Host.Contains('.'))
                throw new InvalidEmailAddressException($"'{value}' is not a valid email address.");
        }
        catch (FormatException)
        {
            throw new InvalidEmailAddressException($"'{value}' is not a valid email address.");
        }

        Value = value;
    }

    public override string ToString() => Value;
}
