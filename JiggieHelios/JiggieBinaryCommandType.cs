namespace JiggieHelios;

public enum JiggieBinaryCommandType
{
    PICK = 0x1,
    MOVE = 0x2,
    DROP = 0x3,
    SELECT = 0x4,
    DESELECT = 0x5,
    MERGE = 0x6,
    ROTATE = 0x7,
    LOCK = 0x8,
    UNLOCK = 0x9,
    STEAL = 0xA,
    HEARTBEAT = 0xB,
    RECORDSOLVED = 0x40,
    RECORD_UPDATE = 0x60,
}