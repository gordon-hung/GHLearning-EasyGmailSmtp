namespace GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;

/// <summary>
/// 所有電子郵件欄位驗證例外的共同基底。
/// 端點可統一捕捉此型別並回傳 400，新增驗證例外時不會被誤判為 502。
/// </summary>
public abstract class EmailValidationException : Exception
{
    protected EmailValidationException(string message) : base(message) { }
}
