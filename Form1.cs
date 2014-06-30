using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Layout_manager_for_Tekla
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ShowDesktopWindows();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ShowDesktopWindows();
        }

        // Display a window's title, saving its handle.
        private struct WindowInfo
        {
            public string Title;
            public IntPtr Handle;

            public WindowInfo(string title, IntPtr handle)
            {
                Title = title;
                Handle = handle;
            }

            // Display the title.
            public override string ToString()
            {
                return Title;
            }
        }

        // Display a list of the desktop windows' titles.
        private void ShowDesktopWindows()
        {
            List<IntPtr> handles;
            List<string> titles;
            DesktopWindowsStuff.GetDesktopWindowHandlesAndTitles(out handles, out titles);

            // Display the window titles.
            lstWindows.Items.Clear();
            for (int i = 0; i < titles.Count; i++)
            {
                lstWindows.Items.Add(new WindowInfo(titles[i], handles[i]));
            }
        }

        // Arrange the selected controls.
        private void btnArrange_Click(object sender, EventArgs e)
        {
            if (lstWindows.SelectedItems.Count == 0) return;

            // Get the form's location and dimensions.
            int screen_top = Screen.PrimaryScreen.WorkingArea.Top;
            int screen_left = Screen.PrimaryScreen.WorkingArea.Left;
            int screen_width = Screen.PrimaryScreen.WorkingArea.Width;
            int screen_height = Screen.PrimaryScreen.WorkingArea.Height;

            // See how big the windows should be.
            int window_width = (int)(screen_width / nudCols.Value);
            int window_height = (int)(screen_height / nudRows.Value);

            // Position the windows.
            int window_num = 0;
            int y = screen_top;
            for (int row = 0; row < nudRows.Value; row++)
            {
                int x = screen_left;
                for (int col = 0; col < nudCols.Value; col++)
                {
                    // Restore the window.
                    WindowInfo window_info =
                        (WindowInfo)lstWindows.SelectedItems[window_num];
                    DesktopWindowsStuff.SetWindowPlacement(
                        window_info.Handle,
                        DesktopWindowsStuff.ShowWindowCommands.Restore);

                    // Position window window_num;
                    DesktopWindowsStuff.SetWindowPos(window_info.Handle,
                        x, y, window_width, window_height);

                    // If that was the last window, return.
                    if (++window_num >= lstWindows.SelectedItems.Count) return;
                    x += window_width;
                }
                y += window_height;
            }
        }
    }
}
