# Google SMTP Email API Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 提供一支 Minimal API endpoint `POST /api/emails`，接收 `to / subject / body` 並透過 Google SMTP（MailKit）寄出純文字郵件。

**Architecture:** 採 DDD + Clean Architecture 4 層（Domain / Application / Infrastructure / WebApi）。Domain 以 Value Objects（`EmailAddress`、`EmailSubject`、`EmailBody`）與 Aggregate（`EmailMessage`）封裝不變條件；Application 定義 `IEmailSender` outbound port 與 `SendEmailCommandHandler`；Infrastructure 用 MailKit 的 `ISmtpClient` 實作 `GoogleSmtpEmailSender`；WebApi 用 Minimal API 暴露 endpoint。

**Tech Stack:**
- C# 14 / .NET 10 (`net10.0`)
- Minimal API
- MailKit 4.x（Google SMTP）
- `Microsoft.Extensions.Options` 10.x
- xUnit v3 3.2.2、`xunit.runner.visualstudio` 3.1.5、`Microsoft.NET.Test.Sdk` 17.x
- NSubstitute 5.3.0

---

## File Structure

```
GHLearning-GHLearning.EasyGmailSmtpGoogle/
├── GHLearning.EasyGmailSmtp.sln
├── .gitignore
├── src/
│   ├── GHLearning.EasyGmailSmtp.Domain/
│   │   ├── GHLearning.EasyGmailSmtp.Domain.csproj
│   │   └── EmailMessages/
│   │       ├── EmailAddress.cs
│   │       ├── EmailSubject.cs
│   │       ├── EmailBody.cs
│   │       ├── EmailMessage.cs
│   │       └── Exceptions/
│   │           ├── InvalidEmailAddressException.cs
│   │           ├── InvalidEmailSubjectException.cs
│   │           └── InvalidEmailBodyException.cs
│   ├── GHLearning.EasyGmailSmtp.Application/
│   │   ├── GHLearning.EasyGmailSmtp.Application.csproj
│   │   ├── Abstractions/
│   │   │   └── IEmailSender.cs
│   │   ├── SendEmail/
│   │   │   ├── SendEmailCommand.cs
│   │   │   └── SendEmailCommandHandler.cs
│   │   └── DependencyInjection.cs
│   ├── GHLearning.EasyGmailSmtp.Infrastructure/
│   │   ├── GHLearning.EasyGmailSmtp.Infrastructure.csproj
│   │   ├── Email/
│   │   │   ├── SmtpOptions.cs
│   │   │   └── GoogleSmtpEmailSender.cs
│   │   └── DependencyInjection.cs
│   └── GHLearning.EasyGmailSmtp.WebApi/
│       ├── GHLearning.EasyGmailSmtp.WebApi.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Contracts/
│       │   ├── SendEmailRequest.cs
│       │   └── SendEmailResponse.cs
│       └── Endpoints/
│           └── EmailEndpoints.cs
├── tests/
│   ├── GHLearning.EasyGmailSmtp.Domain.Tests/
│   │   ├── GHLearning.EasyGmailSmtp.Domain.Tests.csproj
│   │   └── EmailMessages/
│   │       ├── EmailAddressTests.cs
│   │       ├── EmailSubjectTests.cs
│   │       ├── EmailBodyTests.cs
│   │       └── EmailMessageTests.cs
│   ├── GHLearning.EasyGmailSmtp.Application.Tests/
│   │   ├── GHLearning.EasyGmailSmtp.Application.Tests.csproj
│   │   └── SendEmail/
│   │       └── SendEmailCommandHandlerTests.cs
│   └── GHLearning.EasyGmailSmtp.Infrastructure.Tests/
│       ├── GHLearning.EasyGmailSmtp.Infrastructure.Tests.csproj
│       └── Email/
│           └── GoogleSmtpEmailSenderTests.cs
└── docs/superpowers/plans/2026-05-29-google-smtp-email-api.md
```

---

## Task 1: 建立 Solution、Git Repo 與專案骨架

**Files:**
- Create: `GHLearning.EasyGmailSmtp.sln`
- Create: `.gitignore`
- Create: `src/GHLearning.EasyGmailSmtp.Domain/GHLearning.EasyGmailSmtp.Domain.csproj`
- Create: `src/GHLearning.EasyGmailSmtp.Application/GHLearning.EasyGmailSmtp.Application.csproj`
- Create: `src/GHLearning.EasyGmailSmtp.Infrastructure/GHLearning.EasyGmailSmtp.Infrastructure.csproj`
- Create: `src/GHLearning.EasyGmailSmtp.WebApi/GHLearning.EasyGmailSmtp.WebApi.csproj`
- Create: `tests/GHLearning.EasyGmailSmtp.Domain.Tests/GHLearning.EasyGmailSmtp.Domain.Tests.csproj`
- Create: `tests/GHLearning.EasyGmailSmtp.Application.Tests/GHLearning.EasyGmailSmtp.Application.Tests.csproj`
- Create: `tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests.csproj`

- [ ] **Step 1: 初始化 git repo 並建立 feature 分支**

PowerShell 工作目錄為 `C:\Users\gordon.hung_smartdai\Desktop\GHLearning-GHLearning.EasyGmailSmtpGoogle`。

```powershell
git init
git checkout -b feature/ghung/email-api
```

Expected：建立 `.git/` 與 `feature/ghung/email-api` 分支。後續所有 commit 都在此分支，避免觸發 PreToolUse 受保護分支鉤子。

- [ ] **Step 2: 加入 .gitignore（避開 bin/obj 與 secrets）**

Create `.gitignore`：

```
# .NET
bin/
obj/
*.user
*.suo
.vs/

# Rider / VS Code
.idea/
.vscode/

# Secrets
appsettings.*.json
!appsettings.json
!appsettings.Development.json
**/secrets.json
```

- [ ] **Step 3: 建立 Solution 與專案骨架**

