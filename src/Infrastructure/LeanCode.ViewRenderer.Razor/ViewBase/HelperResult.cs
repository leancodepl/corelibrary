using System;
using System.IO;

namespace LeanCode.ViewRenderer.Razor.ViewBase
{
    public class HelperResult
    {
        public Action<TextWriter> WriteAction { get; }

        public HelperResult(Action<TextWriter> action)
        {
            WriteAction = action;
        }

        public void WriteTo(TextWriter writer)
        {
            WriteAction(writer);
        }
    }
}
