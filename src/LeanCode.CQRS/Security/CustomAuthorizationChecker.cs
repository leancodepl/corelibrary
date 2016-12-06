using System;

namespace LeanCode.CQRS.Security
{
    public interface ICustomAuthorizationChecker
    {
        bool CheckIfAuthorized(object obj);
    }

    public abstract class CustomAuthorizationChecker<T> : ICustomAuthorizationChecker
        where T : class
    {
        public bool CheckIfAuthorized(object obj)
        {
            T customAuthorizationChecker = obj as T;
            if (customAuthorizationChecker == null)
                throw new ArgumentNullException(nameof(customAuthorizationChecker), $"{GetType()} is not valid Authorization Checker for {obj.GetType()}.");

            return CheckIfAuthorized(customAuthorizationChecker);
        }

        public abstract bool CheckIfAuthorized(T obj);
    }
}
