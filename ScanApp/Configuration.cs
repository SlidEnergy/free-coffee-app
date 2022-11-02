using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp
{
    public class Configuration
    {
        public string BaseUrl { get; set; } = @"https://zireto.ro/api/";

        public string ApiToken { get; set; } = "1234";

        public string CheckPointsEndPoint { get; set; } = "points/check";

        public string ConsumeEndpoint { get; set; } = "points/consume";
    }
}
