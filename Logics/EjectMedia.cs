using System.Text;
using System.Runtime.InteropServices;
using System;

namespace AutoCdCopy.Logics
{
    class EjectMedia
    {
        //Win32 API Call
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi)]
        static extern int mciSendString(string lpstrCommand, StringBuilder lpstrReturnString, int uReturnLength, IntPtr hwndCallback);

        public static void Eject(string driveName)
        {
            mciSendString(string.Format("set CDAudio!{0} door closed", driveName), null, 127, IntPtr.Zero);
        }
    }
}