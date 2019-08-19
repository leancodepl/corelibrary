using System;
using LeanCode.CQRS;

namespace Example.CQRS
{
    [CustomAuth]
    public class CustomAuthQuery : IQuery<CustomAuthQuery.Result>, IFooRelated
    {
        public Guid FooId { get; set; }

        public class Result { }
    }
}
