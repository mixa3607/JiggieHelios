using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    [JiggieResponseObject(JiggieBinaryCommandType.RECORD_UPDATE)]
    public class RecordUpdateMsg : IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public JiggieBinaryCommandType Type => JiggieBinaryCommandType.RECORD_UPDATE;
        public required ushort UserId { get; set; }
        public required IReadOnlyList<JukeRecord> Rotations { get; set; }

        public static RecordUpdateMsg Decode(BinaryReader reader)
        {
            var recs = new List<JukeRecord>();
            var msg = new RecordUpdateMsg
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
            public IReadOnlyList<ushort> Pos { get; set; }
        }
    }
}