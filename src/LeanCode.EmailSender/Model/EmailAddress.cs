namespace LeanCode.EmailSender.Model
{
    public class EmailAddress
    {
        public string Email { get; set; }
        public string Name { get; set; }

        public EmailAddress(string email, string name)
        {
            Email = email;
            Name = name;
        }

        public override string ToString()
        {
            return Email;
        }
    }
}
