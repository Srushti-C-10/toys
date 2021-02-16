﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.PowerToys.Settings.UI.Library.ViewModels
{
    public class PowerLauncherPluginViewModel : INotifyPropertyChanged
    {
        private readonly PowerLauncherPluginSettings settings;
        private readonly Func<bool> isDark;

        public PowerLauncherPluginViewModel(PowerLauncherPluginSettings settings, Func<bool> isDark)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "PowerLauncherPluginSettings object is null");
            }

            this.settings = settings;
            this.isDark = isDark;
        }

        public string Id { get => settings.Id; }

        public string Name { get => settings.Name; }

        public string Description { get => settings.Description; }

        public string Author { get => settings.Author; }

        public bool Disabled
        {
            get
            {
                return settings.Disabled;
            }

            set
            {
                if (settings.Disabled != value)
                {
                    settings.Disabled = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(ShowWarning));
                }
            }
        }

        public bool IsGlobal
        {
            get
            {
                return settings.IsGlobal;
            }

            set
            {
                if (settings.IsGlobal != value)
                {
                    settings.IsGlobal = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(ShowWarning));
                }
            }
        }

        public string ActionKeyword
        {
            get
            {
                return settings.ActionKeyword;
            }

            set
            {
                if (settings.ActionKeyword != value)
                {
                    settings.ActionKeyword = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(ShowWarning));
                }
            }
        }

        public override string ToString()
        {
            return $"{Name}. {Description}";
        }

        public string IconPath { get => isDark() ? settings.IconPathDark : settings.IconPathLight; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool ShowWarning
        {
            get => !Disabled && !IsGlobal && string.IsNullOrWhiteSpace(ActionKeyword);
        }
    }
}
