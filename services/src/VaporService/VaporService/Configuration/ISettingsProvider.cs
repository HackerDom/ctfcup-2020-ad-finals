using System;
using Vostok.Commons.Time;

namespace VaporService.Configuration
{
    internal interface ISettingsProvider
    {
        StorageSettings StorageSettings { get; }
    }
}