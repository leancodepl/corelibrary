using System;

namespace LeanCode.Dapper
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class RawSqlQueryAttribute : Attribute
    { }
}
