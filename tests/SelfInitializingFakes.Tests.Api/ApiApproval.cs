namespace FakeItEasy.Tests.Approval
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using ApprovalTests;
    using ApprovalTests.Core;
    using ApprovalTests.Reporters;
    using ApprovalTests.Writers;
    using PublicApiGenerator;
    using Xunit;

    public class ApiApproval
    {
        private const string ProjectName = "SelfInitializingFakes";

        [InlineData("net45")]
        [InlineData("netstandard2.0")]
        [InlineData("net5.0")]
        [UseReporter(typeof(DiffReporter))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [Theory]
        public void ApproveApi(string frameworkVersion)
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase!;
            UriBuilder uri = new UriBuilder(new Uri(codeBase));
            string assemblyPath = Uri.UnescapeDataString(uri.Path);
            var containingDirectory = Path.GetDirectoryName(assemblyPath);
            var configurationName = new DirectoryInfo(containingDirectory).Parent.Name;
            var assemblyFile = Path.GetFullPath(
                Path.Combine(
                    GetSourceDirectory(),
                    $"../../src/{ProjectName}/bin/{configurationName}/{frameworkVersion}/{ProjectName}.dll"));

            var assembly = Assembly.LoadFile(Path.GetFullPath(assemblyFile));
            var publicApi = ApiGenerator.GeneratePublicApi(assembly, options: null);

            Approvals.Verify(
                WriterFactory.CreateTextWriter(publicApi),
                new ApprovalNamer(ProjectName, frameworkVersion),
                Approvals.GetReporter());
        }

        private static string GetSourceDirectory([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;

        private class ApprovalNamer : IApprovalNamer
        {
            public ApprovalNamer(string projectName, string frameworkVersion)
            {
                this.Name = frameworkVersion;
                this.SourcePath = Path.Combine(GetSourceDirectory(), "ApprovedApi", projectName);
            }

            public string SourcePath { get; }

            public string Name { get; }
        }
    }
}
