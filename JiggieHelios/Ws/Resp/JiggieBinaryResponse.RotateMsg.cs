using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    public class RotateMsg : IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public JiggieBinaryCommandType Type { get; set; }
        public ushort UserId { get; set; }
        public IReadOnlyList<Rot> Rotations { get; set; }

        public static RotateMsg Decode(JiggieBinaryCommandType cmdType, BinaryReader reader)
        {
            var rotations = new List<Rot>();
            var msg = new RotateMsg
            {
                Type = cmdType,
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

        public class Rot
        {
            public ushort Id { get; set; }
            public byte Rotation { get; set; }
        }
    }
}