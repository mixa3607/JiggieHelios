using JiggieHelios.Ws.Requests;
using JiggieHelios.Ws.Responses;

namespace JiggieHelios.Ws.BinaryCommands.Cmd;

[JiggieBinaryObject(BinaryCommandType.ROTATE)]
public class RotateBinaryCommand : IJiggieBinaryObject
{
    public JiggieResponseType ResponseType => JiggieResponseType.Binary;
    public JiggieRequestType RequestType => JiggieRequestType.Binary;
    public BinaryCommandType Type => BinaryCommandType.ROTATE;
    public ushort UserId { get; set; }
    public IReadOnlyList<Rot> Rotations { get; set; } = Array.Empty<Rot>();

    public static RotateBinaryCommand Decode(BinaryReader reader)
    {
        var type = reader.ReadByte();
        var rotations = new List<Rot>();
        var msg = new RotateBinaryCommand
        {
            UserId = reader.ReadUInt16(),
            Rotations = rotations,
        };
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            rotations.Add(new Rot()
            {
                Id = reader.ReadUInt16(),
                Rotation = reader.ReadByte()
            });
        }

        return msg;
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write((byte)Type);
        writer.Write(UserId);
        foreach (var rotation in Rotations)
        {
            writer.Write(rotation.Id);
            writer.Write(rotation.Rotation);
        }
    }

    public class Rot
    {
        public ushort Id { get; set; }
        public byte Rotation { get; set; }
    }
}