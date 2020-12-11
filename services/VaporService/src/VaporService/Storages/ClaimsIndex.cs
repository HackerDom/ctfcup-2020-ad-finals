using System;
using System.Collections.Concurrent;
using System.IO;
using VaporService.Configuration;
using VaporService.Helpers;
using VaporService.Models;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;

namespace VaporService.Storages
{
    internal class ClaimsIndex : IClaimesIndex
    {
        private AsyncPeriodicalAction _dump;
        private readonly ILog _log;
        private readonly ISettingsProvider _settingsProvider;
        private readonly ConcurrentDictionary<string, ItemMeta> _itemByName;
        private PeriodicalAction _cleanup;

        public ClaimsIndex(ISettingsProvider settingsProvider, ILog log)
        {
            _settingsProvider = settingsProvider;
            _log = log;
            if (File.Exists(_settingsProvider.StorageSettings.WeaponIndexPath))
                _itemByName = File.ReadAllText(_settingsProvider.StorageSettings.WeaponIndexPath)
                    .FromJson<ConcurrentDictionary<string, ItemMeta>>();
            else
                _itemByName = new ConcurrentDictionary<string, ItemMeta>();
            
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
                        _itemByName.ToJson());
                }, exception => _log.Error(exception, "Can't dump index"),
                () => settingsProvider.StorageSettings.IndexDumpPeriod);
            _cleanup = new PeriodicalAction(() =>
                {
                    foreach (var pair in _itemByName.ToArray())
                        if (pair.Value.ExpireAt <= DateTime.UtcNow)
                            _itemByName.TryRemove(pair.Key, out _);
                }, exception => _log.Error(exception, "can't cleanup"),
                () => _settingsProvider.StorageSettings.WeaponStorageCleanupPeriod
            );
            _dump.Start();
            _cleanup.Start();
        }

        public bool ClaimWeapon(string weaponName, string userName)
        {
            return _itemByName.TryAdd(weaponName, new ItemMeta
            {
                Owner = userName,
                ExpireAt = DateTime.UtcNow + _settingsProvider.StorageSettings.WeaponTTL
            });
        }

        public bool IsClaimed(string weaponName)
        {
            return _itemByName.ContainsKey(weaponName);
        }

        public bool IsOwner(string userName, string weaponName)
        {
            return _itemByName.TryGetValue(weaponName, out var meta) && meta.Owner.Equals(userName);
        }
    }
}