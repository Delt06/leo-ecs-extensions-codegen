using System.IO;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Core;
using Cake.Frosting;

namespace UpdateDLL;

public static class Program
{
    public static int Main(string[] args) =>
        new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
}

public class BuildContext : FrostingContext
{
    public BuildContext(ICakeContext context) : base(context)
    {
        Root = context.Arguments.GetArgument("root");
        TargetDirectoryPath = context.Arguments.GetArgument("targetPath");
    }

    public string Root { get; }

    public string TargetDirectoryPath { get; }
}

[IsDependentOn(typeof(CopyDllsTask))]
public sealed class Default : FrostingTask { }

[TaskName("Build")]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetBuild(context.Root,
            new DotNetBuildSettings
            {
                Configuration = "Release",
            }
        );
    }
}

[TaskName("Copy DLLs")] [IsDependentOn(typeof(BuildTask))]
public sealed class CopyDllsTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var pattern = Path.Combine(context.Root, "bin/Release/**/*.dll");
        var targetDirectoryPath = context.TargetDirectoryPath;
        context.CreateDirectory(targetDirectoryPath);
        context.CopyFiles(pattern, targetDirectoryPath);
    }
}