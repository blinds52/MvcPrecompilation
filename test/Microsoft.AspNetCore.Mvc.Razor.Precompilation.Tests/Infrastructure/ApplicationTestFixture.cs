// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.DotNet.Cli.Utils;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Tests
{
    public abstract class ApplicationTestFixture : IDisposable
    {
        private const string NuGetPackagesEnvironmentKey = "NUGET_PACKAGES";
        private readonly string _oldRestoreDirectory;

        protected ApplicationTestFixture(string applicationName)
        {
            ApplicationName = applicationName;
            _oldRestoreDirectory = Environment.GetEnvironmentVariable(NuGetPackagesEnvironmentKey);
            RestoreApplication();
        }

        public string ApplicationName { get; }

        public string ApplicationPath => ApplicationPaths.GetTestAppDirectory(ApplicationName);

        public string TempRestoreDirectory { get; } = CreateTempRestoreDirectory();

        public void UseTempRestoreDirectory()
        {
            Environment.SetEnvironmentVariable(NuGetPackagesEnvironmentKey, TempRestoreDirectory);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(NuGetPackagesEnvironmentKey, _oldRestoreDirectory);
            try
            {
                Directory.Delete(TempRestoreDirectory, recursive: true);
            }
            catch (IOException)
            {
                // Ignore delete failures.
            }
        }

        private void RestoreApplication()
        {
            var packagesDirectory = GetNuGetPackagesDirectory();
            var args = new[]
            {
                Path.Combine(ApplicationPath, "project.json"),
                "-s",
                packagesDirectory,
                "-s",
                ApplicationPaths.ArtifactPackagesDirectory,
                "-s",
                @"D:\work\katana\Mvc\artifacts\build",
                "--packages",
                TempRestoreDirectory,
            };

            var commandResult = Command
                .CreateDotNet("restore", args)
                .CaptureStdOut()
                .CaptureStdErr()
                .Execute();

            Assert.True(commandResult.ExitCode == 0,
                string.Join(Environment.NewLine,
                    $"dotnet {commandResult.StartInfo.Arguments} exited with {commandResult.ExitCode}.",
                    commandResult.StdOut,
                    commandResult.StdErr));

            Console.WriteLine(commandResult.StdOut);
        }

        private static string CreateTempRestoreDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            return Directory.CreateDirectory(path).FullName;
        }

        private static string GetNuGetPackagesDirectory()
        {
            var nugetFeed = Environment.GetEnvironmentVariable(NuGetPackagesEnvironmentKey);
            string basePath;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                basePath = Environment.GetEnvironmentVariable("USERPROFILE");
            }
            else
            {
                basePath = Environment.GetEnvironmentVariable("HOME");
            }

            return Path.Combine(basePath, ".nuget", "packages");
        }
    }
}
