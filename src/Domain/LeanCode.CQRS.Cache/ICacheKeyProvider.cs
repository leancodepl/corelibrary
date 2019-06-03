using System.Text;

namespace LeanCode.CQRS.Cache
{
    public interface ICacheKeyProvider
    {
        void ProvideKey(StringBuilder builder);
    }
}
