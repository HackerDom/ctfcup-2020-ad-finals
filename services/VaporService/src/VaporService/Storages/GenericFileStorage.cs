using System;
using System.IO;
using VaporService.Configuration;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;

namespace VaporService.Storages
{
    internal class GenericFileStorage<TKey, TValue> : FileStorage<TKey, TValue>
    {
        private readonly IEntityMapper<TKey, TValue> _entityMapper;
        private PeriodicalAction _cleanup;
        private readonly ILog _log;
        private readonly ISettingsProvider _settingsProvider;

        public GenericFileStorage(IEntityMapper<TKey, TValue> mapper, ILog log, ISettingsProvider settingsProvider)
        {
            _entityMapper = mapper;
            _log = log;
            _settingsProvider = settingsProvider;

            StartDaemons(settingsProvider);
        }

        private void StartDaemons(ISettingsProvider settingsProvider)
        {
            _cleanup = new PeriodicalAction(() =>
                {
                    if (!Directory.Exists(StorageFolder))
                        return;

                    foreach (var file in Directory.GetFiles(StorageFolder))
                        if (File.GetCreationTimeUtc(file) - DateTime.UtcNow >= settingsProvider.StorageSettings.WeaponTTL)
                            File.Delete(file);
                }, exception => _log.Error(exception, "can't cleanup"),
                () => _settingsProvider.StorageSettings.WeaponStorageCleanupPeriod
            );
            _cleanup.Start();
        }

        protected override string StorageFolder => _entityMapper.FolderProvider();
        protected override string MapKeyToFileName(TKey key) => _entityMapper.KeyToFile(key);
        protected override TKey MapFileNameToKey(string fileName) => _entityMapper.FileToKey(fileName);
        protected override byte[] MapContentToBytes(TValue value) => _entityMapper.EntityToBytes(value);
        protected override TValue MapBytesToContent(byte[] bytesToValue) => _entityMapper.EntityFromBytes(bytesToValue);
    }
}