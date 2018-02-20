﻿namespace Fixie.Tests.Execution
{
    using System;
    using Assertions;
    using Fixie.Execution;
    using static Utility;

    public class CaseFailedTests
    {
        public void ShouldSummarizeAnyGivenException()
        {
            var exception = GetException();

            var @case = Case("Test");
            @case.Fail(exception);
            var failure = new CaseFailed(@case);

            failure.Exception.ShouldBeType<PrimaryException>();
            failure.Exception.Message.ShouldEqual("Primary Exception!");

            failure.StackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldEqual(
                    At<CaseFailedTests>("GetException()"),
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by Zero Exception!",
                    At<CaseFailedTests>("GetException()"));
        }

        class SampleTestClass
        {
            public void Test() { }
        }

        static Case Case(string methodName, params object[] parameters)
            => Case<SampleTestClass>(methodName, parameters);

        static Case Case<TTestClass>(string methodName, params object[] parameters)
            => new Case(typeof(TTestClass).GetInstanceMethod(methodName), parameters);

        static Exception GetException()
        {
            try
            {
                try
                {
                    throw new DivideByZeroException("Divide by Zero Exception!");
                }
                catch (Exception exception)
                {
                    throw new PrimaryException(exception);
                }
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        class PrimaryException : Exception
        {
            public PrimaryException(Exception innerException)
                : base("Primary Exception!", innerException) { }
        }
    }
}