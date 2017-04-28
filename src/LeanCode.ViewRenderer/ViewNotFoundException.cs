using System;

namespace LeanCode.ViewRenderer
{
    public class ViewNotFoundException : Exception
    {
        public string ViewName { get; }

        public ViewNotFoundException(string viewName, string message)
            : base(message)
        {
            ViewName = viewName;
        }
    }
}
