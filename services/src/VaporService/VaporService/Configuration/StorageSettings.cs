using System;
using Vostok.Commons.Time;

namespace VaporService.Configuration
{
    internal class StorageSettings
    {
        public string UserStorageFolder { get; } = "./data/fighters";
        public string WeaponStorageFolder { get; } = "./data/weapon";
        public string JabberwockyStorageFolder { get; } = "./data/jabberwocky";
        public string WeaponIndexPath { get; } = "./data/userIndex";
        public TimeSpan IndexDumpPeriod { get; } = 2.Seconds();
        public TimeSpan WeaponStorageCleanupPeriod { get; } = 1.Minutes();
        public TimeSpan WeaponTTL { get; } = 10.Minutes();
    }
}