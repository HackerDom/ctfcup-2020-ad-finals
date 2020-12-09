using System;

namespace VaporService.Storages
{
    internal class EntityMapper<TKey, TValue> : IEntityMapper<TKey, TValue>
    {
        public EntityMapper(Func<TKey, string> keyToFile, Func<TValue, byte[]> entityToBytes,
            Func<byte[], TValue> entityFromBytes, Func<string> folderProvider)
        {
            KeyToFile = keyToFile;
            EntityToBytes = entityToBytes;
            EntityFromBytes = entityFromBytes;
            FolderProvider = folderProvider;
        }

        public Func<TKey, string> KeyToFile { get; }

        public Func<TValue, byte[]> EntityToBytes { get; }

        public Func<byte[], TValue> EntityFromBytes { get; }

        public Func<string> FolderProvider { get; }
    }
}