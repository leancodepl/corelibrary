namespace LeanCode.ExternalIdentityProviders.Facebook
{
    public sealed class FacebookUser
    {
        public string Id { get; }
        public string? Email { get; }
        public string? FirstName { get; }
        public string? LastName { get; }
        public string Photo { get; }
        public bool IsSilhouette { get; }

        public FacebookUser(
            string id,
            string? email,
            string? firstName,
            string? lastName,
            string photo,
            bool isSilhouette)
        {
            Id = id;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            Photo = photo;
            IsSilhouette = isSilhouette;
        }
    }
}
