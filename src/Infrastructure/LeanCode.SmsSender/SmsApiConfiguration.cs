using System;

namespace LeanCode.SmsSender
{
    /// <remarks>
    /// In an effort to preserve some backward compatibility,
    /// a single class is used for both supported authentication schemes.
    /// Checking which credentials to use is done manually in the client.
    /// </remarks>
    public class SmsApiConfiguration // TODO: change to some immutable record
    {
        public string? Token { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public bool FastMode { get; set; }
        public bool TestMode { get; set; }

        [Obsolete("Use another constructor (preferably the one with token).")]
        public SmsApiConfiguration() { }

        public SmsApiConfiguration(string login, string password, string from)
        {
            Login = login;
            Password = password;
            From = from;
        }

        public SmsApiConfiguration(string token, string from)
        {
            Token = token;
            From = from;

            Login = null!;
            Password = null!;
        }
    }
}
