namespace LeanCode.Facebook
{
    public sealed class FacebookUser
    {
        public string Id { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Photo { get; }
        public string LanguageCode { get; }

        public FacebookUser(
            string id, string email,
            string firstName, string lastName, string photo, string languageCode)
        {
            Id = id;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            Photo = photo;
            LanguageCode = languageCode;
        }
    }
}
