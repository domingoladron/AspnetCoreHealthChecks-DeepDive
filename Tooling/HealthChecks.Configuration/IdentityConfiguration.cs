namespace HealthChecks.Configuration
{
    /// <summary>
    /// Represents the identity and Access Control Configuration
    /// </summary>
    public class IdentityConfiguration
    {
        public bool Enabled { get; set; }
        public string ServerUrl { get; set; }
        public string Audience { get; set; }
        public bool RequireHttpsMetadata { get; set; }

        public string TransportStorageDirectory { get; set; }
    }
}
