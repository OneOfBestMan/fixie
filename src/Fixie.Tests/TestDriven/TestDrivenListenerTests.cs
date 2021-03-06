﻿using TestDriven.Framework;

namespace Fixie.Tests.TestDriven
{
    using System;
    using System.Collections.Generic;
    using Assertions;
    using Fixie.Internal;
    using Fixie.TestDriven;

    public class TestDrivenListenerTests : MessagingTests
    {
        public void ShouldReportResultsToTestDrivenDotNet()
        {
            var testDriven = new StubTestListener();
            var listener = new TestDrivenListener(testDriven);

            using (var console = new RedirectedConsole())
            {
                Run(listener);

                console.Lines()
                    .ShouldEqual(
                        "Console.Out: Fail",
                        "Console.Error: Fail",
                        "Console.Out: FailByAssertion",
                        "Console.Error: FailByAssertion",
                        "Console.Out: Pass",
                        "Console.Error: Pass");
            }

            var results = testDriven.TestResults;
            results.Count.ShouldEqual(5);

            foreach (var result in results)
            {
                result.FixtureType.ShouldEqual(null);
                result.Method.ShouldEqual(null);
                result.TimeSpan.ShouldEqual(TimeSpan.Zero);
                result.TotalTests.ShouldEqual(0);
                result.TestRunnerName.ShouldBeNull();
            }

            var fail = results[0];
            var failByAssertion = results[1];
            var pass = results[2];
            var skipWithReason = results[3];
            var skipWithoutReason = results[4];

            skipWithReason.Name.ShouldEqual(TestClass + ".SkipWithReason");
            skipWithReason.State.ShouldEqual(TestState.Ignored);
            skipWithReason.Message.ShouldEqual("Skipped with reason.");
            skipWithReason.StackTrace.ShouldBeNull();

            skipWithoutReason.Name.ShouldEqual(TestClass + ".SkipWithoutReason");
            skipWithoutReason.State.ShouldEqual(TestState.Ignored);
            skipWithoutReason.Message.ShouldBeNull();
            skipWithoutReason.StackTrace.ShouldBeNull();

            fail.Name.ShouldEqual(TestClass + ".Fail");
            fail.State.ShouldEqual(TestState.Failed);
            fail.Message.ShouldEqual("Fixie.Tests.FailureException");
            fail.StackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldEqual(
                    "'Fail' failed!",
                    "",
                    At("Fail()"));

            failByAssertion.Name.ShouldEqual(TestClass + ".FailByAssertion");
            failByAssertion.State.ShouldEqual(TestState.Failed);
            failByAssertion.Message.ShouldEqual("Fixie.Assertions.AssertActualExpectedException");
            failByAssertion.StackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldEqual(
                    "Expected: 2",
                    "Actual:   1",
                    "",
                    At("FailByAssertion()"));

            pass.Name.ShouldEqual(TestClass + ".Pass");
            pass.State.ShouldEqual(TestState.Passed);
            pass.Message.ShouldBeNull();
            pass.StackTrace.ShouldBeNull();
        }

        class StubTestListener : ITestListener
        {
            public List<TestResult> TestResults { get; } = new List<TestResult>();

            public void TestFinished(TestResult summary)
            {
                TestResults.Add(summary);
            }

            public void WriteLine(string text, Category category)
            {
                throw new NotImplementedException();
            }

            public void TestResultsUrl(string resultsUrl)
            {
                throw new NotImplementedException();
            }
        }
    }
}