using Newtonsoft.Json;

namespace PointsChecker
{
    public class Configuration
    {
        public const string SecureKey = "d0sk39dksldn30dsndswei1vzm49df7dsb32vsm34ks870d";

        public string BaseUrl { get; set; } = @"https://zireto.ro/api/";

        [JsonIgnore]
        public string ApiToken { get; set; }

        public string CheckPointsEndPoint { get; set; } = "points/check";

        public string ConsumeEndpoint { get; set; } = "points/consume";

        public int ScannerInputTimeoutInMilliseconds { get; set; } = 1000;

        public string UserCodePrefix { get; set; } = "scanapp-1";
    }
}
