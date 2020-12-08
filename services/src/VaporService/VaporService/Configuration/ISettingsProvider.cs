namespace VaporService.Configuration
{
    internal interface ISettingsProvider
    {
        StorageSettings StorageSettings { get; }
    }
}