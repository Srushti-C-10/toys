﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Awake.Core.Models;
using Microsoft.PowerToys.Settings.UI.Library;
using NLog;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.

namespace Awake.Core
{
    internal static class TrayHelper
    {
        private static readonly Logger _log;

        private static IntPtr _trayMenu;

        private static IntPtr TrayMenu { get => _trayMenu; set => _trayMenu = value; }

        private static NotifyIcon? _trayIcon;

        private static NotifyIcon TrayIcon { get => _trayIcon; set => _trayIcon = value; }

        private static SettingsUtils? _moduleSettings;

        private static SettingsUtils ModuleSettings { get => _moduleSettings; set => _moduleSettings = value; }

        static TrayHelper()
        {
            _log = LogManager.GetCurrentClassLogger();
            TrayIcon = new NotifyIcon();
            ModuleSettings = new SettingsUtils();
        }

        public static void InitializeTray(string text, Icon icon, ContextMenuStrip? contextMenu = null)
        {
            Task.Factory.StartNew(
                (tray) =>
                {
                    try
                    {
                        _log.Info("Setting up the tray.");
                        ((NotifyIcon?)tray).Text = text;
                        ((NotifyIcon?)tray).Icon = icon;
                        ((NotifyIcon?)tray).ContextMenuStrip = contextMenu;
                        ((NotifyIcon?)tray).Visible = true;
                        ((NotifyIcon?)tray).MouseClick += TrayClickHandler;
                        Application.AddMessageFilter(new TrayMessageFilter());
                        Application.Run();
                        _log.Info("Tray setup complete.");
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"An error occurred initializing the tray. {ex.Message}");
                        _log.Error($"{ex.StackTrace}");
                    }
                }, TrayIcon);
        }

        private static void TrayClickHandler(object? sender, MouseEventArgs e)
        {
            IEnumerable<IntPtr> windowHandles = APIHelper.EnumerateWindowsForProcess(Process.GetCurrentProcess().Id);
            var domain = AppDomain.CurrentDomain.GetHashCode().ToString("x");
            string targetClass = $"{InternalConstants.TrayWindowId}{domain}";

            foreach (var handle in windowHandles)
            {
                StringBuilder className = new (256);
                NativeMethods.GetClassName(handle, className, className.Capacity);
                if (className.ToString().StartsWith(targetClass))
                {
                    NativeMethods.TrackPopupMenuEx(TrayMenu, 0, Cursor.Position.X, Cursor.Position.Y, handle, IntPtr.Zero);
                    break;
                }
            }
        }

        public static void ClearTray()
        {
            TrayIcon.Icon = null;
            TrayIcon.Dispose();
        }

        internal static void SetTray(string text, AwakeSettings settings)
        {
            SetTray(
                text,
                settings.Properties.KeepDisplayOn,
                settings.Properties.Mode);
        }

        private static Action ExitCallback()
        {
            return () =>
            {
                Environment.Exit(Environment.ExitCode);
            };
        }

        private static Action KeepDisplayOnCallback(string moduleName)
        {
            return () =>
            {
                AwakeSettings currentSettings;

                try
                {
                    currentSettings = ModuleSettings.GetSettings<AwakeSettings>(moduleName);
                }
                catch (FileNotFoundException)
                {
                    currentSettings = new AwakeSettings();
                }

                currentSettings.Properties.KeepDisplayOn = !currentSettings.Properties.KeepDisplayOn;

                ModuleSettings.SaveSettings(JsonSerializer.Serialize(currentSettings), moduleName);
            };
        }

        private static Action<uint, uint> TimedKeepAwakeCallback(string moduleName)
        {
            return (hours, minutes) =>
            {
                AwakeSettings currentSettings;

                try
                {
                    currentSettings = ModuleSettings.GetSettings<AwakeSettings>(moduleName);
                }
                catch (FileNotFoundException)
                {
                    currentSettings = new AwakeSettings();
                }

                currentSettings.Properties.Mode = AwakeMode.TIMED;
                currentSettings.Properties.Hours = hours;
                currentSettings.Properties.Minutes = minutes;

                ModuleSettings.SaveSettings(JsonSerializer.Serialize(currentSettings), moduleName);
            };
        }

