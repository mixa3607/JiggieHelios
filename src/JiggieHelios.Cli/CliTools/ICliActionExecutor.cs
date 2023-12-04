namespace JiggieHelios.Cli.CliTools;

public interface ICliActionExecutor
{
    Type ArgType { get; }
}
public interface ICliActionExecutor<T>: ICliActionExecutor
{
    Task ExecuteAsync(T args, CancellationToken ct = default);
}
