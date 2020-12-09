namespace VaporService.Storages
{
    internal class GenericFileStorage<TKey, TValue> : FileStorage<TKey, TValue>
    {
        private readonly IEntityMapper<TKey, TValue> _entityMapper;

        public GenericFileStorage(IEntityMapper<TKey, TValue> mapper)
        {
            _entityMapper = mapper;
        }

        protected override string StorageFolder => _entityMapper.FolderProvider();

        protected override string MapKeyToFileName(TKey key)
        {
            return _entityMapper.KeyToFile(key);
        }

        protected override byte[] MapContentToBytes(TValue value)
        {
            return _entityMapper.EntityToBytes(value);
        }

        protected override TValue MapBytesToContent(byte[] bytesToValue)
        {
            return _entityMapper.EntityFromBytes(bytesToValue);
        }
    }
}