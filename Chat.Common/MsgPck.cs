using MessagePack;

namespace Chat.Common;

public class MsgPck {
    public static byte[] Serialize<T>(T data) {
        return MessagePackSerializer.Serialize(data);
    }
    public static T Deserialize<T>(byte[] data) {
        return MessagePackSerializer.Deserialize<T>(data);
    }
}