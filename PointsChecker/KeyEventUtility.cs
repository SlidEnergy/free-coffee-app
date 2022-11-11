using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PointsChecker
{
    public static class KeyEventUtility
    {
        // ReSharper disable InconsistentNaming
        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }
        // ReSharper restore InconsistentNaming

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs( UnmanagedType.LPWStr)]
        StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        public static extern int ToUnicodeEx(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs( UnmanagedType.LPWStr)]
                StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags,
            IntPtr dwhkl);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        [DllImport("user32.dll")]
        public static extern int ToAscii(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs( UnmanagedType.LPWStr)]
                        StringBuilder pwszBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        public static extern int ToAsciiEx(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs( UnmanagedType.LPWStr)]
                                StringBuilder pwszBuff,
            uint wFlags,
            IntPtr dwhkl);

        public static char GetCharFromKey(KeyboardHookData lParam)
        {
            char ch = ' ';

            var keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            var stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint)lParam.vkCode, (uint)lParam.scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
                default:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
            }
            return ch;
        }

        public static string GetCharFromKeyEx(KeyboardHookData lParam)
        {
            string ch = "";

            var keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            var stringBuilder = new StringBuilder(2);

            int result = ToUnicodeEx((uint)lParam.vkCode, (uint)lParam.scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0, GetKeyboardLayout(0));
            switch (result)
            {
                case -1:
                    break; 
                case 0:
                    break;
                case 1:
                    {
                        ch = stringBuilder.ToString();
                        break;
                    }
                default:
                    {
                        ch = stringBuilder.ToString();
                        break;
                    }
            }
            return ch;
        }

        public static string GetCharFromKeyAscii(KeyboardHookData lParam)
        {
            string ch = "";

            var keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            var stringBuilder = new StringBuilder(10);

            int result = ToAscii((uint)lParam.vkCode, (uint)lParam.scanCode, keyboardState, stringBuilder, 0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                    {
                        ch = stringBuilder.ToString();
                        break;
                    }
                default:
                    {
                        ch = stringBuilder.ToString();
                        break;
                    }
            }
            return ch;
        }

        public static string GetCharFromKeyAsciiEx(KeyboardHookData lParam)
        {
            string ch = "";

            var keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            var stringBuilder = new StringBuilder(10);

            int result = ToAsciiEx((uint)lParam.vkCode, (uint)lParam.scanCode, keyboardState, stringBuilder, 0, GetKeyboardLayout(0));
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                    {
                        ch = stringBuilder.ToString();
                        break;
                    }
                default:
                    {
                        ch = stringBuilder.ToString();
                        break;
                    }
            }
            return ch;
        }
    }
}
