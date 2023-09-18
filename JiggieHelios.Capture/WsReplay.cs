public class WsReplay
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
}