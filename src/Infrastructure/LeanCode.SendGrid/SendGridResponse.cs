namespace LeanCode.SendGrid;

internal class SendGridResponse
{
    public SendGridError[]? Errors { get; set; }
}

internal class SendGridError
{
    public string? Message { get; set; }
}
