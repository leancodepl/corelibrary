using System;
using System.Collections.Generic;

namespace LeanCode.ViewRenderer
{
    public class CompilationFailedException : Exception
    {
        public string FullPath { get; }
        public IReadOnlyList<string> Errors { get; }

        public CompilationFailedException(string path, IReadOnlyList<string> errors, string msg)
            : base(msg)
        {
            FullPath = path;
            Errors = errors;
        }

        public CompilationFailedException(string path, IReadOnlyList<string> errors, string msg, Exception innerException)
            : base(msg, innerException)
        {
            FullPath = path;
            Errors = errors;
        }
    }
}
