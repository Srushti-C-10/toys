# Overview

![PowerToys header image](./doc/images/overview/PT%20hero%20image.png)

Microsoft PowerToys is a set of utilities for power users to tune and streamline their Windows experience for greater productivity. Inspired by the [Windows 95 era PowerToys project](https://en.wikipedia.org/wiki/Microsoft_PowerToys), this reboot provides power users with ways to squeeze more efficiency out of the Windows 10 shell and customize it for individual workflows.  A great overview of the Windows 95 PowerToys can be found [here](https://socket3.wordpress.com/2016/10/22/using-windows-95-powertoys/).

[What's Happening](#whats-happening)   |   [Downloading & Release notes][github-release-link]   |   [Contribution to PowerToys](#contributing)

## Build status

[![Build Status](https://dev.azure.com/ms/PowerToys/_apis/build/status/microsoft.PowerToys?branchName=master)](https://dev.azure.com/ms/PowerToys/_build?definitionId=219)

## Current PowerToy Utilities

### FancyZones

<img align="left" src="./doc/images/overview/FancyZones_small.png" />[FancyZones](/src/modules/fancyzones/) is a window manager that makes it easy to create complex window layouts and quickly position windows into those layouts.
<br>
<br>
<br>
<br>
<br>

### Shortcut Guide

<img align="left" src="./doc/images/overview/Shortcut guide_small.png" />[Windows key shortcut guide](/src/modules/shortcut_guide) appears when a user holds the Windows key down for more than one second and shows the available shortcuts for the current state of the desktop.
<br>
<br>
<br>
<br>
<br>

### PowerRename

<img align="left" src="./doc/images/overview/PowerRename_small.PNG" />[PowerRename](/src/modules/powerrename) is a Windows Shell Extension for advanced bulk renaming using search and replace or regular expressions. PowerRename allows simple search and replace or more advanced regular expression matching. While you type in the search and replace input fields, the preview area will show what the items will be renamed to. PowerRename then calls into the Windows Explorer file operations engine to perform the rename. This has the benefit of allowing the rename operation to be undone after PowerRename exits. This code is based on [Chris Davis's SmartRename](https://github.com/chrdavis/SmartRename).
<br>
<br>

### File Explorer (Preview Panes)

<img align="left" src="./doc/images/overview/PowerPreview_small.PNG" />[File Explorer](/src/modules/previewpane) add-ons right now are just limited to Preview Pane additions for File Explorer. Preview Pane is an existing feature in the File Explorer.  To enable it, you just click the View tab in the ribbon and then click "Preview Pane".

PowerToys will now enable two types of files to be previewed: Markdown (.md) & SVG (.svg)
<br>
<br>
<br>

### Image Resizer

<img align="left" src="./doc/images/overview/ImageResizer_small.png" />[Image Resizer](/src/modules/imageresizer) is a Windows Shell Extension for quickly resizing images.  With a simple right click from File Explorer, resize one or many images instantly. This code is based on [Brice Lambson's Image Resizer](https://github.com/bricelam/ImageResizer).
<br>
<br>
<br>

<!---
### Keyboard Manager

<img align="left" src="./doc/images/overview/KBM_small.png" /> [Keyboard Manager](src/modules/keyboardmanager/) allows you to customize the keyboard to be more productive by remapping keys and creating your own keyboard shortcuts.
<br>
<br>
<br>
<br>
<br>
-->

<!---
### PowerToys Run

<img align="left" src="./doc/images/overview/PowerLauncher_small.png" /> [PowerToys Run](src/modules/launcher/) is a new toy in PowerToys that can help you search and launch your app instantly! It is open source and modular for additional plugins.  Window Walker is now inside!
<br>
<br>
<br>
<br>
<br>
<br>
-->

### Version 1.0 plan

Our plan for all the [goals and utilities for v1.0 detailed over here in the wiki][v1].

## Installing and running Microsoft PowerToys

 👉 **Note:** Microsoft PowerToys requires Windows 10 1803 (build 17134) or later.

### Via Github with MSI [Recommended]

Install from the [Microsoft PowerToys GitHub releases page][github-release-link]. Click on `Assets` to show the files available in the release and then click on `PowerToysSetup-0.17.0-x64.msi` to download the PowerToys installer.

This is our preferred method.

### Other install methods

#### Via Chocolatey - ⚠ Unofficial ⚠

Download and upgrade PowerToys from [Chocolatey](https://chocolatey.org). If you have any issues when installing/upgrading the package please go to the [package page](https://chocolatey.org/packages/powertoys) and follow the [Chocolatey triage process](https://chocolatey.org/docs/package-triage-process)

To install PowerToys, run the following command from the command line / PowerShell:

```powershell
choco install powertoys
```

To upgrade PowerToys, run the following command from the command line / PowerShell:

```powershell
choco upgrade powertoys
```

### Processor support

We currently support the matrix below.

| x64 | x86 | ARM |
|:---:|:---:|:---:|
| [Supported][github-release-link] | [Issue #602](https://github.com/microsoft/PowerToys/issues/602) | [Issue #490](https://github.com/microsoft/PowerToys/issues/490) |

## What's Happening

### April 2020 Update

Our goals for 0.17 release cycle were updatability and stability.

**Auto-updating:** We just added in the code for doing updating, so the first chance to experience this will be when 0.18 is released. We’re also seeing how aggressive everyone wants with this so right now, you’ll have to click “Install” for it to kick off the installer. This is something we’d love feedback on.

Another thing we did was utilized telemetry from PowerToys to prioritize virtual desktop FancyZone work. We knew there was a subset of bugs caused by an underlying issue and seeing how many users it affected helped us reprioritize to do the work sooner.

- We shipped [v0.17][github-release-link]!
- Auto-updating
- FancyZone improvement:
  - Virtual desktop support should be much better!
  - Better taskbar positioning
- Fixed non-admin install regression.
- Lots of bug fixes!

For [0.18](https://github.com/microsoft/PowerToys/issues?q=is%3Aopen+is%3Aissue+project%3Amicrosoft%2FPowerToys%2F4), we are proactively working on:

- Win+R replacement (Launcher)
- Keyboard remapping
- Performance improvements with FancyZones
- A testing utility for FancyZones to be sure we can test different window configurations.
- Settings v2 / Fix bug #243

## Developer Guidance

Please read the [developer docs](/doc/devdocs) for a detailed breakdown.

## Contributing

This project welcomes contributions of all times. Help spec'ing, design, documentation, finding bugs are ways everyone can help on top of coding features / bug fixes. We are excited to work with the power user community to build a set of tools for helping you get the most out of Windows.

We ask that **before you start work on a feature that you would like to contribute**, please read our [Contributor's Guide](contributing.md). We will be happy to work with you to figure out the best approach, provide guidance and mentorship throughout feature development, and help avoid any wasted or duplicate effort.

### ⚠ State of code ⚠

PowerToys is still a very fluidic project and the team is actively working out of this repository.  We will be periodically re-structuring/refactoring the code to make it easier to comprehend, navigate, build, test, and contribute to, so **DO expect significant changes to code layout on a regular basis**.

### License Info

 Most contributions require you to agree to a [Contributor License Agreement (CLA)][oss-CLA] declaring that you have the right to, and actually do, grant us the rights to use your contribution.

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct][oss-conduct-code].

## Privacy Statement

The application logs basic telemetry. Our Telemetry Data page (Coming Soon) has the trends from the telemetry. Please read the [Microsoft privacy statement][privacyLink] for more information.

[oss-CLA]: https://cla.opensource.microsoft.com
[oss-conduct-code]: CODE_OF_CONDUCT.md
[github-release-link]: https://github.com/microsoft/PowerToys/releases/
[v1]: https://github.com/microsoft/PowerToys/wiki/Version-1.0-Strategy
[privacyLink]: http://go.microsoft.com/fwlink/?LinkId=521839
