namespace LeanCode.SendGrid;

internal sealed class SendGridResponse
{
    public SendGridError[]? Errors { get; set; }
}

internal sealed class SendGridError
{
    public string? Message { get; set; }
}
