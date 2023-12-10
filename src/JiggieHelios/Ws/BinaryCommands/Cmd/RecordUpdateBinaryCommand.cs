using JiggieHelios.Ws.Requests;
using JiggieHelios.Ws.Responses;

namespace JiggieHelios.Ws.BinaryCommands.Cmd;

[JiggieBinaryObject(BinaryCommandType.RECORD_UPDATE)]
public class RecordUpdateBinaryCommand : IJiggieBinaryObject
{
    public JiggieResponseType ResponseType => JiggieResponseType.Binary;
    public JiggieRequestType RequestType => JiggieRequestType.Binary;
    public BinaryCommandType Type => BinaryCommandType.RECORD_UPDATE;
    public required ushort UserId { get; set; }
    public required IReadOnlyList<JukeRecord> Rotations { get; set; }

    public static RecordUpdateBinaryCommand Decode(BinaryReader reader)
    {
        var type = reader.ReadByte();
        var recs = new List<JukeRecord>();
        var msg = new RecordUpdateBinaryCommand
        {
            UserId = reader.ReadUInt16(),
            Rotations = recs,
        };
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            recs.Add(new JukeRecord()
            {
                Id = reader.ReadUInt16(),
                Pos = new[] { reader.ReadUInt16(), reader.ReadUInt16() }
            });
        }

        return msg;
    }

    public class JukeRecord
    {
        public ushort Id { get; set; }
        public IReadOnlyList<ushort> Pos { get; set; } = Array.Empty<ushort>();
    }
}