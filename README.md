# GHLearning.EasyGmailSmtp

一個以 **.NET 10 / ASP.NET Core Minimal API** 實作的輕量電子郵件發送服務，採用 **Clean Architecture** 分層設計。
透過 Google Gmail SMTP 寄出純文字信件，唯一的對外端點是 `POST /api/emails`。

---

## ✨ 特色

- **Clean Architecture 四層分層**：Domain / Application / Infrastructure / WebApi，相依方向由外而內。
- **Minimal API**：無控制器，端點以路由群組註冊。
- **領域驗證集中於值物件**：地址、主旨、內文各自封裝驗證規則。
- **明確的錯誤語意**：驗證失敗回 `400`、基礎設施（SMTP）失敗回 `502`，例外不外洩。
- **完整測試覆蓋**：四層皆有對應測試專案，含 `WebApplicationFactory` 整合測試。
- **嚴格編譯規範**：`TreatWarningsAsErrors`、`Nullable enable`、`LangVersion latest`。

---

## 🏗️ 架構

```
GHLearning.EasyGmailSmtp/
├── src/
│   ├── GHLearning.EasyGmailSmtp.Domain/          # 值物件與領域例外（零外部相依）
│   ├── GHLearning.EasyGmailSmtp.Application/     # Use Case：命令 / 處理器 / 抽象介面
│   ├── GHLearning.EasyGmailSmtp.Infrastructure/  # SMTP 實作（MailKit / MimeKit）
│   └── GHLearning.EasyGmailSmtp.WebApi/          # Minimal API 進入點、端點、合約
└── tests/
    ├── GHLearning.EasyGmailSmtp.Domain.Tests/
    ├── GHLearning.EasyGmailSmtp.Application.Tests/
    ├── GHLearning.EasyGmailSmtp.Infrastructure.Tests/
    └── GHLearning.EasyGmailSmtp.WebApi.Tests/    # 整合測試（WebApplicationFactory<Program>）
```

| 層 | 職責 | 關鍵型別 |
|----|------|----------|
| **Domain** | 純領域模型與驗證 | `EmailAddress`、`EmailSubject`、`EmailBody`、`EmailMessage` |
| **Application** | 協調 Use Case | `IEmailSender`、`SendEmailCommand`、`SendEmailCommandHandler` |
| **Infrastructure** | 對外整合 | `SmtpOptions`、`ISmtpClientFactory`、`GoogleSmtpEmailSender` |
| **WebApi** | HTTP 端點與合約 | `EmailEndpoints`、`SendEmailRequest`、`SendEmailResponse` |

> Application 層不直接相依 Infrastructure，`IEmailSender` 透過 DI 注入。
> `SmtpClient` 為有狀態連線資源，由 `ISmtpClientFactory` 在每次寄信時建立並以 `using` 確定性釋放。

---

## 🚀 快速開始

### 先決條件

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- 一組 Gmail 帳號與 **App Password**（16 碼應用程式密碼）

### 設定憑證

編輯 `src/GHLearning.EasyGmailSmtp.WebApi/appsettings.json` 的 `Smtp` 區段：

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

> [!IMPORTANT]
> - `Password` 必須是 Google **App Password**（16 碼），而非 Gmail 登入密碼。
>   Google 自 2024 年起已停止接受一般密碼登入 SMTP。
> - `appsettings.json` 含憑證，**請勿提交至版本控制**。

#### Gmail SMTP Port 與 SSL 必須成對

| Port | UseSsl | 連線方式 |
|------|--------|----------|
| `465` | `true`  | `SslOnConnect`（建議） |
| `587` | `false` | `StartTls` |

> 混用（例如 Port `587` + UseSsl `true`）會造成 `SslHandshakeException`，兩者必須成對設定。

### 建置與執行

```bash
# 還原並建置整個方案
dotnet build

# 啟動 API（含 Swagger UI）
dotnet run --project src/GHLearning.EasyGmailSmtp.WebApi
```

啟動後，於開發環境開啟 Swagger UI：`http://localhost:<port>/swagger`

---

## 📮 API

### `POST /api/emails`

寄出一封純文字電子郵件。

**Request Body**

```json
{
  "to": "recipient@example.com",
  "subject": "Hello from GHLearning.EasyGmailSmtp",
  "body": "This is a plain text email."
}
```

**回應**

| 狀態碼 | 意義 | Body |
|--------|------|------|
| `200 OK` | 寄送成功 | `{ "success": true, "error": null }` |
| `400 Bad Request` | 欄位驗證失敗（地址 / 主旨 / 內文） | `{ "success": false, "error": "<原因>" }` |
| `502 Bad Gateway` | SMTP 等基礎設施錯誤 | `{ "success": false, "error": "Failed to send the email. Please check the server logs." }` |

> `502` 時不會洩漏內部例外細節，完整錯誤會記錄至 logger。

**範例（cURL）**

```bash
curl -X POST http://localhost:<port>/api/emails \
  -H "Content-Type: application/json" \
  -d '{
    "to": "recipient@example.com",
    "subject": "Hello",
    "body": "Hello, World!"
  }'
```

---

## ✅ 驗證規則

| 值物件 | 規則 | 失敗例外 |
|--------|------|----------|
| `EmailAddress` | 以 `MailAddress` 解析；host 須含 `.` | `InvalidEmailAddressException` |
| `EmailSubject` | 不可空白；最大 200 字元 | `InvalidEmailSubjectException` |
| `EmailBody` | 不可空白 | `InvalidEmailBodyException` |

所有驗證例外皆繼承自 `EmailValidationException`，端點據此統一回傳 `400`。

---

## 🧪 測試

```bash
# 執行所有測試
dotnet test

# 僅執行特定層
dotnet test tests/GHLearning.EasyGmailSmtp.Domain.Tests
dotnet test tests/GHLearning.EasyGmailSmtp.Application.Tests
dotnet test tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests
dotnet test tests/GHLearning.EasyGmailSmtp.WebApi.Tests
```

| 套件 | 用途 |
|------|------|
| `xunit.v3` | 測試框架 |
| `NSubstitute` | Mock 框架 |
| `Microsoft.AspNetCore.Mvc.Testing` | WebApi 整合測試 |

整合測試使用 `WebApplicationFactory<Program>`，並透過 `UseEnvironment("Testing")` 替換 DI 服務。

---

## 🛠️ 技術棧

- **.NET 10** / ASP.NET Core Minimal API
- **MailKit / MimeKit** — SMTP 連線與郵件組裝
- **Microsoft.AspNetCore.OpenApi** + **Swashbuckle.AspNetCore.SwaggerUI** — OpenAPI 文件與 Swagger UI

---

## 📐 專案慣例

- `TreatWarningsAsErrors = true`：所有警告視為錯誤。
- `Nullable = enable`：啟用可為 null 的參考型別分析。
- `LangVersion = latest`：使用最新 C# 語言版本。
- 所有 public 類別預設為 `sealed`，除非明確需要繼承。
- 不使用 MediatR 等中介框架；命令處理器直接由 DI 注入至端點。
