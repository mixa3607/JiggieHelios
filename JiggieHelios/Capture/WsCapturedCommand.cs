using System.Net.WebSockets;
using System.Text;

public class WsCapturedCommand
{
    public DateTimeOffset DateTime { get; set; }
    public bool FromServer { get; set; }
    public WebSocketMessageType MessageType { get; set; }
    public string? TextData { get; set; }
    public byte[]? BinaryData { get; set; }

    public void Write(BinaryWriter writer)
    {
        writer.Write(DateTime.ToUnixTimeMilliseconds());
        writer.Write(FromServer);
        writer.Write((byte)MessageType);

        if (MessageType == WebSocketMessageType.Binary)
        {
            writer.Write(BinaryData!.Length);
            writer.Write(BinaryData!);
        }
        else if (MessageType == WebSocketMessageType.Text)
        {
            var bytes = Encoding.UTF8.GetBytes(TextData!);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }
        else if (MessageType == WebSocketMessageType.Close)
        {
            // nothing
        }
    }

    public static WsCapturedCommand Read(BinaryReader reader)
    {
        var cmd = new WsCapturedCommand()
        {
            DateTime = DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadInt64()),
            FromServer = reader.ReadBoolean(),
            MessageType = (WebSocketMessageType)reader.ReadByte(),
        };
        if (cmd.MessageType == WebSocketMessageType.Binary)
        {
            var len = reader.ReadInt32();
            cmd.BinaryData = new byte[len];
            _ = reader.Read(cmd.BinaryData);
        }
        else if (cmd.MessageType == WebSocketMessageType.Text)
        {
            var len = reader.ReadInt32();
            var buff = new byte[len];
            _ = reader.Read(buff);
            cmd.TextData = Encoding.UTF8.GetString(buff);
        }
        else if (cmd.MessageType == WebSocketMessageType.Close)
        {
            // nothing
        }

        return cmd;
    }
}