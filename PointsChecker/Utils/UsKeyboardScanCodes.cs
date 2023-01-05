using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsChecker.Utils
{
    public static class UsKeyboardScanCodes
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static string GetCharByNumPadScanCode(int scanCode)
        {
            // https://www.win.tue.nl/~aeb/linux/kbd/scancodes-1.html

            switch (scanCode)
            {
                case 0x47: return "7";
                case 0x48: return "8";
                case 0x49: return "9";
                case 0x4b: return "4";
                case 0x4c: return "5";
                case 0x4d: return "6";
                case 0x4f: return "1";
                case 0x50: return "2";
                case 0x51: return "3";
                case 0x52: return "0";

                default: 
                    Logger.Debug("ScanCode is not NumPad US original keyboard key");
                    return "";
            }
        }
    }
}
