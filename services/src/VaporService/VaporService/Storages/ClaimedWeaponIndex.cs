using System;
using System.Collections.Concurrent;
using System.IO;
using VaporService.Configuration;
using VaporService.Helpers;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;

namespace VaporService.Storages
{
    internal class ClaimedWeaponIndex : IClaimedWeaponIndex
    {
        private readonly AsyncPeriodicalAction _dump;
        private readonly ILog _log;
        private readonly ISettingsProvider _settingsProvider;
        private readonly ConcurrentDictionary<string, WeaponMeta> weaponByName;

        public ClaimedWeaponIndex(ISettingsProvider settingsProvider, ILog log)
        {
            _settingsProvider = settingsProvider;
            _log = log;
            weaponByName = new ConcurrentDictionary<string, WeaponMeta>();
            _dump = new AsyncPeriodicalAction(
                async () =>
                {
                    await File.WriteAllTextAsync(settingsProvider.StorageSettings.WeaponIndexFolder,
                        weaponByName.ToJson());
                }, exception => _log.Error(exception, "Can't dump index"),
                () => settingsProvider.StorageSettings.WeaponIndexDumpPeriod);
            _dump.Start();
        }

        public bool ClaimWeapon(string weaponName, string userName)
        {
            return weaponByName.TryAdd(weaponName, new WeaponMeta
            {
                Owner = userName,
                ExpireAt = DateTime.UtcNow + _settingsProvider.StorageSettings.WeaponTTL
            });
        }

        public bool IsClaimed(string weaponName)
        {
            return weaponByName.ContainsKey(weaponName);
        }

        public bool IsOwner(string userName, string weaponName)
        {
            return weaponByName.TryGetValue(weaponName, out var meta) && meta.Owner.Equals(userName);
        }
    }
}