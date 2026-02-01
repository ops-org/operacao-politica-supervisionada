namespace OPS.API.Configuration
{
    public class CacheSettings
    {
        public bool Enabled { get; set; } = true;
        public int DefaultExpirationHours { get; set; } = 12;
        public int LocalCacheExpirationHours { get; set; } = 6;
    }
}
