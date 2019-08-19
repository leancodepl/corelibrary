using System;
using LeanCode.CQRS.Security;

namespace Example.CQRS
{
#pragma warning disable SA1302
    public interface CustomAuth
    {
    }
#pragma warning restore SA1302

    public class CustomAuthAttribute : AuthorizeWhenAttribute
    {
        public CustomAuthAttribute()
        : base(typeof(CustomAuth))
        {
        }
    }

    public interface IFooRelated
    {
        Guid FooId { get; }
    }
}
