using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace LeanCode.ViewRenderer
{
    public class CompilationFailedException : Exception
    {
        public string FullPath { get; }
        public ImmutableList<string> Errors { get; }

        public CompilationFailedException(string path, string msg)
            : base(msg)
        {
            FullPath = path;
            Errors = ImmutableList.Create<string>();
        }

        public CompilationFailedException(string path, string msg, Exception innerException)
            : base(msg, innerException)
        {
            FullPath = path;
            Errors = ImmutableList.Create<string>();
        }

        public CompilationFailedException(string path, IReadOnlyList<string> errors, string msg)
            : base(msg)
        {
            FullPath = path;
            Errors = errors.ToImmutableList();
        }

        public CompilationFailedException(
            string path, IReadOnlyList<string> errors, string msg, Exception innerException)
            : base(msg, innerException)
        {
            FullPath = path;
            Errors = errors.ToImmutableList();
        }
    }
}
