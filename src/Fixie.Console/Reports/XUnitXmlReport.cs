﻿namespace Fixie.ConsoleRunner.Reports
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using Execution;

    public class XUnitXmlReport
    {
        public XDocument Transform(Report report)
        {
            return new XDocument(
                new XElement("assemblies",
                    report.Assemblies.Select(Assembly)));
        }

        static XElement Assembly(AssemblyReport assemblyReport)
        {
            var now = DateTime.UtcNow;

            return new XElement("assembly",
                new XAttribute("name", assemblyReport.Location),
                new XAttribute("run-date", now.ToString("yyyy-MM-dd")),
                new XAttribute("run-time", now.ToString("HH:mm:ss")),
                new XAttribute("configFile", AppDomain.CurrentDomain.SetupInformation.ConfigurationFile),
                new XAttribute("time", Seconds(assemblyReport.Duration)),
                new XAttribute("total", assemblyReport.Total),
                new XAttribute("passed", assemblyReport.Passed),
                new XAttribute("failed", assemblyReport.Failed),
                new XAttribute("skipped", assemblyReport.Skipped),
                new XAttribute("environment", $"{IntPtr.Size*8}-bit .NET {Environment.Version}"),
                new XAttribute("test-framework", Framework.Version),
                assemblyReport.Classes.Select(Class));
        }

        static XElement Class(ClassReport classReport)
        {
            return new XElement("class",
                new XAttribute("time", Seconds(classReport.Duration)),
                new XAttribute("name", classReport.Name),
                new XAttribute("total", classReport.Failed + classReport.Passed + classReport.Skipped),
                new XAttribute("passed", classReport.Passed),
                new XAttribute("failed", classReport.Failed),
                new XAttribute("skipped", classReport.Skipped),
                classReport.Cases.Select(Case));
        }

        static XElement Case(CaseCompleted message)
        {
            var @case = new XElement("test",
                new XAttribute("name", message.Name),
                new XAttribute("type", message.MethodGroup.Class),
                new XAttribute("method", message.MethodGroup.Method),
                new XAttribute("result",
                    message.Status == CaseStatus.Failed
                        ? "Fail"
                        : message.Status == CaseStatus.Passed
                            ? "Pass"
                            : "Skip"));

            if (message.Status != CaseStatus.Skipped)
                @case.Add(new XAttribute("time", Seconds(message.Duration)));

            if (message.Status == CaseStatus.Skipped)
            {
                var skip = (CaseSkipped)message;
                if (skip.Reason != null)
                    @case.Add(new XElement("reason", new XElement("message", new XCData(skip.Reason))));
            }

            if (message.Status == CaseStatus.Failed)
            {
                var fail = (CaseFailed)message;
                @case.Add(
                    new XElement("failure",
                        new XAttribute("exception-type", fail.Exception.Type),
                        new XElement("message", new XCData(fail.Exception.Message)),
                        new XElement("stack-trace", new XCData(fail.Exception.StackTrace))));
            }

            return @case;
        }

        static string Seconds(TimeSpan duration)
        {
            //The XML Schema spec requires decimal values to use a culture-ignorant format.
            return duration.TotalSeconds.ToString("0.000", NumberFormatInfo.InvariantInfo);
        }
    }
}