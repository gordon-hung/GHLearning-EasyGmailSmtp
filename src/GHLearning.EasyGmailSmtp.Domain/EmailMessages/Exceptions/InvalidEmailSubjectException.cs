namespace GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;

public sealed class InvalidEmailSubjectException : EmailValidationException
{
    public InvalidEmailSubjectException(string message) : base(message) { }
}
