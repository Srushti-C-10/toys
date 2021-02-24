// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.PowerToys.Settings.UI.OOBE.Enums;
using Microsoft.PowerToys.Settings.UI.OOBE.ViewModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace Microsoft.PowerToys.Settings.UI.OOBE.Views
{
    public sealed partial class OobeShellPage : UserControl
    {
        /// <summary>
        /// Gets view model.
        /// </summary>
        public OobeShellViewModel ViewModel { get; } = new OobeShellViewModel();

        /// <summary>
        /// Gets or sets a shell handler to be used to update contents of the shell dynamically from page within the frame.
        /// </summary>
        public static OobeShellPage OobeShellHandler { get; set; }

        public ObservableCollection<OobePowerToysModule> Modules { get; }

        public OobeShellPage()
        {
            InitializeComponent();

            DataContext = ViewModel;
            OobeShellHandler = this;

            Modules = new ObservableCollection<OobePowerToysModule>();
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();

            Modules.Insert((int)PowerToysModulesEnum.Overview, new OobePowerToysModule()
            {
                ModuleName = "Overview",
                Tag = "Overview",
                IsNew = false,
                Icon = "\uEF3C",
                Image = "ms-appx:///Assets/Modules/ColorPicker.png",
                FluentIcon = "ms-appx:///Assets/FluentIcons/PowerToys.png",
                PreviewImageSource = "https://github.com/microsoft/PowerToys/raw/master/doc/images/overview/PT%20hero%20image.png",
                DescriptionLink = "https://aka.ms/PowerToysOverview",
                Link = "https://github.com/microsoft/PowerToys/releases/",
            });
            Modules.Insert((int)PowerToysModulesEnum.ColorPicker, new OobePowerToysModule()
            {
                ModuleName = "Color Picker",
                Tag = "ColorPicker",
                IsNew = false,
                Icon = "\uEF3C",
                Image = "ms-appx:///Assets/Modules/ColorPicker.png",
                FluentIcon = "ms-appx:///Assets/FluentIcons/ColorPicker.png",
                PreviewImageSource = "https://raw.githubusercontent.com/wiki/microsoft/PowerToys/images/colorpicker/ColorPicking.gif",
                Description = loader.GetString("Oobe_ColorPicker_Description"),
                Link = "https://docs.microsoft.com/en-us/windows/powertoys/color-picker",
            });
            Modules.Insert((int)PowerToysModulesEnum.FancyZones, new OobePowerToysModule()
            {
                ModuleName = "FancyZones",
                Tag = "FancyZones",
                IsNew = false,
                Icon = "\uE737",
                Image = "ms-appx:///Assets/Modules/FancyZones.png",
                FluentIcon = "ms-appx:///Assets/FluentIcons/FancyZones.png",
                PreviewImageSource = "https://user-images.githubusercontent.com/9866362/101410242-5b90a280-38df-11eb-834a-8365453b8429.gif",
                Description = loader.GetString("Oobe_FancyZones_Description"),
                Link = "https://docs.microsoft.com/en-us/windows/powertoys/fancyzones",
            });
            Modules.Insert((int)PowerToysModulesEnum.ImageResizer, new OobePowerToysModule()
            {
                ModuleName = "ImageResizer",
                Tag = "ImageResizer",
                IsNew = false,
                Icon = "\uEB9F",
                Image = "ms-appx:///Assets/Modules/ImageResizer.png",
                FluentIcon = "ms-appx:///Assets/FluentIcons/ImageResizer.png",
                Description = loader.GetString("Oobe_ImageResizer_Description"),
                PreviewImageSource = "https://docs.microsoft.com/en-us/windows/images/powertoys-resize-images.gif",
                Link = "https://docs.microsoft.com/en-us/windows/powertoys/image-resizer",
            });
            Modules.Insert((int)PowerToysModulesEnum.KBM, new OobePowerToysModule()
            {
                ModuleName = "Keyboard Manager",
                Tag = "KBM",
                IsNew = false,
                Icon = "\uE765",
                Image = "ms-appx:///Assets/Modules/KeyboardManager.png",
                FluentIcon = "ms-appx:///Assets/FluentIcons/KeyboardManager.png",
                Description = loader.GetString("Oobe_KBM_Description"),
                PreviewImageSource = "https://docs.microsoft.com/en-us/windows/images/powertoys-keyboard-remap-a-b.png",
                Link = "https://docs.microsoft.com/en-us/windows/powertoys/keyboard-manager",
            });
            Modules.Insert((int)PowerToysModulesEnum.Run, new OobePowerToysModule()
            {
                ModuleName = "PowerToys Run",
                Tag = "Run",
                IsNew = false,
                Icon = "\uE773",
                Image = "ms-appx:///Assets/Modules/PowerLauncher.png",
                FluentIcon = "ms-appx:///Assets/FluentIcons/PowerToysRun.png",
                PreviewImageSource = "https://raw.githubusercontent.com/wiki/microsoft/PowerToys/images/Launcher/QuickStart.gif",
                Description = loader.GetString("Oobe_PowerRun_Description"),
                Link = "https://docs.microsoft.com/en-us/windows/powertoys/run",
            });
            Modules.Insert((int)PowerToysModulesEnum.PowerRename, new OobePowerToysModule()
            {
                ModuleName = "PowerRename",
                Tag = "PowerRename",
                IsNew = false,
                Icon = "\uE8AC",
                Image = "ms-appx:///Assets/Modules/PowerRename.png",
                FluentIcon = "ms-appx:///Assets/FluentIcons/PowerRename.png",
                Description = loader.GetString("Oobe_PowerRename_Description"),
                PreviewImageSource = "https://docs.microsoft.com/en-us/windows/images/powerrename-demo.gif",
                Link = "https://docs.microsoft.com/en-us/windows/powertoys/powerrename",
            });
            Modules.Insert((int)PowerToysModulesEnum.FileExplorer, new OobePowerToysModule()
            {
                ModuleName = "File explorer add-ons",
                Tag = "FileExplorer",
                IsNew = false,
                Icon = "\uEC50",
                FluentIcon = "ms-appx:///Assets/FluentIcons/FileExplorerPreview.png",
                Image = "ms-appx:///Assets/Modules/PowerPreview.png",
                Description = loader.GetString("Oobe_FileExplorer_Description"),
                PreviewImageSource = "https://docs.microsoft.com/en-us/windows/images/powertoys-fileexplorer.gif",
                Link = "https://docs.microsoft.com/en-us/windows/powertoys/file-explorer",
            });
            Modules.Insert((int)PowerToysModulesEnum.ShortcutGuide, new OobePowerToysModule()
            {
                ModuleName = "Shortcut Guide",
                Tag = "ShortcutGuide",
                IsNew = false,
                Icon = "\uEDA7",
                FluentIcon = "ms-appx:///Assets/FluentIcons/ShortcutGuide.png",
                Image = "ms-appx:///Assets/Modules/ShortcutGuide.png",
                Description = loader.GetString("Oobe_ShortcutGuide_Description"),
                PreviewImageSource = "https://docs.microsoft.com/en-us/windows/images/pt-shortcut-guide-large.png",
                Link = "https://docs.microsoft.com/en-us/windows/powertoys/shortcut-guide",
            });
            Modules.Insert((int)PowerToysModulesEnum.VideoConference, new OobePowerToysModule()
            {
                ModuleName = "Video Conference",
                Tag = "VideoConference",
                IsNew = true,
                Icon = "\uEC50",
                FluentIcon = "ms-appx:///Assets/FluentIcons/VideoConferenceMute.png",
                Image = "ms-appx:///Assets/Modules/VideoConference.png",
                Description = loader.GetString("Oobe_VideoConference_Description"),
                PreviewImageSource = "https://docs.microsoft.com/en-us/windows/images/pt-video-conference-mute-settings.png",
                Link = "https://docs.microsoft.com/en-us/windows/powertoys/video-conference-mute",
            });
        }

        private void UserControl_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (Modules.Count > 0)
            {
                NavigationView.SelectedItem = Modules[0];
            }
        }

        [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Params are required for event handler signature requirements.")]
        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            OobePowerToysModule selectedItem = args.SelectedItem as OobePowerToysModule;
            switch (selectedItem.Tag)
            {
                case "Overview": NavigationFrame.Navigate(typeof(OobeOverview)); break;
                case "ColorPicker": NavigationFrame.Navigate(typeof(OobeColorPicker)); break;
                case "FancyZones": NavigationFrame.Navigate(typeof(OobeFancyZones)); break;
                case "Run": NavigationFrame.Navigate(typeof(OobeRun)); break;
                case "ImageResizer": NavigationFrame.Navigate(typeof(OobeImageResizer)); break;
                case "KBM": NavigationFrame.Navigate(typeof(OobeKBM)); break;
                case "PowerRename": NavigationFrame.Navigate(typeof(OobePowerRename)); break;
                case "FileExplorer": NavigationFrame.Navigate(typeof(OobeFileExplorer)); break;
                case "ShortcutGuide": NavigationFrame.Navigate(typeof(OobeShortcutGuide)); break;
                case "VideoConference": NavigationFrame.Navigate(typeof(OobeVideoConference)); break;
            }
        }
    }
}
