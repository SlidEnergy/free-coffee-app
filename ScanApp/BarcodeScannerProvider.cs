using System.Text.RegularExpressions;

namespace ScanApp
{
    public class BarcodeScannerProvider
    {
        public string MatchAndGetUserIdFromQRCode(string keyDownBuffer)
        {
            var regex = new Regex(@"scanapp-([a-z0-9]{7,7})", RegexOptions.IgnoreCase);

            var match = regex.Match(keyDownBuffer);

            if (match.Success && match.Groups.Count > 0)
            {
                return match.Groups[1].Value;
            }

            return null;
        }
    }
}
