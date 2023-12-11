namespace JiggieHelios.Replay.Selenium;

public class ReplayStateSynchronization
{
    public SemaphoreSlim SendingStartedSemaphore { get; set; } = new SemaphoreSlim(1, 1);
    public SemaphoreSlim SendingFinishedSemaphore { get; set; } = new SemaphoreSlim(1, 1);
    public SemaphoreSlim SendingErrorSemaphore { get; set; } = new SemaphoreSlim(1, 1);

    public ReplayStateSynchronization()
    {
        SendingStartedSemaphore.Wait();
        SendingFinishedSemaphore.Wait();
        SendingErrorSemaphore.Wait();
    }
}