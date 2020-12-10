using System;
using VaporService.Helpers;

namespace VaporService.Storages
{
    class JsonMapper<TValue> : EntityMapper<string, TValue>
    {
        public JsonMapper(Func<string> folderProvider) : base(s => s, value => value.ToBytes(),
            bytes => bytes.FromBytes<TValue>(), folderProvider, s => s)
        {
        }
    }
}