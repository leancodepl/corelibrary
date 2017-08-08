namespace LeanCode.SmsSender
{
    public class SmsApiConfiguration
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        public bool FastMode { get; set; }
        public bool TestMode { get; set; }
    }
}
