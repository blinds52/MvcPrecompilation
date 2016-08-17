// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Tests
{
    public abstract class TestAppData
    {
        public TestAppData(string applicationName)
        {
            ApplicationName = applicationName;
        }

        public string ApplicationName { get; }

        public string ApplicationPath => ApplicationPaths.GetTestAppDirectory(ApplicationName);
    }
}
