using System.Reflection;
using PowerArgs;

namespace JiggieHelios.Cli.CliTools;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class VersionHook<T> : ArgHook
{
    public static class CustomAttributesName
    {
        public const string RepoUrl = "REPO_URL";
        public const string ProjectUrl = "PROJECT_URL";

        /// <summary>
        /// branch or tag
        /// </summary>
        public const string GitRef = "GIT_REF";

        /// <summary>
        /// branch or tag
        /// </summary>
        public const string GitRefType = "GIT_REF_TYPE";

        public const string GitCommitSha = "GIT_COMMIT_SHA";
        public const string BuildDate = "BUILD_DATE";
    }

    private CommandLineArgument? _target;
    private bool _iDidTheCancel;

    /// <summary>
    /// If true (which it is by default) the hook will write the help after the target property is populated.  If false, processing will still stop, but
    /// the help will not be written (yoy will have to do it yourself).
    /// </summary>
    public bool WriteVersion { get; set; }

    public VersionHook()
    {
        WriteVersion = true;
        AfterCancelPriority = 0;
    }

    /// <summary>Makes sure the target is a boolean</summary>
    /// <param name="context">Context passed by the parser</param>
    public override void BeforePopulateProperty(HookContext context)
    {
        base.BeforePopulateProperty(context);
        _target = context.CurrentArgument;
        if (context.CurrentArgument.ArgumentType != typeof(bool))
            throw new InvalidArgDefinitionException(nameof(ArgHook) + " attributes can only be used with boolean properties or parameters");
    }

    /// <summary>
    /// This gets called after the target property is populated.  It cancels processing.
    /// </summary>
    /// <param name="context">Context passed by the parser</param>
    public override void AfterPopulateProperty(HookContext context)
    {
        _iDidTheCancel = false;
        base.AfterPopulateProperty(context);
        if (context.CurrentArgument.RevivedValue is not true)
            return;
        _iDidTheCancel = true;
        context.CancelAllProcessing();
    }

    /// <summary>Writes the help as long as WriteHelp is true</summary>
    /// <param name="context">Context passed by the parser</param>
    public override void AfterCancel(HookContext context)
    {
        base.AfterCancel(context);
        if (!_iDidTheCancel || !WriteVersion)
            return;
        var assembly = typeof(T).Assembly;
        var attrs = assembly?.GetCustomAttributes<AssemblyMetadataAttribute>().ToArray();
        var versionAttr = assembly?.GetCustomAttribute<AssemblyFileVersionAttribute>();

        var template = "{{ExeName Cyan !}} version {{Version Green!}}, commit {{GitCommitSha!}}\r\n" +
                       "{{if HasGitTag}}Tag: {{GitTag Green!}}\r\n!{{if}}" +
                       "{{if HasGitRef}}Branch: {{GitRef Red!}}\r\n!{{if}}" +
                       "Build: {{BuildDate DarkGreen!}}\r\n" +
                       "Project: {{ProjectUrl Green!}}\r\n" +
                       "Repo: {{RepoUrl Green!}}\r\n";

        var refType = attrs?.FirstOrDefault(x => x.Key == CustomAttributesName.GitRefType)?.Value ?? "branch";
        var refName = attrs?.FirstOrDefault(x => x.Key == CustomAttributesName.GitRef)?.Value ?? "";

        var gitRef = refType == "branch" ? refName : null;
        var gitTag = refType == "tag" ? refName : null;

        var data = new
        {
            ExeName = Path.GetFileName(Environment.ProcessPath),
            RepoUrl = attrs?.FirstOrDefault(x => x.Key == CustomAttributesName.RepoUrl)?.Value,
            ProjectUrl = attrs?.FirstOrDefault(x => x.Key == CustomAttributesName.ProjectUrl)?.Value,
            GitRef = gitRef,
            GitTag = gitTag,
            GitCommitSha = attrs?.FirstOrDefault(x => x.Key == CustomAttributesName.GitCommitSha)?.Value,
            BuildDate = attrs?.FirstOrDefault(x => x.Key == CustomAttributesName.BuildDate)?.Value,
            HasGitTag = gitTag != null,
            HasGitRef = gitRef != null,
            versionAttr?.Version
        };
        new DocumentRenderer().Render(template, data).Write();
    }
}