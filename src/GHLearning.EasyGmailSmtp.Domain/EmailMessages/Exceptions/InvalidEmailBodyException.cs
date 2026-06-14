namespace GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;

public sealed class InvalidEmailBodyException : EmailValidationException
{
    public InvalidEmailBodyException(string message) : base(message) { }
}
