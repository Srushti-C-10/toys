﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using ManagedCommon;
using PowerLauncher.Helper;
using Wox.Infrastructure.UserSettings;
using Wpf.Ui.Appearance;

namespace PowerLauncher
{
    public class ThemeManager : IDisposable
    {
        private readonly PowerToysRunSettings _settings;
        private readonly MainWindow _mainWindow;
        private Theme _currentTheme;
        private bool _disposed;

        public Theme CurrentTheme => _currentTheme;

        public event Common.UI.ThemeChangedHandler ThemeChanged;

        public ThemeManager(PowerToysRunSettings settings, MainWindow mainWindow)
        {
            _settings = settings;
            _mainWindow = mainWindow;
            _currentTheme = ApplicationThemeManager.GetAppTheme().ToTheme();
            SetTheme(false);

            ApplicationThemeManager.Changed += ApplicationThemeManager_Changed;
        }

        public void SetTheme(bool fromSettings)
        {
            if (_settings.Theme == Theme.Light)
            {
                _currentTheme = Theme.Light;
                _mainWindow?.Dispatcher.Invoke(() => ApplicationThemeManager.Apply(ApplicationTheme.Light, _mainWindow.WindowBackdropType));
            }
            else if (_settings.Theme == Theme.Dark)
            {
                _currentTheme = Theme.Dark;
                _mainWindow?.Dispatcher.Invoke(() => ApplicationThemeManager.Apply(ApplicationTheme.Dark, _mainWindow.WindowBackdropType));
            }
            else if (fromSettings)
            {
                _currentTheme = ApplicationThemeManager.GetAppTheme().ToTheme();
                _mainWindow?.Dispatcher.Invoke(ApplicationThemeManager.ApplySystemTheme);
            }

            ThemeChanged?.Invoke(_currentTheme, _currentTheme);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ApplicationThemeManager_Changed(ApplicationTheme currentApplicationTheme, System.Windows.Media.Color systemAccent)
        {
            var newTheme = currentApplicationTheme.ToTheme();
            if (_currentTheme == newTheme)
            {
                return;
            }

            _currentTheme = newTheme;
            SetTheme(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                ApplicationThemeManager.Changed -= ApplicationThemeManager_Changed;
            }

            _disposed = true;
        }
    }
}