```powershell
dotnet new sln -n GHLearning.EasyGmailSmtp
dotnet new classlib -n GHLearning.EasyGmailSmtp.Domain -o src/GHLearning.EasyGmailSmtp.Domain -f net10.0
dotnet new classlib -n GHLearning.EasyGmailSmtp.Application -o src/GHLearning.EasyGmailSmtp.Application -f net10.0
dotnet new classlib -n GHLearning.EasyGmailSmtp.Infrastructure -o src/GHLearning.EasyGmailSmtp.Infrastructure -f net10.0
dotnet new web -n GHLearning.EasyGmailSmtp.WebApi -o src/GHLearning.EasyGmailSmtp.WebApi -f net10.0
dotnet new classlib -n GHLearning.EasyGmailSmtp.Domain.Tests -o tests/GHLearning.EasyGmailSmtp.Domain.Tests -f net10.0
dotnet new classlib -n GHLearning.EasyGmailSmtp.Application.Tests -o tests/GHLearning.EasyGmailSmtp.Application.Tests -f net10.0
dotnet new classlib -n GHLearning.EasyGmailSmtp.Infrastructure.Tests -o tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests -f net10.0

Remove-Item src/GHLearning.EasyGmailSmtp.Domain/Class1.cs
Remove-Item src/GHLearning.EasyGmailSmtp.Application/Class1.cs
Remove-Item src/GHLearning.EasyGmailSmtp.Infrastructure/Class1.cs
Remove-Item tests/GHLearning.EasyGmailSmtp.Domain.Tests/Class1.cs
Remove-Item tests/GHLearning.EasyGmailSmtp.Application.Tests/Class1.cs
Remove-Item tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests/Class1.cs

dotnet sln add `
  src/GHLearning.EasyGmailSmtp.Domain/GHLearning.EasyGmailSmtp.Domain.csproj `
  src/GHLearning.EasyGmailSmtp.Application/GHLearning.EasyGmailSmtp.Application.csproj `
  src/GHLearning.EasyGmailSmtp.Infrastructure/GHLearning.EasyGmailSmtp.Infrastructure.csproj `
  src/GHLearning.EasyGmailSmtp.WebApi/GHLearning.EasyGmailSmtp.WebApi.csproj `
  tests/GHLearning.EasyGmailSmtp.Domain.Tests/GHLearning.EasyGmailSmtp.Domain.Tests.csproj `
  tests/GHLearning.EasyGmailSmtp.Application.Tests/GHLearning.EasyGmailSmtp.Application.Tests.csproj `
  tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests.csproj
```

Expected：產出 7 個 csproj 並加入 sln。

- [ ] **Step 4: 設定專案參考（依 Clean Architecture 內外向）**

```powershell
dotnet add src/GHLearning.EasyGmailSmtp.Application/GHLearning.EasyGmailSmtp.Application.csproj reference src/GHLearning.EasyGmailSmtp.Domain/GHLearning.EasyGmailSmtp.Domain.csproj
dotnet add src/GHLearning.EasyGmailSmtp.Infrastructure/GHLearning.EasyGmailSmtp.Infrastructure.csproj reference src/GHLearning.EasyGmailSmtp.Application/GHLearning.EasyGmailSmtp.Application.csproj
dotnet add src/GHLearning.EasyGmailSmtp.WebApi/GHLearning.EasyGmailSmtp.WebApi.csproj reference src/GHLearning.EasyGmailSmtp.Application/GHLearning.EasyGmailSmtp.Application.csproj
dotnet add src/GHLearning.EasyGmailSmtp.WebApi/GHLearning.EasyGmailSmtp.WebApi.csproj reference src/GHLearning.EasyGmailSmtp.Infrastructure/GHLearning.EasyGmailSmtp.Infrastructure.csproj

dotnet add tests/GHLearning.EasyGmailSmtp.Domain.Tests/GHLearning.EasyGmailSmtp.Domain.Tests.csproj reference src/GHLearning.EasyGmailSmtp.Domain/GHLearning.EasyGmailSmtp.Domain.csproj
dotnet add tests/GHLearning.EasyGmailSmtp.Application.Tests/GHLearning.EasyGmailSmtp.Application.Tests.csproj reference src/GHLearning.EasyGmailSmtp.Application/GHLearning.EasyGmailSmtp.Application.csproj
dotnet add tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests.csproj reference src/GHLearning.EasyGmailSmtp.Infrastructure/GHLearning.EasyGmailSmtp.Infrastructure.csproj
```

Expected：依賴方向僅向內（WebApi → Infrastructure/Application → Domain）。

- [ ] **Step 5: 為三個測試專案加入 xunit.v3 / NSubstitute 套件**

```powershell
foreach ($p in @(
  "tests/GHLearning.EasyGmailSmtp.Domain.Tests/GHLearning.EasyGmailSmtp.Domain.Tests.csproj",
  "tests/GHLearning.EasyGmailSmtp.Application.Tests/GHLearning.EasyGmailSmtp.Application.Tests.csproj",
  "tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests.csproj"
)) {
  dotnet add $p package xunit.v3 --version 3.2.2
  dotnet add $p package xunit.runner.visualstudio --version 3.1.5
  dotnet add $p package Microsoft.NET.Test.Sdk --version 17.12.0
  dotnet add $p package NSubstitute --version 5.3.0
}
```

並在每個測試 csproj 內手動補上 `<IsPackable>false</IsPackable>` 與 `<IsTestProject>true</IsTestProject>`（dotnet 不會自動加，xunit.runner.visualstudio 需要）：

Modify `tests/GHLearning.EasyGmailSmtp.Domain.Tests/GHLearning.EasyGmailSmtp.Domain.Tests.csproj`（其他兩個測試 csproj 同樣處理）— 在 `<PropertyGroup>` 內加入：

```xml
<TargetFramework>net10.0</TargetFramework>
<IsPackable>false</IsPackable>
<IsTestProject>true</IsTestProject>
<LangVersion>latest</LangVersion>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

