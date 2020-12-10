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
        private AsyncPeriodicalAction _dump;
        private readonly ILog _log;
        private readonly ISettingsProvider _settingsProvider;
        private readonly ConcurrentDictionary<string, WeaponMeta> _weaponByName;
        private PeriodicalAction _cleanup;

        public ClaimedWeaponIndex(ISettingsProvider settingsProvider, ILog log)
        {
            _settingsProvider = settingsProvider;
            _log = log;
            if (File.Exists(_settingsProvider.StorageSettings.WeaponIndexPath))
                _weaponByName = File.ReadAllText(_settingsProvider.StorageSettings.WeaponIndexPath)
                    .FromJson<ConcurrentDictionary<string, WeaponMeta>>();
            else
                _weaponByName = new ConcurrentDictionary<string, WeaponMeta>();
            
            StartDaemons(settingsProvider);
        }

        private void StartDaemons(ISettingsProvider settingsProvider)
        {
            _dump = new AsyncPeriodicalAction(
                async () =>
                {
                    if (!Directory.Exists(Path.GetDirectoryName(settingsProvider.StorageSettings.WeaponIndexPath)))
                        Directory.CreateDirectory(settingsProvider.StorageSettings.WeaponIndexPath);
                    
                    await File.WriteAllTextAsync(settingsProvider.StorageSettings.WeaponIndexPath,
                        _weaponByName.ToJson());
                }, exception => _log.Error(exception, "Can't dump index"),
                () => settingsProvider.StorageSettings.WeaponIndexDumpPeriod);
            _cleanup = new PeriodicalAction(() =>
                {
                    if (!Directory.Exists(settingsProvider.StorageSettings.WeaponIndexPath))
                        return;
                    
                    foreach (var pair in _weaponByName.ToArray())
                        if (pair.Value.ExpireAt >= DateTime.UtcNow)
                            _weaponByName.TryRemove(pair.Key, out _);
                }, exception => _log.Error(exception, "can't cleanup"),
                () => _settingsProvider.StorageSettings.WeaponStorageCleanupPeriod
            );
            _dump.Start();
            _cleanup.Start();
        }

        public bool ClaimWeapon(string weaponName, string userName)
        {
            return _weaponByName.TryAdd(weaponName, new WeaponMeta
            {
                Owner = userName,
                ExpireAt = DateTime.UtcNow + _settingsProvider.StorageSettings.WeaponTTL
            });
        }

        public bool IsClaimed(string weaponName)
        {
            return _weaponByName.ContainsKey(weaponName);
        }

        public bool IsOwner(string userName, string weaponName)
        {
            return _weaponByName.TryGetValue(weaponName, out var meta) && meta.Owner.Equals(userName);
        }
    }
}