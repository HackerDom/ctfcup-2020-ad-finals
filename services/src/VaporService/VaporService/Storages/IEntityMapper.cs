using System;

namespace VaporService.Storages
{
    internal interface IEntityMapper<TKey, TValue>
    {
        Func<TKey, string> KeyToFile { get; }
        Func<TValue, byte[]> EntityToBytes { get; }
        Func<byte[], TValue> EntityFromBytes { get; }
        Func<string> FolderProvider { get; }
    }
}