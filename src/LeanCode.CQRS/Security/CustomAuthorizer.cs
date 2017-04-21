using System;

namespace LeanCode.CQRS.Security
{
    public interface ICustomAuthorizer
    {
        bool CheckIfAuthorized(object obj);
    }

    public abstract class CustomAuthorizer<T> : ICustomAuthorizer
        where T : class
    {
        public bool CheckIfAuthorized(object obj)
        {
            T customAuthorizer = obj as T;
            if (customAuthorizer == null)
            {
                throw new ArgumentNullException(nameof(customAuthorizer), $"{GetType()} is not valid Authorizer for {obj.GetType()}.");
            }

            return CheckIfAuthorized(customAuthorizer);
        }

        public abstract bool CheckIfAuthorized(T obj);
    }
}