        private static Action PassiveKeepAwakeCallback(string moduleName)
        {
            return () =>
            {
                AwakeSettings currentSettings;

                try
                {
                    currentSettings = ModuleSettings.GetSettings<AwakeSettings>(moduleName);
                }
                catch (FileNotFoundException)
                {
                    currentSettings = new AwakeSettings();
                }

                currentSettings.Properties.Mode = AwakeMode.PASSIVE;

                ModuleSettings.SaveSettings(JsonSerializer.Serialize(currentSettings), moduleName);
            };
        }

        private static Action IndefiniteKeepAwakeCallback(string moduleName)
        {
            return () =>
            {
                AwakeSettings currentSettings;

                try
                {
                    currentSettings = ModuleSettings.GetSettings<AwakeSettings>(moduleName);
                }
                catch (FileNotFoundException)
                {
                    currentSettings = new AwakeSettings();
                }

                currentSettings.Properties.Mode = AwakeMode.INDEFINITE;

                ModuleSettings.SaveSettings(JsonSerializer.Serialize(currentSettings), moduleName);
            };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1005:Single line comments should begin with single space", Justification = "For debugging purposes - will remove later.")]
        public static void SetTray(string text, bool keepDisplayOn, AwakeMode mode)
        {
            TrayMenu = NativeMethods.CreatePopupMenu();
            NativeMethods.InsertMenu(TrayMenu, 0, NativeConstants.MF_BYPOSITION | NativeConstants.MF_STRING, NativeConstants.WM_USER + 1, "Exit");
            NativeMethods.InsertMenu(TrayMenu, 0, NativeConstants.MF_BYPOSITION | NativeConstants.MF_SEPARATOR, 0, string.Empty);
            NativeMethods.InsertMenu(TrayMenu, 0, NativeConstants.MF_BYPOSITION | NativeConstants.MF_STRING | (keepDisplayOn ? NativeConstants.MF_CHECKED : NativeConstants.MF_UNCHECKED), NativeConstants.WM_USER + 2, "Keep screen on");

            var awakeTimeMenu = NativeMethods.CreatePopupMenu();
            NativeMethods.InsertMenu(awakeTimeMenu, 0, NativeConstants.MF_BYPOSITION | NativeConstants.MF_STRING, NativeConstants.WM_USER + 3, "30 minutes");
            NativeMethods.InsertMenu(awakeTimeMenu, 1, NativeConstants.MF_BYPOSITION | NativeConstants.MF_STRING, NativeConstants.WM_USER + 4, "1 hour");
            NativeMethods.InsertMenu(awakeTimeMenu, 2, NativeConstants.MF_BYPOSITION | NativeConstants.MF_STRING, NativeConstants.WM_USER + 5, "2 hours");

            var modeMenu = NativeMethods.CreatePopupMenu();
            NativeMethods.InsertMenu(modeMenu, 0, NativeConstants.MF_BYPOSITION | NativeConstants.MF_STRING | (mode == AwakeMode.PASSIVE ? NativeConstants.MF_CHECKED : NativeConstants.MF_UNCHECKED), NativeConstants.WM_USER + 6, "Off (keep using the selected power plan)");
            NativeMethods.InsertMenu(modeMenu, 1, NativeConstants.MF_BYPOSITION | NativeConstants.MF_STRING | (mode == AwakeMode.INDEFINITE ? NativeConstants.MF_CHECKED : NativeConstants.MF_UNCHECKED), NativeConstants.WM_USER + 7, "Keep awake indefinitely");

            NativeMethods.InsertMenu(modeMenu, 2, NativeConstants.MF_BYPOSITION | NativeConstants.MF_POPUP | (mode == AwakeMode.TIMED ? NativeConstants.MF_CHECKED : NativeConstants.MF_UNCHECKED), (uint)awakeTimeMenu, "Keep awake temporarily");
            NativeMethods.InsertMenu(TrayMenu, 0, NativeConstants.MF_BYPOSITION | NativeConstants.MF_POPUP, (uint)modeMenu, "Mode");

            //ContextMenuStrip? contextMenuStrip = new ContextMenuStrip();

            //// Main toolstrip.
            //ToolStripMenuItem? operationContextMenu = new ToolStripMenuItem
            //{
            //    Text = "Mode",
            //};

            //// No keep-awake menu item.
            //CheckButtonToolStripMenuItem? passiveMenuItem = new CheckButtonToolStripMenuItem
            //{
            //    Text = "Off (Keep using the selected power plan)",
            //};

            //passiveMenuItem.Checked = mode == AwakeMode.PASSIVE;

            //passiveMenuItem.Click += (e, s) =>
            //{
            //    // User opted to set the mode to indefinite, so we need to write new settings.
            //    passiveKeepAwakeCallback();
            //};

            //// Indefinite keep-awake menu item.
            //CheckButtonToolStripMenuItem? indefiniteMenuItem = new CheckButtonToolStripMenuItem
            //{
            //    Text = "Keep awake indefinitely",
            //};

            //indefiniteMenuItem.Checked = mode == AwakeMode.INDEFINITE;

            //indefiniteMenuItem.Click += (e, s) =>
            //{
            //    // User opted to set the mode to indefinite, so we need to write new settings.
            //    indefiniteKeepAwakeCallback();
            //};

            //CheckButtonToolStripMenuItem? displayOnMenuItem = new CheckButtonToolStripMenuItem
            //{
            //    Text = "Keep screen on",
            //};

            //displayOnMenuItem.Checked = keepDisplayOn;

            //displayOnMenuItem.Click += (e, s) =>
            //{
            //    // User opted to set the display mode directly.
            //    keepDisplayOnCallback();
            //};

            //// Timed keep-awake menu item
            //ToolStripMenuItem? timedMenuItem = new ToolStripMenuItem
            //{
            //    Text = "Keep awake temporarily",
            //};

            //timedMenuItem.Checked = mode == AwakeMode.TIMED;
            //timedMenuItem.AccessibleName = timedMenuItem.Text + (timedMenuItem.Checked ? ". Checked. " : ". UnChecked. ");

            //ToolStripMenuItem? halfHourMenuItem = new ToolStripMenuItem
            //{
            //    Text = "30 minutes",
            //};

            //halfHourMenuItem.Click += (e, s) =>
            //{
            //    // User is setting the keep-awake to 30 minutes.
            //    timedKeepAwakeCallback(0, 30);
            //};

            //ToolStripMenuItem? oneHourMenuItem = new ToolStripMenuItem
            //{
            //    Text = "1 hour",
            //};

            //oneHourMenuItem.Click += (e, s) =>
            //{
            //    // User is setting the keep-awake to 1 hour.
            //    timedKeepAwakeCallback(1, 0);
            //};

            //ToolStripMenuItem? twoHoursMenuItem = new ToolStripMenuItem
            //{
            //    Text = "2 hours",
            //};

            //twoHoursMenuItem.Click += (e, s) =>
            //{
            //    // User is setting the keep-awake to 2 hours.
            //    timedKeepAwakeCallback(2, 0);
            //};

            //// Exit menu item.
            //ToolStripMenuItem? exitContextMenu = new ToolStripMenuItem
            //{
            //    Text = "Exit",
            //};

            //exitContextMenu.Click += (e, s) =>
            //{
            //    // User is setting the keep-awake to 2 hours.
            //    exitCallback();
            //};
            TrayIcon.Text = text;
        }

        private class CheckButtonToolStripMenuItemAccessibleObject : ToolStripItem.ToolStripItemAccessibleObject
        {
            private CheckButtonToolStripMenuItem _menuItem;

            public CheckButtonToolStripMenuItemAccessibleObject(CheckButtonToolStripMenuItem menuItem)
                : base(menuItem)
            {
                _menuItem = menuItem;
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.CheckButton;
                }
            }

            public override string Name => _menuItem.Text + ", " + Role + ", " + (_menuItem.Checked ? "Checked" : "Unchecked");
        }

        private class CheckButtonToolStripMenuItem : ToolStripMenuItem
        {
            public CheckButtonToolStripMenuItem()
            {
            }

            protected override AccessibleObject CreateAccessibilityInstance()
            {
                return new CheckButtonToolStripMenuItemAccessibleObject(this);
            }
        }
    }
}
