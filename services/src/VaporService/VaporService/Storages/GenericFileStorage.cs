using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using VaporService.Configuration;
using VaporService.Helpers;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;

namespace VaporService.Storages
{
    public class Weapon
    {
        public string Name { get; set; }
        public bool IsVorpal { get; set; }
        public string Property { get; set; }
        public string ArcaneProperty { get; set; }
    }

    class WeaponMeta
    {
        public string Owner;
        public DateTime ExpireAt { get; set; }
    }

    public interface IClaimedWeaponIndex
    {
        bool ClaimWeapon(string userName, string weaponName);
        bool IsClaimed(string weaponName);
        bool IsOwner(string userName, string weaponName);
    }

    class ClaimedWeaponIndex : IClaimedWeaponIndex
    {
        private ConcurrentDictionary<string, WeaponMeta> weaponByName;
        private readonly ILog _log;
        private readonly AsyncPeriodicalAction _dump;
        private ISettingsProvider _settingsProvider;

        public ClaimedWeaponIndex(ISettingsProvider settingsProvider, ILog log)
        {
            _settingsProvider = settingsProvider;
            _log = log;
            weaponByName = new ConcurrentDictionary<string, WeaponMeta>();
            _dump = new AsyncPeriodicalAction(
                async () =>
                {
                    await File.WriteAllTextAsync(settingsProvider.StorageSettings.WeaponIndexFolder, weaponByName.ToJson());
                }, exception => _log.Error(exception, "Can't dump index"),
                () => settingsProvider.StorageSettings.WeaponIndexDumpPeriod);
            _dump.Start();
        }

        public bool ClaimWeapon(string weaponName, string userName)
        {
            return weaponByName.TryAdd(weaponName, new WeaponMeta
            {
                Owner = userName,
                ExpireAt = DateTime.UtcNow +  _settingsProvider.StorageSettings.WeaponTTL
            });
        }

        public bool IsClaimed(string weaponName) => weaponByName.ContainsKey(weaponName);

        public bool IsOwner(string userName, string weaponName) =>
            weaponByName.TryGetValue(weaponName, out var meta) && meta.Owner.Equals(userName);
    }
    
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