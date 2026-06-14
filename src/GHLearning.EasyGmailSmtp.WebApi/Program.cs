using GHLearning.EasyGmailSmtp.Application;
using GHLearning.EasyGmailSmtp.Infrastructure;
using GHLearning.EasyGmailSmtp.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "GHLearning.EasyGmailSmtp API v1");
    });
}

app.MapEmailEndpoints();

app.Run();

// 讓整合測試（WebApplicationFactory<Program>）可引用此進入點
public partial class Program;
