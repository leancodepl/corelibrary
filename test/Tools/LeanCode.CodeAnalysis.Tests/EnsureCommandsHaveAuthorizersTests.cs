using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CQRS;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests
{
    public class EnsureCommandsHaveAuthorizersTests
    {
        private const string DiagnosticId = DiagnosticsIds.QueriesShouldHaveAuthorizers;
        private readonly EnsureCommandsHaveAuthorizers analyzer = new EnsureCommandsHaveAuthorizers();
        private readonly Action<Diagnostic> reportDiagnostic = Substitute.For<Action<Diagnostic>>();

        [Theory]
        [MemberData(nameof(ClassLoader.LoadTestContracts), "TestSamples/Accepted_commands.cs", MemberType = typeof(ClassLoader))]
        public void Commands_with_authorization_attributes_are_accepted(Compilation compilation, ISymbol symbol)
        {
            var context = GetContext(compilation, symbol);

            analyzer.AnalyzeSymbol(context);

            reportDiagnostic.DidNotReceiveWithAnyArgs()(null);
        }

        [Theory]
        [MemberData(nameof(ClassLoader.LoadTestContracts), "TestSamples/Rejected_commands.cs", MemberType = typeof(ClassLoader))]
        public void Commands_without_authorization_are_rejected(Compilation compilation, ISymbol symbol)
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
