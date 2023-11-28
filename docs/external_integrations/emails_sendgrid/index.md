# Emails - SendGrid

Managing email communication is a crucial aspect of any modern business or application. LeanCode CoreLibrary streamlines the process of sending emails by integrating with SendGrid, a widely acclaimed and trusted email delivery system. With LeanCode CoreLibrary's custom integration with SendGrid, you gain access to a simplified and efficient email delivery solution. For more information, you can explore [SendGrid documentation](https://docs.sendgrid.com/) and the official [SendGrid C# library GitHub repository](https://github.com/sendgrid/sendgrid-csharp).

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.SendGrid | [![NuGet version (LeanCode.SendGrid)](https://img.shields.io/nuget/vpre/LeanCode.SendGrid.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.SendGrid/8.0.2260-preview/) | Configuration |
| LeanCode.ViewRenderer.Razor | [![NuGet version (LeanCode.ViewRenderer.Razor)](https://img.shields.io/nuget/vpre/LeanCode.ViewRenderer.Razor.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.ViewRenderer.Razor/8.0.2260-preview/) | `.cshtml` templates |
| SendGrid | [![NuGet version (SendGrid)](https://img.shields.io/nuget/v/SendGrid.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/SendGrid/9.28.1/) | Email sending |

## Configuration

To incorporate SendGrid into your LeanCode CoreLibrary-based application, follow the example below. This will enable the SendGrid client, allowing you to send emails through `.cshtml` template files. The example assumes that templates are located in the `Templates` folder.

```csharp
// . . .

private static readonly RazorViewRendererOptions ViewOptions = new("Templates");

// . . .

public override void ConfigureServices(IServiceCollection services)
{
    // . . .

    services.AddRazorViewRenderer(ViewOptions);

    // Add SendGrid ApiKey
    services.AddSendGridClient(new SendGridClientOptions { ApiKey = "" });

    // . . .
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

Suppose you need to send an email to an employee to inform them of their assignment, with the assignment name as a parameter. For this purpose you can create a `EmployeeAssignedToAssignmentEmail.cs` class:

```csharp
public class EmployeeAssignedToAssignmentEmail
{
    public string AssignmentName { get; set; }
}
```

You should also define a `EmployeeAssignedToAssignmentEmail.cshtml` template file inside `Templates` directory with the same name as model above to ensure proper linkage:

```html
<div>
    You have been assigned to @Model.AssignmentName assignment.
<div>
```

After configuring as shown above, you can start sending emails using SendGrid with the following code. Make sure to verify your sender address in SendGrid:

```csharp
// . . .
private const string FromEmail = "no-reply@leancode.pl";
private const string FromName = "LeanCode";

private readonly SendGridRazorClient sendGridClient;

// . . .

public async Task SendEmployeeAssignedToAssignmentEmailAsync(
    Employee employee,
    Assignment assignment,
    CancellationToken cancellationToken)
{
    var vm = new EmployeeAssignedToAssignmentEmail
    {
        AssignmentName = assignment.Name
    };

    var message = new SendGridRazorMessage()
        .WithSubject("You have been assigned to assignment")
        .WithSender(FromEmail, FromName)
        .WithRecipient(employee.Email)
        .WithHtmlContent(vm);

    await sendGridClient.SendEmailAsync(message, cancellationToken);
}
```
