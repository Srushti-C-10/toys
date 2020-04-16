﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.PowerToys.Settings.UI.Lib;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Microsoft.PowerToys.Settings.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PowerPreviewPage : Page
    {
        private const string PreviewPaneKey = "File Explorer Preview";

        public PowerPreviewPage()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PowerPreviewSettings settings;
            try
            {
                base.OnNavigatedTo(e);
                settings = SettingsUtils.GetSettings<PowerPreviewSettings>(PreviewPaneKey);
                ToggleSwitch_Preview_SVG.IsOn = settings.properties.IDS_PREVPANE_SVG_BOOL_TOGGLE_CONTROLL.Value;
                ToggleSwitch_Preview_MD.IsOn = settings.properties.PREVPANE_MD_BOOL_TOGGLE_CONTROLL_ID.Value;
            }
            catch
            {
                settings = new PowerPreviewSettings(PreviewPaneKey);
                SettingsUtils.SaveSettings(settings.ToJsonString(), PreviewPaneKey);
                ToggleSwitch_Preview_SVG.IsOn = settings.properties.IDS_PREVPANE_SVG_BOOL_TOGGLE_CONTROLL.Value;
                ToggleSwitch_Preview_MD.IsOn = settings.properties.PREVPANE_MD_BOOL_TOGGLE_CONTROLL_ID.Value;
            }
        }

        private void ToggleSwitch_Preview_SVG_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch swt = sender as ToggleSwitch;

            if (swt != null)
            {
                PowerPreviewSettings settings = SettingsUtils.GetSettings<PowerPreviewSettings>(PreviewPaneKey);
                settings.properties.IDS_PREVPANE_SVG_BOOL_TOGGLE_CONTROLL.Value = swt.IsOn;

                if (ShellPage.DefaultSndMSGCallback != null)
                {
                    SndPowerPreviewSettings snd = new SndPowerPreviewSettings(settings);
                    SndModuleSettings<SndPowerPreviewSettings> ipcMessage = new SndModuleSettings<SndPowerPreviewSettings>(snd);
                    ShellPage.DefaultSndMSGCallback(ipcMessage.ToJsonString());
                }
            }
        }

        private void ToggleSwitch_Preview_MD_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch swt = sender as ToggleSwitch;

            if (swt != null)
            {
                PowerPreviewSettings settings = SettingsUtils.GetSettings<PowerPreviewSettings>(PreviewPaneKey);
                settings.properties.PREVPANE_MD_BOOL_TOGGLE_CONTROLL_ID.Value = swt.IsOn;

                if (ShellPage.DefaultSndMSGCallback != null)
                {
                    SndPowerPreviewSettings snd = new SndPowerPreviewSettings(settings);
                    SndModuleSettings<SndPowerPreviewSettings> ipcMessage = new SndModuleSettings<SndPowerPreviewSettings>(snd);
                    ShellPage.DefaultSndMSGCallback(ipcMessage.ToJsonString());
                }
            }
        }
    }
}
