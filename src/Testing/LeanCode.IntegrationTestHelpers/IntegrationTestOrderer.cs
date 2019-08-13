using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace LeanCode.IntegrationTestHelpers
{
    public class IntegrationTestOrderer : ITestCaseOrderer
    {
        private readonly Serilog.ILogger logger;

        public const string ClassName = "LeanCode.IntegrationTestHelpers.IntegrationTestOrderer";
        public const string AssemblyName = "LeanCode.IntegrationTestHelpers";

        public IntegrationTestOrderer()
        {
            IntegrationTestLogging.EnsureLoggerLoaded();

            logger = Serilog.Log.ForContext<IntegrationTestOrderer>();
        }

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(
            IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            var ordered =
                testCases
                    .Select(tc => (TestCase: tc, Attribute: IntegrationFactAttribute.GetCustomOrder(tc.TestMethod.Method)))
                    .ToList();
            var all = ordered.All(s => s.Attribute != null);
            var any = ordered.Any(s => s.Attribute != null);
            if (!all && any)
            {
                logger.Error("CustomOrder is specified on some tests in the collection, but not all tests have it. This is all-or-nothing configuration. Tests in collection: ");
                foreach (var (tc, attrib) in ordered)
                {
                    logger.Error(
                        "    {TestName} - CustomOrder: {Has}",
                        tc.DisplayName, attrib != null);
                }

                logger.Error("Ignoring and using name only");
                ordered = ordered.Select(tc => (tc.TestCase, (int?)null)).ToList();
            }

            return ordered
                .OrderBy(tc => tc.Attribute)
                .ThenBy(tc => tc.TestCase.DisplayName)
                .Select(tc => tc.TestCase)
                .ToList();
        }
    }
}
