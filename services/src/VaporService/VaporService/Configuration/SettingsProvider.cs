namespace VaporService.Configuration
{
    internal class SettingsProvider : ISettingsProvider
    {
        public StorageSettings StorageSettings { get; } = new();
    }
}