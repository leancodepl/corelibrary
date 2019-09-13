using System;
using System.Diagnostics.CodeAnalysis;
using Serilog.Core;
using Serilog.Events;

namespace LeanCode.Logging
{
    public abstract class BaseSanitizer<T> : IDestructuringPolicy
    {
        public const string Placeholder = "*** REDACTED ***";

        public bool TryDestructure(
            object value,
            ILogEventPropertyValueFactory propertyValueFactory,
            [NotNullWhen(true)] out LogEventPropertyValue? result)
        {
            if (TrySanitizeInternal(value, out var newValue))
            {
                result = propertyValueFactory.CreatePropertyValue(newValue, true);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private bool TrySanitizeInternal(object obj, [NotNullWhen(true)] out object? newObj)
        {
            if (obj is T u)
            {
                newObj = TrySanitize(u);

                if (ReferenceEquals(newObj, obj))
                {
                    throw new InvalidOperationException("The BaseSanitizer implementation must create new object.");
                }

                return newObj != null;
            }
            else
            {
                newObj = null;

                return false;
            }
        }

        protected abstract T TrySanitize(T obj);
    }
}
