using Newtonsoft.Json;

namespace LeanCode.SmsSender.Responses
{
    public class Error
    {
        [JsonProperty(PropertyName = "error", Required = Required.Always)]
        public int Code { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Message { get; set; }
    }
}