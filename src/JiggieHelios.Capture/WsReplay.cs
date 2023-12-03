namespace JiggieHelios.Capture;

public class WsReplay: IDisposable
{
    private readonly FileStream _stream;
    private readonly BinaryReader _reader;


    public WsReplay(string filePath)
    {
        _stream = File.OpenRead(filePath);
        _reader = new BinaryReader(_stream);
    }

    public bool Available()
    {
        return _stream.Position < _stream.Length;
    }

    public WsCapturedCommand? Read()
    {
        if (!Available())
        {
            return null;
        }

        return WsCapturedCommand.Read(_reader);
    }

    public IEnumerable<WsCapturedCommand> GetEnumerator()
    {
        while (Available())
        {
            yield return WsCapturedCommand.Read(_reader);
        }
    }

    public void Dispose()
    {
        _stream.Dispose();
        _reader.Dispose();
    }
}