﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Reflection;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Tests
{
    public static class TestEmbeddedResource
    {
        private static readonly object _writeLock = new object();

        public static void AssertContent(string resourceFile, string actual)
        {
            var expected = GetResourceContent(resourceFile);
#if GENERATE_BASELINES
            // Normalize line endings to '\r\n' for comparison. This removes Environment.NewLine from the equation. Not
            // worth updating files just because we generate baselines on a different system.
            var normalizedPreviousContent = expected?.Replace("\r", "").Replace("\n", "\r\n");
            var normalizedContent = actual.Replace("\r", "").Replace("\n", "\r\n");

            if (!string.Equals(normalizedPreviousContent, normalizedContent, System.StringComparison.Ordinal))
            {
                var solutionRoot = ApplicationPaths.SolutionDirectory;
                var projectName = typeof(TestEmbeddedResource).GetTypeInfo().Assembly.GetName().Name;
                var fullPath = Path.Combine(solutionRoot, "test", GetProjectName(), "Resources", resourceFile);
                lock (_writeLock)
                {
                    // Write content to the file, creating it if necessary.
                    File.WriteAllText(fullPath, actual);
                }
            }
#else
            Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
#endif
        }

        private static string GetResourceContent(string resourceFile)
        {
            resourceFile = $"{GetProjectName()}.Resources.{resourceFile}";
            var assembly = typeof(TestEmbeddedResource).GetTypeInfo().Assembly;
            using (var streamReader = new StreamReader(assembly.GetManifestResourceStream(resourceFile)))
            {
                // Normalize line endings to '\r\n' (CRLF). This removes core.autocrlf, core.eol, core.safecrlf, and
                // .gitattributes from the equation and treats "\r\n" and "\n" as equivalent. Does not handle
                // some line endings like "\r" but otherwise ensures checksums and line mappings are consistent.
                return streamReader.ReadToEnd().Replace("\r", "").Replace("\n", "\r\n");
            }
        }

        private static string GetProjectName() => typeof(TestEmbeddedResource).GetTypeInfo().Assembly.GetName().Name;
    }
}