namespace LeanCode.ExternalIdentityProviders.Facebook
{
    public sealed record FacebookConfiguration(string AppSecret, int PhotoSize)
    {
        public FacebookConfiguration(string appSecret)
            : this(appSecret, 250)
        { }
    }
}
