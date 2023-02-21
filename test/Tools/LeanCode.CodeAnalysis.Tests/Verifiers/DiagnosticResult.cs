using System;
using Microsoft.CodeAnalysis;

namespace LeanCode.CodeAnalysis.Tests.Verifiers;

public struct DiagnosticResult : IEquatable<DiagnosticResult>
{
    public string Id { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }

    public DiagnosticResult(string id, int line, int column)
    {
        Id = id;
        Line = line;
        Column = column;
    }

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

    public override bool Equals(object obj) => obj is DiagnosticResult d && Equals(d);
    public override int GetHashCode() => HashCode.Combine(Id, Line, Column);
    public static bool operator ==(DiagnosticResult left, DiagnosticResult right) => left.Equals(right);
    public static bool operator !=(DiagnosticResult left, DiagnosticResult right) => !left.Equals(right);
}
