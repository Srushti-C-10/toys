﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using interop;
using Windows.UI.Popups;

namespace Microsoft.PowerToys.Settings.UI.Runner
{
    public class Program
    {
        // Quantity of arguments
        private const int ArgumentsQty = 5;

        // Create an instance of the  IPC wrapper.
        private static TwoWayPipeMessageIPCManaged ipcmanager;

        public static bool IsElevated { get; set; }

        public static bool IsUserAnAdmin { get; set; }

        public static int PowerToysPID { get; set; }

        [STAThread]
        public static void Main(string[] args)
        {
            using (new UI.App())
            {
                App app = new App();
                app.InitializeComponent();

                if (args.Length >= ArgumentsQty)
                {
                    int.TryParse(args[2], out int powerToysPID);
                    PowerToysPID = powerToysPID;

                    if (args[4] == "true")
                    {
                        IsElevated = true;
                    }
                    else
                    {
                        IsElevated = false;
                    }

                    if (args[5] == "true")
                    {
                        IsUserAnAdmin = true;
                    }
                    else
                    {
                        IsUserAnAdmin = false;
                    }

                    WaitForPowerToysRunner();

                    ipcmanager = new TwoWayPipeMessageIPCManaged(args[1], args[0], null);
                    ipcmanager.Start();
                    app.Run();
                }
                else
                {
                    MessageBox.Show(
                        "The application cannot be run as a standalone process. Please start the application through the runner.",
                        "Forbidden",
                        MessageBoxButton.OK);
                    app.Shutdown();
                }
            }
        }

        public static TwoWayPipeMessageIPCManaged GetTwoWayIPCManager()
        {
            return ipcmanager;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        internal static void WaitForPowerToysRunner()
        {
            Task.Run(() =>
            {
                const uint INFINITE = 0xFFFFFFFF;
                const uint WAIT_OBJECT_0 = 0x00000000;
                const uint SYNCHRONIZE = 0x00100000;

                IntPtr powerToysProcHandle = OpenProcess(SYNCHRONIZE, false, PowerToysPID);
                if (WaitForSingleObject(powerToysProcHandle, INFINITE) == WAIT_OBJECT_0)
                {
                    Environment.Exit(0);
                }
            });
        }
    }
}
