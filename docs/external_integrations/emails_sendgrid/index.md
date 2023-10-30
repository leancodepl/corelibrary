# Emails - SendGrid

Managing email communication is a crucial aspect of any modern business or application. LeanCode CoreLibrary streamlines the process of sending emails by integrating with SendGrid, a widely acclaimed and trusted email delivery system. With LeanCode CoreLibrary's custom integration with SendGrid, you gain access to a simplified and efficient email delivery solution. For more information, you can explore [SendGrid documentation](https://docs.sendgrid.com/) and the official [SendGrid C# library GitHub repository](https://github.com/sendgrid/sendgrid-csharp).

## Configuration

To incorporate SendGrid into your LeanCode CoreLibrary-based application, follow the example below. This will enable the SendGrid client, allowing you to send emails through `.cshtml` template files. The example assumes that templates are located in the `Templates` folder.

```csharp
. . .

private static readonly RazorViewRendererOptions ViewOptions = new("Templates");

. . .

public override void ConfigureServices(IServiceCollection services)
{
    . . .

    services.AddRazorViewRenderer(ViewOptions);

    // Add SendGrid ApiKey
    services.AddSendGridClient(new SendGridClientOptions { ApiKey = "" });

    . . .
}
```

Add this code to the startup project to ensure that email templates in the `Templates` folder are included in the output directory and preserved when the project is built:

```xml
  <ItemGroup>
    <None Include="Templates/**">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <Content Remove="Templates/**" />
  </ItemGroup>
```

## Sending emails

Suppose you wish to send a welcome email to a specific user, with their username as an email parameter. For this purpose, you can create a `WelcomeEmail.cs` class:

```csharp
public class WelcomeEmail
{
    public string Username { get; set; }
}
```

You should also define a `WelcomeEmail.cshtml` template file inside `Templates` directory with the same name as model above to ensure proper linkage:

```html
<div>
    Welcome @Model.Username
<div>
```

After configuring as shown above, you can start sending emails using SendGrid with the following code. Make sure to verify your sender address in SendGrid:

```csharp
. . .

private readonly SendGridRazorClient sendGridClient;

. . .

public async Task SendWelcomeEmailAsync(
    string username,
    string email,
    string fromEmail,
    string fromName,
    string subject,
    CancellationToken cancellationToken)
{
    var vm = new WelcomeEmail { Username = username };

    var message = new SendGridRazorMessage()
        .WithSubject(subject)
        .WithSender(fromEmail, fromName)
        .WithRecipient(email)
        .WithHtmlContent(vm);

    await sendGridClient.SendEmailAsync(message, cancellationToken);
}
```
