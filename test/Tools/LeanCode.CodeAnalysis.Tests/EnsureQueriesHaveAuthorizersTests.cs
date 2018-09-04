using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests
{
    public class EnsureQueriesHaveAuthorizersTests
    {
        private const string DiagnosticId = EnsureQueriesHaveAuthorizers.DiagnosticId;
        private readonly EnsureQueriesHaveAuthorizers analyzer = new EnsureQueriesHaveAuthorizers();
        private readonly Action<Diagnostic> reportDiagnostic = Substitute.For<Action<Diagnostic>>();

        [Theory]
        [MemberData(nameof(ClassLoader.LoadTestContracts), "TestSamples/Accepted_queries.cs", MemberType = typeof(ClassLoader))]
        public void Queries_with_authorization_attributes_are_accepted(Compilation compilation, ISymbol symbol)
        {
            var context = GetContext(compilation, symbol);

            analyzer.AnalyzeSymbol(context);

            reportDiagnostic.DidNotReceiveWithAnyArgs()(null);
        }

        [Theory]
        [MemberData(nameof(ClassLoader.LoadTestContracts), "TestSamples/Rejected_queries.cs", MemberType = typeof(ClassLoader))]
        public void Queries_without_authorization_are_rejected(Compilation compilation, ISymbol symbol)
        {
            var context = GetContext(compilation, symbol);

            analyzer.AnalyzeSymbol(context);

            reportDiagnostic.Received()(Arg.Is<Diagnostic>(d => d.Id == DiagnosticId));
        }

        private SymbolAnalysisContext GetContext(Compilation compilation, ISymbol symbol)
        {
            return new SymbolAnalysisContext(symbol, compilation, null, reportDiagnostic, _ => true, CancellationToken.None);
        }
    }
}
