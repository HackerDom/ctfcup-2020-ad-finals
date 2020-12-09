using System;
using System.Threading.Tasks;

namespace VaporService.Storages
{
    public interface IStorage<in TKey, TValue>
    {
        Task Put(TKey key, TValue entity, bool overwrite = true);
        Task<TValue> Get(TKey key);
        void Delete(TKey key);
        Task Update(TKey key, Action<TValue> update);
    }
}