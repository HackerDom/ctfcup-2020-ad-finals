using System;
using Vostok.Commons.Time;

namespace VaporService.Configuration
{
    internal class StorageSettings
    {
        public string UserStorageFolder { get; } = "./data/users/";
        public string WeaponStorageFolder { get; } = "./data/weapon/";
        public string WeaponIndexFolder { get; } = "./data/userIndex";
        public TimeSpan WeaponIndexDumpPeriod { get; } = 2.Seconds();
        
        public TimeSpan WeaponTTL { get; } = 5.Seconds();
    }
}