namespace JiggieHelios.Cli.CliTools;

public interface ICliActionExecutor<T>
{
    Task ExecuteAsync(T args, CancellationToken ct = default);
}