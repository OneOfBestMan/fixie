﻿namespace Fixie.Tests.Internal.Listeners
{
    using System.Linq;
    using Fixie.Internal;
    using Fixie.Internal.Listeners;
    using Assertions;

    public class ConsoleListenerTests : MessagingTests
    {
        public void ShouldReportResults()
        {
            var listener = new ConsoleListener();

            using (var console = new RedirectedConsole())
            {
                Run(listener);

                console.Output
                       .CleanStackTraceLineNumbers()
                       .CleanDuration()
                       .Lines()
                       .ShouldEqual(
                           "Console.Out: Fail",
                           "Console.Error: Fail",
                           "Test '" + TestClass + ".Fail' failed:",
                           "",
                           "'Fail' failed!",
                           "",
                           "Fixie.Tests.FailureException",
                           At("Fail()"),
                           "",

                           "Console.Out: FailByAssertion",
                           "Console.Error: FailByAssertion",
                           "Test '" + TestClass + ".FailByAssertion' failed:",
                           "",
                           "Expected: 2",
                           "Actual:   1",
                           "",
                           "Fixie.Assertions.AssertActualExpectedException",
                           At("FailByAssertion()"),
                           "",

                           "Console.Out: Pass",
                           "Console.Error: Pass",

                           "Test '" + TestClass + ".SkipWithReason' skipped:",
                           "Skipped with reason.",
                           "",
                           "Test '" + TestClass + ".SkipWithoutReason' skipped",
                           "",
                           "1 passed, 2 failed, 2 skipped, took 1.23 seconds");
            }
        }

        public void ShouldNotReportPassCountsWhenZeroTestsHavePassed()
        {
            void ZeroPassed(Discovery discovery)
                => discovery.Methods.Where(x => !x.Name.StartsWith("Pass"));

            var listener = new ConsoleListener();

            using (var console = new RedirectedConsole())
            {
                Run(listener, ZeroPassed);

                console.Output
                    .Lines()
                    .Last()
                    .CleanDuration()
                    .ShouldEqual("2 failed, 2 skipped, took 1.23 seconds");
            }
        }

        public void ShouldNotReportFailCountsWhenZeroTestsHaveFailed()
        {
            void ZeroFailed(Discovery discovery)
                => discovery.Methods.Where(x => !x.Name.StartsWith("Fail"));

            var listener = new ConsoleListener();

            using (var console = new RedirectedConsole())
            {
                Run(listener, ZeroFailed);

                console.Output
                    .Lines()
                    .Last()
                    .CleanDuration()
                    .ShouldEqual("1 passed, 2 skipped, took 1.23 seconds");
            }
        }

        public void ShouldNotReportSkipCountsWhenZeroTestsHaveBeenSkipped()
        {
            void ZeroSkipped(Discovery discovery)
                => discovery.Methods.Where(x => !x.Name.StartsWith("Skip"));

            var listener = new ConsoleListener();

            using (var console = new RedirectedConsole())
            {
                Run(listener, ZeroSkipped);

                console.Output
                    .Lines()
                    .Last()
                    .CleanDuration()
                    .ShouldEqual("1 passed, 2 failed, took 1.23 seconds");
            }
        }

        public void ShouldProvideDiagnosticDescriptionWhenNoTestsWereExecuted()
        {
            void NoTestsFound(Discovery discovery)
                => discovery.Methods.Where(x => false);

            var listener = new ConsoleListener();

            using (var console = new RedirectedConsole())
            {
                Run(listener, NoTestsFound);

                console.Output
                    .Lines()
                    .Last()
                    .ShouldEqual("No tests found.");
            }
        }
    }
}