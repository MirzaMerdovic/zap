﻿using Cake.Core.Diagnostics;
using Cake.Frosting;
using Cake.SonarScanner;
using System.Collections.Generic;

namespace Cake.CI.Tasks
{
    [TaskName("Sonar")]
    [IsDependentOn(typeof(TestTask))]
    public sealed class SonarTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.Log.Information($"Working directory: {context.Environment.WorkingDirectory.FullPath}");

            var reportsExist = context.FileSystem.GetDirectory(context.Environment.WorkingDirectory.Combine("reports")).Exists;
            context.Log.Information($"Reports directory exists: {reportsExist}");

            var sourcesDirectory = context.FileSystem.GetDirectory(context.Environment.WorkingDirectory.FullPath.Remove(context.Environment.WorkingDirectory.FullPath.LastIndexOf('/')));

            context.Log.Information($"Source directory: {sourcesDirectory.Path.FullPath}");

            var settings = new SonarScannerSettings
            {
                Debug = true,
                Properties = new Dictionary<string, string>
                {
                    //context.Environment.GetEnvironmentVariable("sonar_scanner_token")
                    ["sonar.login"] = "e11403c2cfe7b3ac9bd38c14f3befc3e47688076",
                    //["sonar.login"] = "d864617b60288b7232fd9f821d959b63e7752b37"
                    ["sonar.host.url"] = "https://sonarcloud.io",
                    ["sonar.sources"] = sourcesDirectory.Path.FullPath,
                    ["sonar.projectBaseDir"] = ".."
                }
            };

            context.SonarScanner(settings);
        }
    }
}
