using Xunit;

namespace LeanCode.Firebase.FCM.Tests
{
    public sealed class FCMFactAttribute : FactAttribute
    {
        public FCMFactAttribute()
        {
            if (string.IsNullOrEmpty(FCMClientTests.Key))
            {
                Skip = "Key not set";
            }
            else if (string.IsNullOrEmpty(FCMClientTests.Token))
            {
                Skip = "No recipient token provided";
            }
        }
    }
}
