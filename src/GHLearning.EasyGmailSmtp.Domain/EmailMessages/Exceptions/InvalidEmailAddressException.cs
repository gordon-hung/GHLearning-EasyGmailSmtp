namespace GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;

public sealed class InvalidEmailAddressException : EmailValidationException
{
    public InvalidEmailAddressException(string message) : base(message) { }
}
