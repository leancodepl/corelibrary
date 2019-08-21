using System;
using Microsoft.CodeAnalysis;

namespace LeanCode.CodeAnalysis.Tests.Verifiers
{
    public struct DiagnosticResult : IEquatable<DiagnosticResult>
    {
        public string Id { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public DiagnosticResult(Diagnostic d)
        {
            var pos = d.Location.GetLineSpan();

            Id = d.Id;
            Line = pos.StartLinePosition.Line;
            Column = pos.StartLinePosition.Character;
        }

        public bool Equals(DiagnosticResult other)
        {
            return Id == other.Id
                && Line == other.Line
                && Column == other.Column;
        }
    }
}
