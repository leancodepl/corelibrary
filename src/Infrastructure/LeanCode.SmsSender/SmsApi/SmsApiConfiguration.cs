namespace LeanCode.SmsSender.SmsApi
{
    public sealed class SmsApiConfiguration
    {
        public string SmsApiLogin { get; set; }
        public string SmsApiPassword { get; set; }
        public string SmsApiFrom {get; set; }
        public bool FastMode { get; set; }
        public bool TestMode { get; set; }
    }
}
