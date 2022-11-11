using System.Text.RegularExpressions;

namespace PointsChecker
{
    public class BarcodeScannerProvider
    {
        Configuration configuration;

        public BarcodeScannerProvider(Configuration config)
        {
            configuration = config;
        }

        public string MatchAndGetUserIdFromQRCode(string keyDownBuffer)
        {
            var regex = new Regex(configuration.UserCodePrefix + @"(\d+)", RegexOptions.IgnoreCase);

            var match = regex.Match(keyDownBuffer);

            if (match.Success && match.Groups.Count > 0)
            {
                return match.Groups[1].Value;
            }

            return null;
        }
    }
}
