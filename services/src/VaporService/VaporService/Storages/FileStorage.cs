using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VaporService.Storages
{
    internal abstract class FileStorage<TKey, TValue> : IStorage<TKey, TValue>
    {
        protected abstract string StorageFolder { get; }

        public IEnumerable<TKey> GetKeys() =>
            Directory.GetFiles(StorageFolder).Select(path => MapFileNameToKey(Path.GetFileName(path)));

        public Task Put(TKey key, TValue entity, bool overwrite = true)
        {
            if (!Directory.Exists(StorageFolder)) Directory.CreateDirectory(StorageFolder);

            var path = GetPath(key);
            return File.WriteAllBytesAsync(path, MapContentToBytes(entity));
        }

        public async Task<TValue> Get(TKey key)
        {
            var path = GetPath(key);
            if (!File.Exists(path))
                return default;

            var bytes = await File.ReadAllBytesAsync(path);
            return MapBytesToContent(bytes);
        }

        public async Task Update(TKey key, Action<TValue> update)
        {
            var entity = await Get(key);
            update(entity);
            await Put(key, entity);
        }

        public void Delete(TKey key)
        {
            File.Delete(GetPath(key));
        }

        private string GetPath(TKey key)
        {
            return Path.Combine(StorageFolder, MapKeyToFileName(key));
        }

        protected abstract string MapKeyToFileName(TKey key);
        protected abstract TKey MapFileNameToKey(string fileName);
        protected abstract byte[] MapContentToBytes(TValue value);
        protected abstract TValue MapBytesToContent(byte[] bytesToValue);
    }
}