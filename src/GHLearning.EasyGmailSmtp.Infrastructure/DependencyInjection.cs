using GHLearning.EasyGmailSmtp.Application.Abstractions;
using GHLearning.EasyGmailSmtp.Infrastructure.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GHLearning.EasyGmailSmtp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));

        // 有狀態的 SmtpClient 不再以 DI 生命週期管理，改由工廠在每次寄信時建立並釋放。
        // 工廠本身無狀態，故註冊為 Singleton；sender 為 Transient。
        services.AddSingleton<ISmtpClientFactory, SmtpClientFactory>();
        services.AddTransient<IEmailSender, GoogleSmtpEmailSender>();
        return services;
    }
}
