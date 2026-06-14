using GHLearning.EasyGmailSmtp.Application.SendEmail;
using Microsoft.Extensions.DependencyInjection;

namespace GHLearning.EasyGmailSmtp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<SendEmailCommandHandler>();
        return services;
    }
}
