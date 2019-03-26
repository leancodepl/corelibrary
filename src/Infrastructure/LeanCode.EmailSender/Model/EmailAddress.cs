using System;

namespace LeanCode.EmailSender.Model
{
    public class EmailAddress
    {
        public string Email { get; }
        public string Name { get; }

        public EmailAddress(string email, string name)
        {
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Name = name;
        }

        public override string ToString() => Email;
    }
}
