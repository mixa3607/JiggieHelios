using JiggieHelios;

public static class JiggieBinaryResponse
{
    public class GroupsMsg : IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Json;
        public JiggieBinaryCommandType Type { get; set; }
        public ushort UserId { get; set; }
        public IReadOnlyList<Group> Groups { get; set; }

        public static GroupsMsg Decode(JiggieBinaryCommandType cmdType, BinaryReader reader)
        {
            var groups = new List<Group>();
            var msg = new GroupsMsg
            {
                Type = cmdType,
                UserId = reader.ReadUInt16(),
                Groups = groups,
            };
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                groups.Add(new Group()
                {
                    Id = reader.ReadUInt16(),
                    X = reader.ReadSingle(),
                    Y = reader.ReadSingle(),
                });
            }

            return msg;
        }


        public class Group
        {
            public ushort Id { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
        }
    }

    public class GroupIdsMsg : IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Json;
        public JiggieBinaryCommandType Type { get; set; }
        public ushort UserId { get; set; }
        public IReadOnlyList<ushort> GroupIds { get; set; }

        public static GroupIdsMsg Decode(JiggieBinaryCommandType cmdType, BinaryReader reader)
        {
            var groups = new List<ushort>();
            var msg = new GroupIdsMsg
            {
                Type = cmdType,
                UserId = reader.ReadUInt16(),
                GroupIds = groups,
            };
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                groups.Add(reader.ReadUInt16());
            }

            return msg;
        }
    }

    public class MergeMsg : IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Json;
        public JiggieBinaryCommandType Type { get; set; }
        public ushort UserId { get; set; }
        public ushort GroupIdA { get; set; }
        public ushort GroupIdB { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public static MergeMsg Decode(JiggieBinaryCommandType cmdType, BinaryReader reader)
        {
            var msg = new MergeMsg
            {
                Type = cmdType,
                UserId = reader.ReadUInt16(),
                GroupIdA = reader.ReadUInt16(),
                GroupIdB = reader.ReadUInt16(),
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
            };

            return msg;
        }
    }

    public class RotateMsg : IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Json;
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

    public class RecordUpdateMsg : IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Json;
        public required JiggieBinaryCommandType Type { get; set; }
        public required ushort UserId { get; set; }
        public required IReadOnlyList<JukeRecord> Rotations { get; set; }

        public static RecordUpdateMsg Decode(JiggieBinaryCommandType cmdType, BinaryReader reader)
        {
            var recs = new List<JukeRecord>();
            var msg = new RecordUpdateMsg
            {
                Type = cmdType,
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

    public class HeartbeatMsg : IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Json;
        public required JiggieBinaryCommandType Type { get; set; }
        public required ushort UserId { get; set; }

        public static HeartbeatMsg Decode(JiggieBinaryCommandType cmdType, BinaryReader reader)
        {
            var msg = new HeartbeatMsg
            {
                Type = cmdType,
                UserId = reader.ReadUInt16(),
            };

            return msg;
        }
    }

    public class RawMsg : IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Json;
        public required JiggieBinaryCommandType Type { get; set; }
        public required ushort UserId { get; set; }
        public required byte[] RawPayload { get; set; }

        public static RawMsg Decode(JiggieBinaryCommandType cmdType, BinaryReader reader)
        {
            var msg = new RawMsg
            {
                Type = cmdType,
                UserId = reader.ReadUInt16(),
                RawPayload = new byte[reader.BaseStream.Length - reader.BaseStream.Position]
            };
            var read = reader.Read(msg.RawPayload);

            return msg;
        }
    }
}