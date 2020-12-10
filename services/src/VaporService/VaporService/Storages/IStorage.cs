using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VaporService.Storages
{
    public interface IStorage<TKey, TValue>
    {
        IEnumerable<TKey> GetKeys();
        Task Put(TKey key, TValue entity, bool overwrite = true);
        Task<TValue> Get(TKey key);
        void Delete(TKey key);
        Task Update(TKey key, Action<TValue> update);
    }
}