# CLAUDE.md — GHLearning.EasyGmailSmtp

## 專案概述

一個以 .NET 10 / ASP.NET Core Minimal API 實作的電子郵件發送服務，採用 Clean Architecture 分層。
唯一的 Use Case 是 `POST /api/emails`，透過 Google Gmail SMTP 寄出純文字信件。

---

## 目錄結構

```
GHLearning.EasyGmailSmtp/
├── src/
│   ├── GHLearning.EasyGmailSmtp.Domain/          # 值物件與領域例外
│   ├── GHLearning.EasyGmailSmtp.Application/     # Use Case（命令 / 處理器 / 抽象介面）
│   ├── GHLearning.EasyGmailSmtp.Infrastructure/  # SMTP 實作（MailKit）
│   └── GHLearning.EasyGmailSmtp.WebApi/          # Minimal API 進入點、端點、合約
└── tests/
    ├── GHLearning.EasyGmailSmtp.Domain.Tests/
    ├── GHLearning.EasyGmailSmtp.Application.Tests/
    ├── GHLearning.EasyGmailSmtp.Infrastructure.Tests/
    └── GHLearning.EasyGmailSmtp.WebApi.Tests/    # 整合測試（WebApplicationFactory）
```

---

## 常用指令

```bash
# 建置整個方案
dotnet build

# 執行所有測試
dotnet test

# 僅執行特定層的測試
dotnet test tests/GHLearning.EasyGmailSmtp.Domain.Tests
dotnet test tests/GHLearning.EasyGmailSmtp.Application.Tests
dotnet test tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests
dotnet test tests/GHLearning.EasyGmailSmtp.WebApi.Tests

# 啟動 API（含 Swagger UI）
dotnet run --project src/GHLearning.EasyGmailSmtp.WebApi
```

API 啟動後 Swagger UI 位於 `http://localhost:<port>/swagger`。

---

## 架構層說明

### Domain（`GHLearning.EasyGmailSmtp.Domain`）

純領域模型，零外部相依。

| 型別 | 用途 |
|------|------|
| `EmailAddress` | 驗證格式；使用 `MailAddress` 解析並要求含 `.` 的 host |
| `EmailSubject` | 不可空白；最大 200 字元 |
| `EmailBody` | 不可空白 |
| `EmailMessage` | 三個值物件的聚合根 |

驗證失敗拋出對應的 `Invalid*Exception`（繼承自 `Exception`，非 `ApplicationException`）。

### Application（`GHLearning.EasyGmailSmtp.Application`）

| 型別 | 用途 |
|------|------|
| `IEmailSender` | 基礎設施抽象介面 |
| `SendEmailCommand` | 記錄型別（To / Subject / Body 字串） |
| `SendEmailCommandHandler` | 將字串轉成領域物件後呼叫 `IEmailSender` |

Application 層不直接相依 Infrastructure；透過 DI 注入 `IEmailSender`。

### Infrastructure（`GHLearning.EasyGmailSmtp.Infrastructure`）

| 型別 | 用途 |
|------|------|
| `SmtpOptions` | 對應 `appsettings.json` 的 `Smtp` 區段 |
| `GoogleSmtpEmailSender` | 使用 MailKit 連線 Gmail SMTP |

**Gmail SMTP 設定規則（Port 與 SSL 必須搭配）：**

| Port | UseSsl | 連線方式 |
|------|--------|----------|
| `465` | `true` | `SslOnConnect`（建議） |
| `587` | `false` | `StartTls` |

混用（例如 Port 587 + UseSsl true）會造成 `SslHandshakeException`，**兩者必須成對設定**。

### WebApi（`GHLearning.EasyGmailSmtp.WebApi`）

- Minimal API，路由群組：`/api/emails`
- 端點：`POST /api/emails`
- 回應型別：`200 OK`、`400 Bad Request`、`502 Bad Gateway`
- SMTP 等基礎設施錯誤回傳 502，不讓例外外洩；完整錯誤記錄至 logger

---

## 設定檔

### `appsettings.json`（`src/GHLearning.EasyGmailSmtp.WebApi/`）

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 465,
    "UseSsl": true,
    "RequireAuthentication": true,
    "Username": "your-account@gmail.com",
    "Password": "your-16-char-app-password",
    "FromAddress": "your-account@gmail.com",
    "FromName": "Easy Email"
  }
}
```

**重要：**
- `Password` 必須填入 Google **App Password**（16 碼），不是 Gmail 登入密碼。
  Google 自 2024 年起已停止接受一般密碼登入 SMTP。
- `appsettings.json` 包含憑證，**不應提交至版本控制**。

---

## 測試框架

| 套件 | 用途 |
|------|------|
| `xunit.v3` | 測試框架 |
| `NSubstitute` | Mock 框架 |
| `Microsoft.AspNetCore.Mvc.Testing` | WebApi 整合測試 |

整合測試使用 `WebApplicationFactory<Program>`，並透過 `UseEnvironment("Testing")` 替換 DI 服務。

---

## 專案慣例

- `TreatWarningsAsErrors = true`：所有警告視為錯誤，不得忽略
- `Nullable = enable`：啟用可為 null 的參考型別分析
- `LangVersion = latest`：使用最新 C# 語言版本
- 所有 public 類別皆為 `sealed`，除非明確需要繼承
- 不使用 MediatR 或其他中介框架；命令處理器直接由 DI 注入至端點
