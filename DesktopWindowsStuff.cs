using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// made by Rod Stephens
// http://blog.csharphelper.com/2012/03/01/tile-a-set-of-desktop-windows-in-rows-and-columns-in-c.aspx
//
// modified by Domen Zagar, zagar.domen@gmail.com, June 2014
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

namespace Layout_manager_for_Tekla
{
    static class DesktopWindowsStuff
    {
        #region "Find Desktop Windows"
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd,
            StringBuilder lpWindowText, int nMaxCount);

        //[DllImport("user32.dll", EntryPoint = "GetRoot",
        //ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        //private static extern HWND GetAncestor(IntPtr hwnd, GA_ROOT);

        [DllImport("user32.dll", /*ntryPoint = "GetRoot",*/ CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetAncestor(IntPtr hWnd, int flags);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumDesktopWindows(IntPtr hDesktop,
            EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        // Define the SetWindowPosFlags enumeration.
        [Flags()]
        public enum SetWindowPosFlags : uint
        {
            SynchronousWindowPosition = 0x4000,
            DeferErase = 0x2000,
            DrawFrame = 0x0020,
            FrameChanged = 0x0020,
            HideWindow = 0x0080,
            DoNotActivate = 0x0010,
            DoNotCopyBits = 0x0100,
            IgnoreMove = 0x0002,
            DoNotChangeOwnerZOrder = 0x0200,
            DoNotRedraw = 0x0008,
            DoNotReposition = 0x0200,
            DoNotSendChangingEvent = 0x0400,
            IgnoreResize = 0x0001,
            IgnoreZOrder = 0x0004,
            ShowWindow = 0x0040,
        }

        // Define the callback delegate's type.
        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        // Save window titles and handles in these lists.
        private static List<IntPtr> WindowHandles;
        private static List<string> WindowTitles;

        // Return a list of the desktop windows' handles and titles.
        public static void GetDesktopWindowHandlesAndTitles(
            out List<IntPtr> handles, out List<string> titles)
        {
            WindowHandles = new List<IntPtr>();
            WindowTitles = new List<string>();

            if (!EnumDesktopWindows(IntPtr.Zero, FilterCallback, IntPtr.Zero))
            {
                handles = null;
                titles = null;
            }
            else
            {
                handles = WindowHandles;
                titles = WindowTitles;
            }
        }

        // We use this function to filter windows.
        // This version selects visible windows that have titles.
        private static bool FilterCallback(IntPtr hWnd, int lParam)
        {
            StringBuilder sb_AncestorName = new StringBuilder(1024);
            int lengthAN = GetWindowText(GetAncestor(hWnd, 3), sb_AncestorName, sb_AncestorName.Capacity);
            string titleAncestor = sb_AncestorName.ToString();

            // Get the window's title.
            StringBuilder sb_title = new StringBuilder(1024);
            int length = GetWindowText(hWnd, sb_title, sb_title.Capacity);               // gets sb_title, which gets added to title list
            string title = sb_title.ToString();                                          //  

            if (titleAncestor.Length >= 5 && titleAncestor.Substring(0, 5) == "Tekla")
            {
                // If the window is visible and has a title, save it.
                if (IsWindowVisible(hWnd) && string.IsNullOrEmpty(title) == false)
                {
                    WindowHandles.Add(hWnd);
                    WindowTitles.Add(title);
                }
            }
            // Return true to indicate that we
            // should continue enumerating windows.
            return true;
        }
        #endregion "Find Desktop Windows"

        #region "SetWindowPos"
        // Define the SetWindowPos API function.
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        // Wrapper for SetWindowPos.
        public static void SetWindowPos(IntPtr hWnd, int x, int y, int width, int height)
        {
            SetWindowPos(hWnd, IntPtr.Zero, x, y, width, height, 0);
        }
        #endregion "SetWindowPos"

        #region "SetWindowPlacement"
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPlacement(IntPtr hWnd,
           [In] ref WINDOWPLACEMENT lpwndpl);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPLACEMENT
        {
            public int Length;
            public int Flags;
            public ShowWindowCommands ShowCmd;
            public POINT MinPosition;
            public POINT MaxPosition;
            public RECT NormalPosition;
            public static WINDOWPLACEMENT Default
            {
                get
                {
                    WINDOWPLACEMENT result = new WINDOWPLACEMENT();
                    result.Length = Marshal.SizeOf(result);
                    return result;
                }
            }
        }

        internal enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            ShowMinimized = 2,
            Maximize = 3, // is this the right value?
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            private int _Left;
            private int _Top;
            private int _Right;
            private int _Bottom;
        }

        // Wrapper for SetWindowPlacement.
        public static void SetWindowPlacement(IntPtr handle, ShowWindowCommands show_command)
        {
            // Prepare the WINDOWPLACEMENT structure.
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.Length = Marshal.SizeOf(placement);

            // Get the window's current placement.
            GetWindowPlacement(handle, out placement);

            // Perform the action.
            placement.ShowCmd = show_command;
            SetWindowPlacement(handle, ref placement);
        }
        #endregion "SetWindowPlacement"

    }
}
