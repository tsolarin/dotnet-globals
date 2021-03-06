﻿using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using DotNet.Globals.Core;

namespace DotNet.Globals.Cli
{
    class Program
    {
        public static int Main(string[] args)
        {
            ConsoleLogger logger = new ConsoleLogger();
            PackageOperations packageOperations = PackageOperations.GetInstance(logger, GetApplicationFolder());

            var app = new CommandLineApplication();
            app.Name = "dotnet globals";
            app.FullName = ".NET Core Globals";
            app.Description = "Install and use command line tools built on .NET Core";
            app.HelpOption("-h|--help");
            app.VersionOption("-v|--version", "1.0.0");

            app.Command("install", c =>
            {
                c.Description = "Installs a package";
                c.HelpOption("-h|--help");

                var packageArgument = c.Argument("<PACKAGE>",
                    "The package to install. Can be a NuGet package, a git repo or folder path of a .NET Core project");

                var sourceOption = c.Option("-s|--source", "Specifies a NuGet package source", CommandOptionType.SingleValue);
                var folderOption = c.Option("--folder", "Specifies project folder relative from root of git repo", CommandOptionType.SingleValue);

                c.OnExecute(() =>
                {
                    if (string.IsNullOrEmpty(packageArgument.Value))
                    {
                        logger.LogError("<PACKAGE> argument is required. Use -h|--help to see help");
                        return 1;
                    }

                    try
                    {
                        packageOperations.Install(packageArgument.Value, new Options()
                        {
                            Folder = folderOption.Value(),
                            NuGetPackageSource = sourceOption.HasValue() ? sourceOption.Value() : "https://api.nuget.org/v3/index.json"
                        });
                        logger.LogSuccess("Package install successful");
                        return 0;
                    }
                    catch (System.Exception ex)
                    {
                        logger.LogError(ex.Message);
                        return 1;
                    }
                });
            });

            app.Command("uninstall", c =>
            {
                c.Description = "Uninstall a package";
                c.HelpOption("-h|--help");

                var packageArgument = c.Argument("<PACKAGE>", "The package to uninstall");

                c.OnExecute(() =>
                {
                    if (string.IsNullOrEmpty(packageArgument.Value))
                    {
                        logger.LogError("<PACKAGE> argument is required. Use -h|--help to see help");
                        return 1;
                    }

                    try
                    {
                        packageOperations.Uninstall(packageArgument.Value);
                        logger.LogSuccess("Package removed successfully");
                        return 0;
                    }
                    catch (System.Exception ex)
                    {
                        logger.LogError(ex.Message);
                        return 1;
                    }
                });
            });

            app.Command("update", c =>
            {
                c.Description = "Updates a package";
                c.HelpOption("-h|--help");

                var packageArgument = c.Argument("<PACKAGE>", "The package to update");

                c.OnExecute(() =>
                {
                    if (string.IsNullOrEmpty(packageArgument.Value))
                    {
                        logger.LogError("<PACKAGE> argument is required. Use -h|--help to see help");
                        return 1;
                    }

                    try
                    {
                        packageOperations.Update(packageArgument.Value);
                        logger.LogSuccess($"{packageArgument.Value} updated successfully");
                    }
                    catch (System.Exception ex)
                    {
                        logger.LogError(ex.Message);
                        return 1;
                    }

                    return 0;
                });
            });

            app.Command("list", c =>
            {
                c.Description = "Lists all installed packages";

                c.OnExecute(() =>
                {
                    string[] packages = packageOperations.List();
                    Console.WriteLine(packageOperations.PackagesFolder.FullName);

                    if (packages.Length == 0)
                        logger.LogWarning("No packages installed");
                    else
                        foreach (string package in packages)
                            Console.WriteLine("|-- {0}", package);

                    return 0;
                });
            });

            if (args.Length == 0)
                app.ShowHelp();

            try
            {
                return app.Execute(args);
            }
            catch (System.Exception)
            {
                return 1;
            }
        }

        private static string GetApplicationFolder()
        {
            string userHome = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetEnvironmentVariable("USERPROFILE");
            string applicationFolder = Path.Combine(userHome, ".dotnet-globals");
            if (!Directory.Exists(applicationFolder))
                Directory.CreateDirectory(applicationFolder);
            return applicationFolder;
        }
    }
}
