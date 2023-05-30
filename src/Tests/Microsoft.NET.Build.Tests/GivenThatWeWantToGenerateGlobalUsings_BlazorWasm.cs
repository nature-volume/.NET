// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using FluentAssertions;
using Microsoft.NET.TestFramework;
using Microsoft.NET.TestFramework.Assertions;
using Microsoft.NET.TestFramework.Commands;
using Microsoft.NET.TestFramework.ProjectConstruction;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.NET.Build.Tests
{
    public class GivenThatWeWantToGenerateGlobalUsings_BlazorWasm : SdkTest
    {
        public GivenThatWeWantToGenerateGlobalUsings_BlazorWasm(ITestOutputHelper log) : base(log) { }

        [RequiresMSBuildVersionFact("17.0.0.32901")]
        public void It_generates_blazorwasm_usings_and_builds_successfully()
        {
            var tfm = ToolsetInfo.CurrentTargetFramework;
            var testProject = CreateTestProject(tfm);
            testProject.AdditionalProperties["ImplicitUsings"] = "enable";
            var testAsset = _testAssetsManager.CreateTestProject(testProject);
            var globalUsingsFileName = $"{testAsset.TestProject.Name}.GlobalUsings.g.cs";

            var buildCommand = new BuildCommand(testAsset);
            buildCommand
                .Execute()
                .Should()
                .Pass();

            var outputDirectory = buildCommand.GetIntermediateDirectory(tfm);

            outputDirectory.Should().HaveFile(globalUsingsFileName);

            File.ReadAllText(Path.Combine(outputDirectory.FullName, globalUsingsFileName)).Should().Be(
@"// <auto-generated/>
global using global::Microsoft.Extensions.Configuration;
global using global::Microsoft.Extensions.DependencyInjection;
global using global::Microsoft.Extensions.Logging;
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
");
        }

        [Fact]
        public void It_can_disable_blazorwasm_usings()
        {
            var tfm = ToolsetInfo.CurrentTargetFramework;
            var testProject = CreateTestProject(tfm);
            testProject.AdditionalProperties["ImplicitUsings"] = "disable";
            var testAsset = _testAssetsManager.CreateTestProject(testProject);
            var globalUsingsFileName = $"{testAsset.TestProject.Name}.GlobalUsings.g.cs";

            var buildCommand = new BuildCommand(testAsset);
            buildCommand
                .Execute()
                .Should()
                .Fail();

            var outputDirectory = buildCommand.GetIntermediateDirectory(tfm);

            outputDirectory.Should().NotHaveFile(globalUsingsFileName);
        }

        private TestProject CreateTestProject(string tfm)
        {
            var testProject = new TestProject
            {
                IsExe = true,
                TargetFrameworks = tfm,
                ProjectSdk = "Microsoft.NET.Sdk.BlazorWebAssembly"
            };
            testProject.PackageReferences.Add(new TestPackageReference("Microsoft.AspNetCore.Components.WebAssembly", "5.0.0"));
            testProject.SourceFiles["Program.cs"] = @"
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
";
            return testProject;
        }
    }
}
