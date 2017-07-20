namespace LeanCode.Facebook
{
    public sealed class FacebookUser
    {
        public string Id { get; }
        public string Email { get; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Photo { get; }

        public FacebookUser(
            string id, string email,
            string firstName, string lastName, string photo)
        {
            Id = id;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            Photo = photo;
        }
    }
}
