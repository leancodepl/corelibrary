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
                    .Select(tc => (tc, IntegrationFactAttribute.GetCustomOrder(tc.TestMethod.Method)))
                    .ToList();
            var all = ordered.All(s => s.Item2 != null);
            var any = ordered.Any(s => s.Item2 != null);
            if (!all && any)
            {
                logger.Error("CustomOrder is specified on some tests in the collection, but not all tests have it. This is all-or-nothing configuration. Tests in collection: ");
                foreach (var tc in ordered)
                {
                    logger.Error(
                        "    {TestName} - CustomOrder: {Has}",
                        tc.Item1.DisplayName, tc.Item2 != null);
                }
                logger.Error("Ignoring and using name only");
                ordered = ordered.Select(tc => (tc.Item1, (int?)null)).ToList();
            }

            return ordered
                .OrderBy(tc => tc.Item2)
                .ThenBy(tc => tc.Item1.DisplayName)
                .Select(tc => tc.Item1)
                .ToList();
        }
    }
}
