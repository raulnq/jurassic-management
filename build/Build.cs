using System;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Docker.DockerTasks;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Docker;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.StartEnv);

    string DockerPrefix = "jurassic";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;

    AbsolutePath SourceDirectory => RootDirectory / "src" / "server";

    AbsolutePath PublishDirectory => RootDirectory / "publish";

    string MigratorProject = "Migrator";

    Target CompileMigrator => _ => _
    .Executes(() =>
    {
        DotNetBuild(s => s
            .SetProjectFile(SourceDirectory / MigratorProject)
            .SetConfiguration(Configuration));
    });

    Target PublishMigrator => _ => _
    .DependsOn(CompileMigrator)
    .Executes(() =>
    {
        DotNetPublish(s => s
            .SetProject(SourceDirectory / MigratorProject)
            .SetConfiguration(Configuration)
            .DisableNoBuild()
            .DisableNoRestore()
            .SetOutput(PublishDirectory / MigratorProject));
    });

    Target RunMigrator => _ => _
    .DependsOn(PublishMigrator)
    .Executes(() =>
    {
        var executable = PublishDirectory / MigratorProject / $"{MigratorProject}.dll";

        DotNet(executable.ToString());
    });

    string StorageBlobPort = "10000";
    string StorageQueuePort = "10001";
    string StorageTablePort = "10002";

    Target RunOrStartStorage => _ => _
        .Executes(() =>
        {
            try
            {
                DockerRun(x => x
                .SetName($"{DockerPrefix}-storage")
                .SetImage("mcr.microsoft.com/azure-storage/azurite")
                .EnableDetach()
                .AddPublish($"{StorageBlobPort}:10000")
                .AddPublish($"{StorageQueuePort}:10001")
                .AddPublish($"{StorageTablePort}:10002")
                );
            }
            catch (Exception)
            {
                DockerStart(x => x
                .AddContainers($"{DockerPrefix}-storage")
                );
            }
        });

    Target StopStorage => _ => _
        .Executes(() =>
        {
            DockerStop(x => x
            .AddContainers($"{DockerPrefix}-storage")
            );
        });

    Target RemoveStorage => _ => _
    .DependsOn(StopStorage)
    .Executes(() =>
    {
        DockerRm(x => x
        .AddContainers($"{DockerPrefix}-storage")
        );
    });

    string SQLPort = "1431";
    string SQLPassword = "Sqlserver123$";

    Target RunOrStartSQLServer => _ => _
    .Executes(() =>
    {
        try
        {
            DockerRun(x => x
            .SetName($"{DockerPrefix}-sqlserver")
            .AddEnv("ACCEPT_EULA=Y", $"MSSQL_SA_PASSWORD={SQLPassword}")
            .SetImage("mcr.microsoft.com/mssql/server:2019-CU14-ubuntu-20.04")
            .EnableDetach()
            .SetPublish($"{SQLPort}:1433")
            );
        }
        catch (Exception)
        {
            DockerStart(x => x
            .AddContainers($"{DockerPrefix}-sqlserver")
            );
        }
    });

    Target StopSQLServer => _ => _
    .Executes(() =>
    {
        DockerStop(x => x
        .AddContainers($"{DockerPrefix}-sqlserver")
        );
    });

    Target RemoveSQLServer => _ => _
    .DependsOn(StopSQLServer)
    .Executes(() =>
    {
        DockerRm(x => x
        .AddContainers($"{DockerPrefix}-sqlserver")
        );
    });

    string SEQPort = "5339";

    Target RunOrStartSeq => _ => _
    .Executes(() =>
    {
        try
        {
            DockerRun(x => x
            .SetName($"{DockerPrefix}-seq")
            .AddEnv("ACCEPT_EULA=Y")
            .SetRestart("unless-stopped")
            .SetImage("datalust/seq:latest")
            .EnableDetach()
            .SetPublish($"{SEQPort}:80")
            );
        }
        catch (Exception)
        {
            DockerStart(x => x
            .AddContainers($"{DockerPrefix}-seq")
            );
        }
    });

    Target StopSeq => _ => _
    .Executes(() =>
    {
        DockerStop(x => x
        .AddContainers($"{DockerPrefix}-seq")
        );
    });

    Target RemoveSeq => _ => _
    .DependsOn(StopSeq)
    .Executes(() =>
    {
        DockerRm(x => x
        .AddContainers($"{DockerPrefix}-seq")
        );
    });

    Target StartEnv => _ => _
    .DependsOn(RunOrStartSQLServer)
    .DependsOn(RunOrStartSeq)
    .DependsOn(RunOrStartStorage)
    .DependsOn(RunOrStartRabbitMQ)
    .Executes(() =>
    {
        Serilog.Log.Information("Development env started");
    });

    Target StopEnv => _ => _
    .DependsOn(StopSQLServer)
    .DependsOn(StopSeq)
    .DependsOn(StopStorage)
    .DependsOn(StopRabbitMQ)
    .Executes(() =>
    {
        Serilog.Log.Information("Development env stoped");
    });

    Target RemoveEnv => _ => _
    .DependsOn(RemoveSQLServer)
    .DependsOn(RemoveSeq)
    .DependsOn(RemoveStorage)
    .DependsOn(RemoveRabbitMQ)
    .Executes(() =>
    {
        Serilog.Log.Information("Development env removed");
    });

    string RabbitMQUser = "admin";
    string RabbitMQPassword = "Rabbitmq123$";
    string RabbitMQAdminPort = "15671";
    string RabbitMQPort = "5671";

    Target RunOrStartRabbitMQ => _ => _
    .Executes(() =>
    {
        try
        {
            DockerRun(x => x
            .SetName($"{DockerPrefix}-rabbitmq")
            .SetHostname($"{DockerPrefix}-host")
            .AddEnv($"RABBITMQ_DEFAULT_USER={RabbitMQUser}", $"RABBITMQ_DEFAULT_PASS={RabbitMQPassword}")
            .SetImage("rabbitmq:3-management")
            .EnableDetach()
            .AddPublish($"{RabbitMQAdminPort}:15672")
            .AddPublish($"{RabbitMQPort}:5672")
            );
        }
        catch (Exception)
        {
            DockerStart(x => x
            .AddContainers($"{DockerPrefix}-rabbitmq")
            );
        }
    });

    Target StopRabbitMQ => _ => _
    .Executes(() =>
    {
        DockerStop(x => x
        .AddContainers($"{DockerPrefix}-rabbitmq")
        );
    });

    Target RemoveRabbitMQ => _ => _
    .DependsOn(StopRabbitMQ)
    .Executes(() =>
    {
        DockerRm(x => x
        .AddContainers($"{DockerPrefix}-rabbitmq")
        );
    });
}
