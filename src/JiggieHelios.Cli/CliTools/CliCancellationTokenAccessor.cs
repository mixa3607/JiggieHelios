namespace JiggieHelios.Cli.CliTools;

public class CliCancellationTokenAccessor
{
    public CancellationToken CancellationToken { get; }
    public CancellationTokenSource CancellationTokenSource { get; }

    public CliCancellationTokenAccessor()
    {
        CancellationTokenSource = new();
        CancellationToken = CancellationTokenSource.Token;
    }
}