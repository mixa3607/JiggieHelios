namespace JiggieHelios.Cli.CliTools;

public class ExitStatusException : Exception
{
    public ExitStatusException(int exitCode) : base($"Exit with status {exitCode}")
    {
        ExitCode = exitCode;
    }

    public ExitStatusException(string message, int exitCode) : base(message)
    {
        ExitCode = exitCode;
    }

    public ExitStatusException(string message, int exitCode, Exception innerException) : base(message, innerException)
    {
        ExitCode = exitCode;
    }

    public int ExitCode { get; }
}