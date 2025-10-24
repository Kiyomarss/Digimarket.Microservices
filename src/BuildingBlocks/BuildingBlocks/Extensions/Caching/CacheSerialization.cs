using MessagePack;

namespace BuildingBlocks.Extensions.Caching;

public static class CacheSerialization
{
    public static byte[] ToBytes<T>(T value) =>
        MessagePackSerializer.Serialize(value);

    public static T? FromBytes<T>(byte[]? bytes) =>
        bytes == null ? default : MessagePackSerializer.Deserialize<T>(bytes);
}