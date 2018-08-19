using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class Win32API
    {
        public const int SW_SHOW = 5;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWDEFAULT = 10;
        #region Const Parameter
        public const int WM_USER = 0x400;
        public const int WM_PAINT = 0xF;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_CHAR = 0x102;

        public const int EM_GETSCROLLPOS = (WM_USER + 221);
        public const int EM_SETSCROLLPOS = (WM_USER + 222);

        public const int VK_CONTROL = 0x11;
        public const int VK_UP = 0x26;
        public const int VK_DOWN = 0x28;
        public const int VK_NUMLOCK = 0x90;
        public const int WM_VSCROLL = 0x115;

        public const short KS_ON = 0x01;
        public const short KS_KEYDOWN = 0x80;

        public const int EM_POSFROMCHAR = 0xd6;
        public const int EM_CHARFROMPOS = 0xd7;
        public const int EM_LINEFROMCHAR = 0xc9;
        public const int EM_LINEINDEX = 0xbb;

        public const int SB_VERT = 0x1;
        public const int SB_THUMBPOSITION = 0x4;
        public const int SB_BOTTOM = 0x7;

        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_LOGON_NETWORK = 3;
        public const int LOGON32_LOGON_BATCH = 4;
        public const int LOGON32_LOGON_SERVICE = 5;
        public const int LOGON32_LOGON_UNLOCK = 7;
        public const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        public const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        public const int LOGON32_PROVIDER_DEFAULT = 0;
        #endregion

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        public static void PreventSleep()
        {
            SetThreadExecutionState(ExecutionState.EsContinuous | ExecutionState.EsSystemRequired);
        }

        public static void AllowSleep()
        {
            SetThreadExecutionState(ExecutionState.EsContinuous);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

        [FlagsAttribute]
        private enum ExecutionState : uint
        {
            EsAwaymodeRequired = 0x00000040,
            EsContinuous = 0x80000000,
            EsDisplayRequired = 0x00000002,
            EsSystemRequired = 0x00000001
        }

        #region Dll Import
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref int lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, ref POINT p, int lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, ref POINT p);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, ref int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, uint wMsg, int wParam, int lParam);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, uint hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern void SetForegroundWindow(IntPtr hwnd);

        [DllImport("User32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        [DllImport("User32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int cmdShow);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32")]
        public static extern int GetDoubleClickTime();

        [DllImport("user32")]
        public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);

        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetLongPathName([MarshalAs(UnmanagedType.LPTStr)]string path, [MarshalAs(UnmanagedType.LPTStr)]StringBuilder longPath, int longPathLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName([MarshalAs(UnmanagedType.LPTStr)]string path, [MarshalAs(UnmanagedType.LPTStr)]StringBuilder shortPath, int shortPathLength);

        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);
        // First Method
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section,
                 string key, string def, StringBuilder retVal,
            int size, string filePath);
        // Second Method
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string Section, int Key,
               string Value, [MarshalAs(UnmanagedType.LPArray)] byte[] Result,
               int Size, string FileName);

        // Third Method
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(int Section, string Key,
               string Value, [MarshalAs(UnmanagedType.LPArray)] byte[] Result,
               int Size, string FileName);

        [DllImport("kernel32.dll")]
        public static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport("user32.dll")]
        public static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        [DllImport("user32.dll")]
        public static extern bool PostMessageA(IntPtr hWnd, int nBar, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool LockWindowUpdate(IntPtr hWndLock);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "LockWindowUpdate", SetLastError = true)]
        public static extern IntPtr LockWindow(IntPtr Handle);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        public static bool CheckLibrary(string fileName)
        {
            return LoadLibrary(fileName) == IntPtr.Zero;
        }

        [DllImport("advapi32.dll")]
        public static extern int LogonUserA(String lpszUserName,
            String lpszDomain,
            String lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DuplicateToken(IntPtr hToken,
            int impersonationLevel,
            ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);


        #endregion
    }
}