- [ ] **Step 6: 為 src/* 啟用 Nullable 與 ImplicitUsings**

Modify `src/GHLearning.EasyGmailSmtp.Domain/GHLearning.EasyGmailSmtp.Domain.csproj`、`src/GHLearning.EasyGmailSmtp.Application/GHLearning.EasyGmailSmtp.Application.csproj`、`src/GHLearning.EasyGmailSmtp.Infrastructure/GHLearning.EasyGmailSmtp.Infrastructure.csproj`、`src/GHLearning.EasyGmailSmtp.WebApi/GHLearning.EasyGmailSmtp.WebApi.csproj` 的 `<PropertyGroup>`，確保包含：

```xml
<TargetFramework>net10.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
<LangVersion>latest</LangVersion>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

- [ ] **Step 7: 驗證整個 solution 可 build**

```powershell
dotnet build GHLearning.EasyGmailSmtp.sln
```

Expected：`Build succeeded`，0 errors、0 warnings。

- [ ] **Step 8: Commit 骨架**

```powershell
git add .gitignore GHLearning.EasyGmailSmtp.sln src tests docs
git commit -m "🤖 chore(scaffold): 建立 GHLearning.EasyGmailSmtp solution 與 DDD 四層骨架"
```

---

## Task 2: Domain — `EmailAddress` Value Object（TDD）

**Files:**
- Create: `src/GHLearning.EasyGmailSmtp.Domain/EmailMessages/Exceptions/InvalidEmailAddressException.cs`
- Create: `src/GHLearning.EasyGmailSmtp.Domain/EmailMessages/EmailAddress.cs`
- Test: `tests/GHLearning.EasyGmailSmtp.Domain.Tests/EmailMessages/EmailAddressTests.cs`

- [ ] **Step 1: 撰寫紅燈測試**

Create `tests/GHLearning.EasyGmailSmtp.Domain.Tests/EmailMessages/EmailAddressTests.cs`:

```csharp
using GHLearning.EasyGmailSmtp.Domain.EmailMessages;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;
using Xunit;

namespace GHLearning.EasyGmailSmtp.Domain.Tests.EmailMessages;

public class EmailAddressTests
{
    [Fact]
    public void Constructor_WithValidAddress_SetsValue()
    {
        var address = new EmailAddress("user@example.com");

        Assert.Equal("user@example.com", address.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespace_Throws(string input)
    {
        Assert.Throws<InvalidEmailAddressException>(() => new EmailAddress(input));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@tld")]
    [InlineData("@no-local.com")]
    public void Constructor_WithMalformedAddress_Throws(string input)
    {
        Assert.Throws<InvalidEmailAddressException>(() => new EmailAddress(input));
    }

    [Fact]
    public void Constructor_WithNull_Throws()
    {
        Assert.Throws<InvalidEmailAddressException>(() => new EmailAddress(null!));
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = new EmailAddress("user@example.com");
        var b = new EmailAddress("user@example.com");

        Assert.Equal(a, b);
    }
}
```

- [ ] **Step 2: 跑紅燈**

```powershell
dotnet test tests/GHLearning.EasyGmailSmtp.Domain.Tests/GHLearning.EasyGmailSmtp.Domain.Tests.csproj
```

Expected：FAIL — 找不到 `EmailAddress`/`InvalidEmailAddressException`（編譯錯）。

- [ ] **Step 3: 寫 minimal 實作（exception）**

Create `src/GHLearning.EasyGmailSmtp.Domain/EmailMessages/Exceptions/InvalidEmailAddressException.cs`:

```csharp
namespace GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;

public sealed class InvalidEmailAddressException : Exception
{
    public InvalidEmailAddressException(string message) : base(message) { }
}
```

- [ ] **Step 4: 寫 minimal 實作（EmailAddress）**

Create `src/GHLearning.EasyGmailSmtp.Domain/EmailMessages/EmailAddress.cs`:

```csharp
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
            if (!string.Equals(parsed.Address, value, StringComparison.OrdinalIgnoreCase))
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
```

- [ ] **Step 5: 跑綠燈**

```powershell
dotnet test tests/GHLearning.EasyGmailSmtp.Domain.Tests/GHLearning.EasyGmailSmtp.Domain.Tests.csproj
```

Expected：PASS（5 tests passed）。

- [ ] **Step 6: Commit**

```powershell
git add src/GHLearning.EasyGmailSmtp.Domain tests/GHLearning.EasyGmailSmtp.Domain.Tests
git commit -m "🤖 feat(domain): 加入 EmailAddress Value Object 與驗證"
```

---

## Task 3: Domain — `EmailSubject` Value Object（TDD）

**Files:**
- Create: `src/GHLearning.EasyGmailSmtp.Domain/EmailMessages/Exceptions/InvalidEmailSubjectException.cs`
- Create: `src/GHLearning.EasyGmailSmtp.Domain/EmailMessages/EmailSubject.cs`
- Test: `tests/GHLearning.EasyGmailSmtp.Domain.Tests/EmailMessages/EmailSubjectTests.cs`

- [ ] **Step 1: 撰寫紅燈測試**

Create `tests/GHLearning.EasyGmailSmtp.Domain.Tests/EmailMessages/EmailSubjectTests.cs`:

```csharp
using GHLearning.EasyGmailSmtp.Domain.EmailMessages;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;
using Xunit;

namespace GHLearning.EasyGmailSmtp.Domain.Tests.EmailMessages;

public class EmailSubjectTests
{
    [Fact]
    public void Constructor_WithValidSubject_SetsValue()
    {
        var subject = new EmailSubject("Hello");
        Assert.Equal("Hello", subject.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespace_Throws(string input)
    {
        Assert.Throws<InvalidEmailSubjectException>(() => new EmailSubject(input));
    }

    [Fact]
    public void Constructor_WithNull_Throws()
    {
        Assert.Throws<InvalidEmailSubjectException>(() => new EmailSubject(null!));
    }

    [Fact]
    public void Constructor_ExceedingMaxLength_Throws()
    {
        var tooLong = new string('a', EmailSubject.MaxLength + 1);
        Assert.Throws<InvalidEmailSubjectException>(() => new EmailSubject(tooLong));
    }

    [Fact]
    public void Constructor_AtMaxLength_Succeeds()
    {
        var atLimit = new string('a', EmailSubject.MaxLength);
        var subject = new EmailSubject(atLimit);
        Assert.Equal(EmailSubject.MaxLength, subject.Value.Length);
    }
}
```

- [ ] **Step 2: 跑紅燈**

```powershell
dotnet test tests/GHLearning.EasyGmailSmtp.Domain.Tests/GHLearning.EasyGmailSmtp.Domain.Tests.csproj
```

Expected：FAIL（`EmailSubject` 不存在）。

- [ ] **Step 3: 寫 minimal 實作（exception）**

Create `src/GHLearning.EasyGmailSmtp.Domain/EmailMessages/Exceptions/InvalidEmailSubjectException.cs`:

```csharp
namespace GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;

public sealed class InvalidEmailSubjectException : Exception
{
    public InvalidEmailSubjectException(string message) : base(message) { }
}
```

- [ ] **Step 4: 寫 minimal 實作（EmailSubject）**

Create `src/GHLearning.EasyGmailSmtp.Domain/EmailMessages/EmailSubject.cs`:

```csharp
using GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;

namespace GHLearning.EasyGmailSmtp.Domain.EmailMessages;

public sealed record EmailSubject
{
    public const int MaxLength = 200;

    public string Value { get; }

    public EmailSubject(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidEmailSubjectException("Email subject cannot be empty.");

        if (value.Length > MaxLength)
            throw new InvalidEmailSubjectException(
                $"Email subject must not exceed {MaxLength} characters.");

        Value = value;
    }

    public override string ToString() => Value;
}
```

- [ ] **Step 5: 跑綠燈**

```powershell
dotnet test tests/GHLearning.EasyGmailSmtp.Domain.Tests/GHLearning.EasyGmailSmtp.Domain.Tests.csproj
```

Expected：PASS（先前 5 + 新 5 = 10 tests passed）。

- [ ] **Step 6: Commit**

```powershell
git add src/GHLearning.EasyGmailSmtp.Domain tests/GHLearning.EasyGmailSmtp.Domain.Tests
git commit -m "🤖 feat(domain): 加入 EmailSubject Value Object 與長度上限"
```

---

## Task 4: Domain — `EmailBody` Value Object（TDD）

**Files:**
- Create: `src/GHLearning.EasyGmailSmtp.Domain/EmailMessages/Exceptions/InvalidEmailBodyException.cs`
- Create: `src/GHLearning.EasyGmailSmtp.Domain/EmailMessages/EmailBody.cs`
- Test: `tests/GHLearning.EasyGmailSmtp.Domain.Tests/EmailMessages/EmailBodyTests.cs`

- [ ] **Step 1: 撰寫紅燈測試**

Create `tests/GHLearning.EasyGmailSmtp.Domain.Tests/EmailMessages/EmailBodyTests.cs`:

```csharp
using GHLearning.EasyGmailSmtp.Domain.EmailMessages;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;
using Xunit;

namespace GHLearning.EasyGmailSmtp.Domain.Tests.EmailMessages;

public class EmailBodyTests
{
    [Fact]
    public void Constructor_WithValidBody_SetsValue()
    {
        var body = new EmailBody("Hello world");
        Assert.Equal("Hello world", body.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespace_Throws(string input)
    {
        Assert.Throws<InvalidEmailBodyException>(() => new EmailBody(input));
    }

    [Fact]
    public void Constructor_WithNull_Throws()
    {
        Assert.Throws<InvalidEmailBodyException>(() => new EmailBody(null!));
    }
}
```

- [ ] **Step 2: 跑紅燈**

```powershell
dotnet test tests/GHLearning.EasyGmailSmtp.Domain.Tests/GHLearning.EasyGmailSmtp.Domain.Tests.csproj
```

Expected：FAIL（`EmailBody` 不存在）。

- [ ] **Step 3: 寫 minimal 實作（exception）**

Create `src/GHLearning.EasyGmailSmtp.Domain/EmailMessages/Exceptions/InvalidEmailBodyException.cs`:

```csharp
namespace GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;

public sealed class InvalidEmailBodyException : Exception
{
    public InvalidEmailBodyException(string message) : base(message) { }
}
```

- [ ] **Step 4: 寫 minimal 實作（EmailBody）**

Create `src/GHLearning.EasyGmailSmtp.Domain/EmailMessages/EmailBody.cs`:

```csharp
using GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;

namespace GHLearning.EasyGmailSmtp.Domain.EmailMessages;

public sealed record EmailBody
{
    public string Value { get; }

    public EmailBody(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidEmailBodyException("Email body cannot be empty.");

        Value = value;
    }

    public override string ToString() => Value;
}
```

- [ ] **Step 5: 跑綠燈**

```powershell
dotnet test tests/GHLearning.EasyGmailSmtp.Domain.Tests/GHLearning.EasyGmailSmtp.Domain.Tests.csproj
```

Expected：PASS（先前 10 + 新 4 = 14 tests passed）。

- [ ] **Step 6: Commit**

```powershell
git add src/GHLearning.EasyGmailSmtp.Domain tests/GHLearning.EasyGmailSmtp.Domain.Tests
git commit -m "🤖 feat(domain): 加入 EmailBody Value Object"
```

---

## Task 5: Domain — `EmailMessage` Aggregate（TDD）

**Files:**
- Create: `src/GHLearning.EasyGmailSmtp.Domain/EmailMessages/EmailMessage.cs`
- Test: `tests/GHLearning.EasyGmailSmtp.Domain.Tests/EmailMessages/EmailMessageTests.cs`

- [ ] **Step 1: 撰寫紅燈測試**

Create `tests/GHLearning.EasyGmailSmtp.Domain.Tests/EmailMessages/EmailMessageTests.cs`:

```csharp
using GHLearning.EasyGmailSmtp.Domain.EmailMessages;
using Xunit;

namespace GHLearning.EasyGmailSmtp.Domain.Tests.EmailMessages;

public class EmailMessageTests
{
    [Fact]
    public void Constructor_WithAllParts_SetsProperties()
    {
        var to = new EmailAddress("user@example.com");
        var subject = new EmailSubject("Hello");
        var body = new EmailBody("World");

        var message = new EmailMessage(to, subject, body);

        Assert.Equal(to, message.To);
        Assert.Equal(subject, message.Subject);
        Assert.Equal(body, message.Body);
    }

    [Fact]
    public void Constructor_WithNullTo_Throws()
    {
        var subject = new EmailSubject("Hello");
        var body = new EmailBody("World");

        Assert.Throws<ArgumentNullException>(() => new EmailMessage(null!, subject, body));
    }

    [Fact]
    public void Constructor_WithNullSubject_Throws()
    {
        var to = new EmailAddress("user@example.com");
        var body = new EmailBody("World");

        Assert.Throws<ArgumentNullException>(() => new EmailMessage(to, null!, body));
    }

    [Fact]
    public void Constructor_WithNullBody_Throws()
    {
        var to = new EmailAddress("user@example.com");
        var subject = new EmailSubject("Hello");

        Assert.Throws<ArgumentNullException>(() => new EmailMessage(to, subject, null!));
    }
}
```

- [ ] **Step 2: 跑紅燈**

```powershell
dotnet test tests/GHLearning.EasyGmailSmtp.Domain.Tests/GHLearning.EasyGmailSmtp.Domain.Tests.csproj
```

Expected：FAIL（`EmailMessage` 不存在）。

- [ ] **Step 3: 寫 minimal 實作**

Create `src/GHLearning.EasyGmailSmtp.Domain/EmailMessages/EmailMessage.cs`:

```csharp
namespace GHLearning.EasyGmailSmtp.Domain.EmailMessages;

public sealed class EmailMessage
{
    public EmailAddress To { get; }
    public EmailSubject Subject { get; }
    public EmailBody Body { get; }

    public EmailMessage(EmailAddress to, EmailSubject subject, EmailBody body)
    {
        ArgumentNullException.ThrowIfNull(to);
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(body);

        To = to;
        Subject = subject;
        Body = body;
    }
}
```

- [ ] **Step 4: 跑綠燈**

```powershell
dotnet test tests/GHLearning.EasyGmailSmtp.Domain.Tests/GHLearning.EasyGmailSmtp.Domain.Tests.csproj
```

Expected：PASS（先前 14 + 新 4 = 18 tests passed）。

- [ ] **Step 5: Commit**

```powershell
git add src/GHLearning.EasyGmailSmtp.Domain tests/GHLearning.EasyGmailSmtp.Domain.Tests
git commit -m "🤖 feat(domain): 加入 EmailMessage Aggregate"
```

---

## Task 6: Application — `IEmailSender` Port、`SendEmailCommand` 與 Handler（TDD）

**Files:**
- Create: `src/GHLearning.EasyGmailSmtp.Application/Abstractions/IEmailSender.cs`
- Create: `src/GHLearning.EasyGmailSmtp.Application/SendEmail/SendEmailCommand.cs`
- Create: `src/GHLearning.EasyGmailSmtp.Application/SendEmail/SendEmailCommandHandler.cs`
- Create: `src/GHLearning.EasyGmailSmtp.Application/DependencyInjection.cs`
- Test: `tests/GHLearning.EasyGmailSmtp.Application.Tests/SendEmail/SendEmailCommandHandlerTests.cs`

- [ ] **Step 1: 為 Application 專案加入 Microsoft.Extensions.DependencyInjection.Abstractions 套件**

```powershell
dotnet add src/GHLearning.EasyGmailSmtp.Application/GHLearning.EasyGmailSmtp.Application.csproj package Microsoft.Extensions.DependencyInjection.Abstractions --version 10.0.0
```

- [ ] **Step 2: 撰寫紅燈測試**

Create `tests/GHLearning.EasyGmailSmtp.Application.Tests/SendEmail/SendEmailCommandHandlerTests.cs`:

```csharp
using GHLearning.EasyGmailSmtp.Application.Abstractions;
using GHLearning.EasyGmailSmtp.Application.SendEmail;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;
using NSubstitute;
using Xunit;

namespace GHLearning.EasyGmailSmtp.Application.Tests.SendEmail;

public class SendEmailCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_DelegatesToSender()
    {
        var sender = Substitute.For<IEmailSender>();
        var handler = new SendEmailCommandHandler(sender);
        var command = new SendEmailCommand("user@example.com", "Hello", "World");

        await handler.HandleAsync(command, CancellationToken.None);

        await sender.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m =>
                m.To.Value == "user@example.com" &&
                m.Subject.Value == "Hello" &&
                m.Body.Value == "World"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithInvalidEmail_DoesNotCallSender()
    {
        var sender = Substitute.For<IEmailSender>();
        var handler = new SendEmailCommandHandler(sender);
        var command = new SendEmailCommand("not-an-email", "Hello", "World");

        await Assert.ThrowsAsync<InvalidEmailAddressException>(
            () => handler.HandleAsync(command, CancellationToken.None));

        await sender.DidNotReceive().SendAsync(
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_PropagatesCancellationToken()
    {
        var sender = Substitute.For<IEmailSender>();
        var handler = new SendEmailCommandHandler(sender);
        var command = new SendEmailCommand("user@example.com", "Hello", "World");
        using var cts = new CancellationTokenSource();

        await handler.HandleAsync(command, cts.Token);

        await sender.Received(1).SendAsync(Arg.Any<EmailMessage>(), cts.Token);
    }
}
```

- [ ] **Step 3: 跑紅燈**

```powershell
dotnet test tests/GHLearning.EasyGmailSmtp.Application.Tests/GHLearning.EasyGmailSmtp.Application.Tests.csproj
```

Expected：FAIL（編譯錯：缺 `IEmailSender`、`SendEmailCommand`、`SendEmailCommandHandler`）。

- [ ] **Step 4: 寫 minimal 實作（port）**

Create `src/GHLearning.EasyGmailSmtp.Application/Abstractions/IEmailSender.cs`:

```csharp
using GHLearning.EasyGmailSmtp.Domain.EmailMessages;

namespace GHLearning.EasyGmailSmtp.Application.Abstractions;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
```

- [ ] **Step 5: 寫 minimal 實作（command）**

Create `src/GHLearning.EasyGmailSmtp.Application/SendEmail/SendEmailCommand.cs`:

```csharp
namespace GHLearning.EasyGmailSmtp.Application.SendEmail;

public sealed record SendEmailCommand(string To, string Subject, string Body);
```

- [ ] **Step 6: 寫 minimal 實作（handler）**

Create `src/GHLearning.EasyGmailSmtp.Application/SendEmail/SendEmailCommandHandler.cs`:

```csharp
using GHLearning.EasyGmailSmtp.Application.Abstractions;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages;

namespace GHLearning.EasyGmailSmtp.Application.SendEmail;

public sealed class SendEmailCommandHandler
{
    private readonly IEmailSender _emailSender;

    public SendEmailCommandHandler(IEmailSender emailSender)
    {
        ArgumentNullException.ThrowIfNull(emailSender);
        _emailSender = emailSender;
    }

    public async Task HandleAsync(SendEmailCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var message = new EmailMessage(
            new EmailAddress(command.To),
            new EmailSubject(command.Subject),
            new EmailBody(command.Body));

        await _emailSender.SendAsync(message, cancellationToken);
    }
}
```

- [ ] **Step 7: 寫 Application DI extension**

Create `src/GHLearning.EasyGmailSmtp.Application/DependencyInjection.cs`:

```csharp
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
```

- [ ] **Step 8: 跑綠燈**

```powershell
dotnet test tests/GHLearning.EasyGmailSmtp.Application.Tests/GHLearning.EasyGmailSmtp.Application.Tests.csproj
```

Expected：PASS（3 tests passed）。

- [ ] **Step 9: Commit**

```powershell
git add src/GHLearning.EasyGmailSmtp.Application tests/GHLearning.EasyGmailSmtp.Application.Tests
git commit -m "🤖 feat(application): 加入 SendEmail use case 與 IEmailSender port"
```

---

## Task 7: Infrastructure — `SmtpOptions` + `GoogleSmtpEmailSender`（TDD）

**Files:**
- Create: `src/GHLearning.EasyGmailSmtp.Infrastructure/Email/SmtpOptions.cs`
- Create: `src/GHLearning.EasyGmailSmtp.Infrastructure/Email/GoogleSmtpEmailSender.cs`
- Create: `src/GHLearning.EasyGmailSmtp.Infrastructure/DependencyInjection.cs`
- Test: `tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests/Email/GoogleSmtpEmailSenderTests.cs`

- [ ] **Step 1: 為 Infrastructure 專案加入 MailKit 與 Options 套件**

```powershell
dotnet add src/GHLearning.EasyGmailSmtp.Infrastructure/GHLearning.EasyGmailSmtp.Infrastructure.csproj package MailKit --version 4.8.0
dotnet add src/GHLearning.EasyGmailSmtp.Infrastructure/GHLearning.EasyGmailSmtp.Infrastructure.csproj package Microsoft.Extensions.Options --version 10.0.0
dotnet add src/GHLearning.EasyGmailSmtp.Infrastructure/GHLearning.EasyGmailSmtp.Infrastructure.csproj package Microsoft.Extensions.Options.ConfigurationExtensions --version 10.0.0
dotnet add src/GHLearning.EasyGmailSmtp.Infrastructure/GHLearning.EasyGmailSmtp.Infrastructure.csproj package Microsoft.Extensions.DependencyInjection.Abstractions --version 10.0.0
```

- [ ] **Step 2: 為 Infrastructure.Tests 加入 MailKit（測試需引用 MimeMessage / ISmtpClient）**

```powershell
dotnet add tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests.csproj package MailKit --version 4.8.0
dotnet add tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests.csproj package Microsoft.Extensions.Options --version 10.0.0
```

- [ ] **Step 3: 寫 SmtpOptions（先於測試引用）**

Create `src/GHLearning.EasyGmailSmtp.Infrastructure/Email/SmtpOptions.cs`:

```csharp
namespace GHLearning.EasyGmailSmtp.Infrastructure.Email;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 465;
    public bool UseSsl { get; set; } = true;
    public bool RequireAuthentication { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Easy Email";
}
```

- [ ] **Step 4: 撰寫紅燈測試**

Create `tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests/Email/GoogleSmtpEmailSenderTests.cs`:

```csharp
using GHLearning.EasyGmailSmtp.Domain.EmailMessages;
using GHLearning.EasyGmailSmtp.Infrastructure.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NSubstitute;
using Xunit;

namespace GHLearning.EasyGmailSmtp.Infrastructure.Tests.Email;

public class GoogleSmtpEmailSenderTests
{
    private static SmtpOptions DefaultOptions() => new()
    {
        Host = "smtp.gmail.com",
        Port = 465,
        UseSsl = true,
        RequireAuthentication = true,
        Username = "sender@gmail.com",
        Password = "app-password",
        FromAddress = "sender@gmail.com",
        FromName = "Sender"
    };

    private static EmailMessage SampleMessage() => new(
        new EmailAddress("recipient@example.com"),
        new EmailSubject("Hello"),
        new EmailBody("World"));

    [Fact]
    public async Task SendAsync_ConnectsAuthenticatesSendsAndDisconnects_InOrder()
    {
        var client = Substitute.For<ISmtpClient>();
        var options = Options.Create(DefaultOptions());
        var sut = new GoogleSmtpEmailSender(options, client);

        await sut.SendAsync(SampleMessage(), CancellationToken.None);

        Received.InOrder(() =>
        {
            client.ConnectAsync(
                "smtp.gmail.com",
                465,
                SecureSocketOptions.SslOnConnect,
                Arg.Any<CancellationToken>());
            client.AuthenticateAsync(
                "sender@gmail.com",
                "app-password",
                Arg.Any<CancellationToken>());
            client.SendAsync(
                Arg.Any<MimeMessage>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<MailKit.ITransferProgress>());
            client.DisconnectAsync(true, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task SendAsync_WithoutSsl_UsesStartTls()
    {
        var client = Substitute.For<ISmtpClient>();
        var opts = DefaultOptions();
        opts.UseSsl = false;
        opts.Port = 587;
        var sut = new GoogleSmtpEmailSender(Options.Create(opts), client);

        await sut.SendAsync(SampleMessage(), CancellationToken.None);

        await client.Received(1).ConnectAsync(
            "smtp.gmail.com",
            587,
            SecureSocketOptions.StartTls,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_WithRequireAuthenticationFalse_SkipsAuthenticate()
    {
        var client = Substitute.For<ISmtpClient>();
        var opts = DefaultOptions();
        opts.RequireAuthentication = false;
        var sut = new GoogleSmtpEmailSender(Options.Create(opts), client);

        await sut.SendAsync(SampleMessage(), CancellationToken.None);

        await client.DidNotReceive().AuthenticateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_BuildsMimeMessageFromDomainMessage()
    {
        var client = Substitute.For<ISmtpClient>();
        var sut = new GoogleSmtpEmailSender(Options.Create(DefaultOptions()), client);
        MimeMessage? captured = null;

        await client.SendAsync(
            Arg.Do<MimeMessage>(m => captured = m),
            Arg.Any<CancellationToken>(),
            Arg.Any<MailKit.ITransferProgress>());

        await sut.SendAsync(SampleMessage(), CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal("Hello", captured!.Subject);
        Assert.Equal("sender@gmail.com", ((MailboxAddress)captured.From[0]).Address);
        Assert.Equal("recipient@example.com", ((MailboxAddress)captured.To[0]).Address);
        Assert.Contains("World", captured.TextBody);
    }

    [Fact]
    public async Task SendAsync_DisconnectsEvenWhenSendThrows()
    {
        var client = Substitute.For<ISmtpClient>();
        client.SendAsync(
            Arg.Any<MimeMessage>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<MailKit.ITransferProgress>())
            .Returns<Task<string>>(_ => throw new InvalidOperationException("smtp boom"));

        var sut = new GoogleSmtpEmailSender(Options.Create(DefaultOptions()), client);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.SendAsync(SampleMessage(), CancellationToken.None));

        await client.Received(1).DisconnectAsync(true, Arg.Any<CancellationToken>());
    }
}
```

- [ ] **Step 5: 跑紅燈**

```powershell
dotnet test tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests.csproj
```

Expected：FAIL（`GoogleSmtpEmailSender` 不存在）。

- [ ] **Step 6: 寫 minimal 實作**

Create `src/GHLearning.EasyGmailSmtp.Infrastructure/Email/GoogleSmtpEmailSender.cs`:

```csharp
using GHLearning.EasyGmailSmtp.Application.Abstractions;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GHLearning.EasyGmailSmtp.Infrastructure.Email;

public sealed class GoogleSmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ISmtpClient _client;

    public GoogleSmtpEmailSender(IOptions<SmtpOptions> options, ISmtpClient client)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(client);
        _options = options.Value;
        _client = client;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        var mime = BuildMimeMessage(message);
        var secureOption = _options.UseSsl
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        await _client.ConnectAsync(_options.Host, _options.Port, secureOption, cancellationToken);

        if (_options.RequireAuthentication)
        {
            await _client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
        }

        try
        {
            await _client.SendAsync(mime, cancellationToken);
        }
        finally
        {
            await _client.DisconnectAsync(true, cancellationToken);
        }
    }

    private MimeMessage BuildMimeMessage(EmailMessage message)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        mime.To.Add(MailboxAddress.Parse(message.To.Value));
        mime.Subject = message.Subject.Value;
        mime.Body = new TextPart("plain") { Text = message.Body.Value };
        return mime;
    }
}
```

- [ ] **Step 7: 寫 Infrastructure DI extension**

Create `src/GHLearning.EasyGmailSmtp.Infrastructure/DependencyInjection.cs`:

```csharp
using GHLearning.EasyGmailSmtp.Application.Abstractions;
using GHLearning.EasyGmailSmtp.Infrastructure.Email;
using MailKit.Net.Smtp;
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
        services.AddTransient<ISmtpClient, SmtpClient>();
        services.AddScoped<IEmailSender, GoogleSmtpEmailSender>();
        return services;
    }
}
```

- [ ] **Step 8: 跑綠燈**

```powershell
dotnet test tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests.csproj
```

Expected：PASS（5 tests passed）。

- [ ] **Step 9: Commit**

```powershell
git add src/GHLearning.EasyGmailSmtp.Infrastructure tests/GHLearning.EasyGmailSmtp.Infrastructure.Tests
git commit -m "🤖 feat(infrastructure): 加入 GoogleSmtpEmailSender 與 SmtpOptions"
```

---

## Task 8: WebApi — Contracts（Request / Response DTO）

**Files:**
- Create: `src/GHLearning.EasyGmailSmtp.WebApi/Contracts/SendEmailRequest.cs`
- Create: `src/GHLearning.EasyGmailSmtp.WebApi/Contracts/SendEmailResponse.cs`

- [ ] **Step 1: 寫 SendEmailRequest**

Create `src/GHLearning.EasyGmailSmtp.WebApi/Contracts/SendEmailRequest.cs`:

```csharp
namespace GHLearning.EasyGmailSmtp.WebApi.Contracts;

public sealed record SendEmailRequest(string To, string Subject, string Body);
```

- [ ] **Step 2: 寫 SendEmailResponse**

Create `src/GHLearning.EasyGmailSmtp.WebApi/Contracts/SendEmailResponse.cs`:

```csharp
namespace GHLearning.EasyGmailSmtp.WebApi.Contracts;

public sealed record SendEmailResponse(bool Success, string? Error);
```

- [ ] **Step 3: 驗證可 build**

```powershell
dotnet build src/GHLearning.EasyGmailSmtp.WebApi/GHLearning.EasyGmailSmtp.WebApi.csproj
```

Expected：`Build succeeded`，0 errors、0 warnings。

- [ ] **Step 4: Commit**

```powershell
git add src/GHLearning.EasyGmailSmtp.WebApi/Contracts
git commit -m "🤖 feat(api): 加入 SendEmail 請求與回應 DTO"
```

---

## Task 9: WebApi — Minimal API Endpoint

**Files:**
- Create: `src/GHLearning.EasyGmailSmtp.WebApi/Endpoints/EmailEndpoints.cs`

- [ ] **Step 1: 寫 EmailEndpoints extension**

Create `src/GHLearning.EasyGmailSmtp.WebApi/Endpoints/EmailEndpoints.cs`:

```csharp
using GHLearning.EasyGmailSmtp.Application.SendEmail;
using GHLearning.EasyGmailSmtp.Domain.EmailMessages.Exceptions;
using GHLearning.EasyGmailSmtp.WebApi.Contracts;

namespace GHLearning.EasyGmailSmtp.WebApi.Endpoints;

public static class EmailEndpoints
{
    public static IEndpointRouteBuilder MapEmailEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/emails").WithTags("Emails");

        group.MapPost("/", async (
            SendEmailRequest request,
            SendEmailCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await handler.HandleAsync(
                    new SendEmailCommand(request.To, request.Subject, request.Body),
                    cancellationToken);

                return Results.Ok(new SendEmailResponse(true, null));
            }
            catch (InvalidEmailAddressException ex)
            {
                return Results.BadRequest(new SendEmailResponse(false, ex.Message));
            }
            catch (InvalidEmailSubjectException ex)
            {
                return Results.BadRequest(new SendEmailResponse(false, ex.Message));
            }
            catch (InvalidEmailBodyException ex)
            {
                return Results.BadRequest(new SendEmailResponse(false, ex.Message));
            }
        })
        .WithName("SendEmail")
        .Produces<SendEmailResponse>(StatusCodes.Status200OK)
        .Produces<SendEmailResponse>(StatusCodes.Status400BadRequest);

        return app;
    }
}
```

- [ ] **Step 2: 驗證可 build**

```powershell
dotnet build src/GHLearning.EasyGmailSmtp.WebApi/GHLearning.EasyGmailSmtp.WebApi.csproj
```

Expected：`Build succeeded`，0 errors、0 warnings。

- [ ] **Step 3: Commit**

```powershell
git add src/GHLearning.EasyGmailSmtp.WebApi/Endpoints
git commit -m "🤖 feat(api): 加入 POST /api/emails Minimal API endpoint"
```

---

## Task 10: WebApi — Program.cs、appsettings.json 與 DI 組裝

**Files:**
- Modify: `src/GHLearning.EasyGmailSmtp.WebApi/Program.cs`
- Modify: `src/GHLearning.EasyGmailSmtp.WebApi/appsettings.json`
- Create: `src/GHLearning.EasyGmailSmtp.WebApi/appsettings.Development.json`

- [ ] **Step 1: 為 WebApi 專案加入 OpenAPI 套件**

```powershell
dotnet add src/GHLearning.EasyGmailSmtp.WebApi/GHLearning.EasyGmailSmtp.WebApi.csproj package Microsoft.AspNetCore.OpenApi --version 10.0.0
```

- [ ] **Step 2: 覆寫 Program.cs**

Modify `src/GHLearning.EasyGmailSmtp.WebApi/Program.cs`（template 預設只有 hello world，整檔替換）：

```csharp
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
}

app.MapEmailEndpoints();

app.Run();
```

- [ ] **Step 3: 覆寫 appsettings.json（**不要**在此填真實密碼，僅留結構）**

Modify `src/GHLearning.EasyGmailSmtp.WebApi/appsettings.json`：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 465,
    "UseSsl": true,
    "RequireAuthentication": true,
    "Username": "",
    "Password": "",
    "FromAddress": "",
    "FromName": "Easy Email"
  }
}
```

- [ ] **Step 4: 建立 appsettings.Development.json（**會被 .gitignore 排除**，本機填真實 Gmail App Password）**

Create `src/GHLearning.EasyGmailSmtp.WebApi/appsettings.Development.json`：

```json
{
  "Smtp": {
    "Username": "your-account@gmail.com",
    "Password": "your-16-char-app-password",
    "FromAddress": "your-account@gmail.com",
    "FromName": "Easy Email (Dev)"
  }
}
```

> Gmail App Password 取得方式：Google 帳戶 → 安全性 → 兩步驟驗證 → 應用程式密碼。

- [ ] **Step 5: 驗證 WebApi 可 build 且 solution 全綠**

```powershell
dotnet build GHLearning.EasyGmailSmtp.sln
dotnet test GHLearning.EasyGmailSmtp.sln
```

Expected：`Build succeeded` + 全部測試通過（Domain 18 + Application 3 + Infrastructure 5 = 26 tests）。

- [ ] **Step 6: Commit**

```powershell
git add src/GHLearning.EasyGmailSmtp.WebApi/Program.cs src/GHLearning.EasyGmailSmtp.WebApi/appsettings.json src/GHLearning.EasyGmailSmtp.WebApi/appsettings.Development.json src/GHLearning.EasyGmailSmtp.WebApi/GHLearning.EasyGmailSmtp.WebApi.csproj
git commit -m "🤖 feat(api): 組裝 Program.cs DI 與 SMTP 設定"
```

---

## Task 11: 整合驗證（手動 smoke test）

> 本任務無新檔；目的是用真實 Gmail SMTP 驗 endpoint 是否端到端可用。

**Files:**（無）

- [ ] **Step 1: 確認 `appsettings.Development.json` 已填入有效 Gmail App Password**

開啟 `src/GHLearning.EasyGmailSmtp.WebApi/appsettings.Development.json` 確認 `Username`、`Password`、`FromAddress` 都已填，且 `Password` 是 16 碼 App Password（無空白）。

- [ ] **Step 2: 啟動 WebApi**

```powershell
dotnet run --project src/GHLearning.EasyGmailSmtp.WebApi/GHLearning.EasyGmailSmtp.WebApi.csproj
```

Expected：console 出現 `Now listening on: http://localhost:5xxx`（記下 port，假設為 5000）。

- [ ] **Step 3: 在另一個 PowerShell 視窗呼叫 endpoint**

```powershell
$body = @{
  to      = "your-test-recipient@example.com"
  subject = "GHLearning.EasyGmailSmtp Smoke Test"
  body    = "Hello from GHLearning.EasyGmailSmtp!"
} | ConvertTo-Json

Invoke-RestMethod -Method Post `
  -Uri "http://localhost:5000/api/emails" `
  -ContentType "application/json" `
  -Body $body
```

Expected：回應 `success=True, error=$null`，且收件信箱實際收到郵件。

- [ ] **Step 4: 驗證壞 email 回 400**

```powershell
$bad = @{ to = "not-an-email"; subject = "x"; body = "y" } | ConvertTo-Json
try {
  Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/emails" `
    -ContentType "application/json" -Body $bad
} catch {
  $_.Exception.Response.StatusCode
  $_.ErrorDetails.Message
}
```

Expected：`BadRequest` + JSON body 含 `success=false, error="'not-an-email' is not a valid email address."`。

- [ ] **Step 5: 停掉 WebApi**

回到第一個視窗按 `Ctrl+C`。

- [ ] **Step 6: 跑全測試最後一次確認**

```powershell
dotnet test GHLearning.EasyGmailSmtp.sln
```

Expected：全綠（26 tests passed）。

- [ ] **Step 7: Commit（如僅有 smoke test 紀錄則跳過；若過程修了任何小 bug，就 commit）**

```powershell
git status
# 若有變更才 commit
git add .
git commit -m "🤖 chore(verify): 通過 Google SMTP 端到端 smoke test"
```

---

## Self-Review 摘要

- **Spec coverage**：
  - Google SMTP 寄信 → Task 7 `GoogleSmtpEmailSender`（MailKit；預設 `smtp.gmail.com:465` + `SslOnConnect` + `RequireAuthentication=true`，可切換至 587/StartTls 或關閉驗證）
  - 收件信箱 / 標題 / 內容 → Task 2/3/4 Value Objects + Task 6 `SendEmailCommand` + Task 8 `SendEmailRequest`
  - DDD + Clean Architecture → Task 2~5 Domain / Task 6 Application / Task 7 Infrastructure / Task 8~10 WebApi，依賴方向僅向內
  - xunit.v3 3.2.2 + xunit.runner.visualstudio 3.1.5 + NSubstitute 5.3.0 → Task 1 Step 5 固定版號
  - C# .NET 10 → 全部 csproj 用 `net10.0`
  - Minimal API → Task 9 `MapGroup` + `MapPost`
- **Placeholder scan**：無 TBD / TODO / "implement later"；所有 code block 為完整檔內容。
- **Type consistency**：
  - `IEmailSender.SendAsync(EmailMessage, CancellationToken)` 一致於 Task 6 / 7 / 7 tests
  - `EmailMessage` constructor (`To, Subject, Body`) 一致於 Task 5 / 6 / 7
  - `SmtpOptions` 欄位（Host/Port/UseSsl/RequireAuthentication/Username/Password/FromAddress/FromName）一致於 Task 7 / 10 appsettings.json，預設值（`smtp.gmail.com` / `465` / `UseSsl=true` / `RequireAuthentication=true`）一致
  - `SendEmailCommand(To, Subject, Body)` 一致於 Task 6 / 9
