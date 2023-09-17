using System.Net.WebSockets;
using JiggieHelios;
using Websocket.Client;

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

public class WsCapture : IWsCapture
{
    private readonly FileStream _stream;
    private readonly BinaryWriter _writer;

    public WsCapture()
    {
        var file = $"./files/caps/{DateTime.Now:yyyy.MM.dd hh.mm.ss}.cap";
        var dir = Path.GetDirectoryName(file)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        _stream = File.OpenWrite(file);
        _writer = new BinaryWriter(_stream);
    }

    public void Write(DateTimeOffset dt, ResponseMessage msg)
    {
        lock (this)
        {
            new WsCapturedCommand()
            {
                DateTime = dt,
                FromServer = true,
                MessageType = msg.MessageType,
                BinaryData = msg.MessageType == WebSocketMessageType.Binary ? msg.Binary : null,
                TextData = msg.MessageType == WebSocketMessageType.Text ? msg.Text : null,
            }.Write(_writer);
            _writer.Flush();
        }
    }

    public void Write(DateTimeOffset dt, string msg)
    {
        lock (this)
        {
            new WsCapturedCommand()
            {
                DateTime = dt,
                FromServer = true,
                MessageType = WebSocketMessageType.Text,
                BinaryData = null,
                TextData = msg
            }.Write(_writer);
            _writer.Flush();
        }
    }

    public void Write(DateTimeOffset dt, byte[] msg)
    {
        lock (this)
        {
            new WsCapturedCommand()
            {
                DateTime = dt,
                FromServer = true,
                MessageType = WebSocketMessageType.Binary,
                BinaryData = msg,
                TextData = null
            }.Write(_writer);
            _writer.Flush();
        }
    }
}