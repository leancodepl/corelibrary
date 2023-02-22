namespace LeanCode;

public static class UrlHelper
{
    public static string Concat(string a, string b)
    {
        return a.TrimEnd('/') + '/' + b.TrimStart('/');
    }
}
