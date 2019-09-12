namespace LeanCode.SmsSender
{
    public class SmsApiConfiguration
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public bool FastMode { get; set; }
        public bool TestMode { get; set; }
    }
}
