// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Tests
{
    public class SimpleAppTest : IClassFixture<SimpleAppTest.SimpleAppTestFixture>
    {
        public SimpleAppTest(SimpleAppTestFixture fixture)
        {
            Fixture = fixture;
        }

        public ApplicationTestFixture Fixture { get; }

        [Theory]
        [InlineData(RuntimeFlavor.Clr)]
        [InlineData(RuntimeFlavor.CoreClr)]
        public async Task Precompilation_WorksForSimpleApps(RuntimeFlavor flavor)
        {
            // Arrange
            var applicationPath = ApplicationPaths.GetTestAppDirectory("SimpleApp");

            var logger = new LoggerFactory()
                .AddConsole()
                .CreateLogger($"{nameof(SimpleAppTest)}:{flavor}");

            var deploymentParameters = new DeploymentParameters(
                applicationPath,
                ServerType.Kestrel,
                flavor,
                RuntimeArchitecture.x64)
            {
                PublishApplicationBeforeDeployment = true,
                TargetFramework = flavor == RuntimeFlavor.Clr ? "net451" : "netcoreapp1.0",
                Configuration = "Release",
            };

            // Act
            Fixture.UseTempRestoreDirectory();
            using (var deployer = ApplicationDeployerFactory.Create(deploymentParameters, logger))
            {
                var deploymentResult = deployer.Deploy();
                var httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(deploymentResult.ApplicationBaseUri)
                };

                var response = await httpClient.GetStringAsync("");

                // Assert
                TestEmbeddedResource.AssertContent("SimpleAppTest.Home.Index.txt", response);
            }
        }

        public class SimpleAppTestFixture : ApplicationTestFixture
        {
            public SimpleAppTestFixture()
                : base("SimpleApp")
            {
            }
        }
    }
}
