using System.Text.RegularExpressions;

namespace PointsChecker
{
    public class BarcodeScannerProvider
    {
        public string MatchAndGetUserIdFromQRCode(string keyDownBuffer)
        {
            var regex = new Regex(@"scanapp-(\d+)", RegexOptions.IgnoreCase);

            var match = regex.Match(keyDownBuffer);

            if (match.Success && match.Groups.Count > 0)
            {
                return match.Groups[1].Value;
            }

            return null;
        }
    }
}
