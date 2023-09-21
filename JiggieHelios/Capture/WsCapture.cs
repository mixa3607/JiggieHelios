using System.Net.WebSockets;
using JiggieHelios;
using Websocket.Client;

public class WsCapture : IWsCapture
{
    private readonly Stream _stream;
    private readonly BinaryWriter _writer;
    public int CommandsCount { get; private set; }

    public WsCapture()
    {
        var file = $"./files/caps/{DateTime.Now:yyyy.MM.dd hh.mm.ss}.cap";
        var dir = Path.GetDirectoryName(file)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        _stream = File.OpenWrite(file);
        _writer = new BinaryWriter(_stream);
    }

    public WsCapture(Stream stream)
    {
        _stream = stream;
        _writer = new BinaryWriter(_stream);
    }

    public void Write(DateTimeOffset dt, ResponseMessage msg)
    {
        lock (this)
        {
            CommandsCount++;
            new WsCapturedCommand()
            {
                DateTime = dt,
                FromServer = true,
                MessageType = msg.MessageType,
                BinaryData = msg.MessageType == WebSocketMessageType.Binary ? msg.Binary : null,
                TextData = msg.MessageType == WebSocketMessageType.Text ? msg.Text : null,
            }.Write(_writer);
        }
    }

    public void Write(DateTimeOffset dt, string msg)
    {
        lock (this)
        {
            CommandsCount++;
            new WsCapturedCommand()
            {
                DateTime = dt,
                FromServer = false,
                MessageType = WebSocketMessageType.Text,
                BinaryData = null,
                TextData = msg
            }.Write(_writer);
        }
    }

    public void Write(DateTimeOffset dt, byte[] msg)
    {
        lock (this)
        {
            CommandsCount++;
            new WsCapturedCommand()
            {
                DateTime = dt,
                FromServer = false,
                MessageType = WebSocketMessageType.Binary,
                BinaryData = msg,
                TextData = null
            }.Write(_writer);
        }
    }
}