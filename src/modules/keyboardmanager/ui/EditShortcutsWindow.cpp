#include "pch.h"
#include "EditShortcutsWindow.h"
#include "ShortcutControl.h"
#include "KeyDropDownControl.h"

LRESULT CALLBACK EditShortcutsWindowProc(HWND, UINT, WPARAM, LPARAM);

// This Hwnd will be the window handler for the Xaml Island: A child window that contains Xaml.
HWND hWndXamlIslandEditShortcutsWindow = nullptr;
// This variable is used to check if window registration has been done to avoid repeated registration leading to an error.
bool isEditShortcutsWindowRegistrationCompleted = false;
// Holds the native window handle of EditShortcuts Window.
HWND hwndEditShortcutsNativeWindow = nullptr;
std::mutex editShortcutsWindowMutex;

// Function to create the Edit Shortcuts Window
void createEditShortcutsWindow(HINSTANCE hInst, KeyboardManagerState& keyboardManagerState)
{
    // Window Registration
    const wchar_t szWindowClass[] = L"EditShortcutsWindow";

    if (!isEditShortcutsWindowRegistrationCompleted)
    {
        WNDCLASSEX windowClass = {};
        windowClass.cbSize = sizeof(WNDCLASSEX);
        windowClass.lpfnWndProc = EditShortcutsWindowProc;
        windowClass.hInstance = hInst;
        windowClass.lpszClassName = szWindowClass;
        windowClass.hbrBackground = (HBRUSH)(COLOR_WINDOW);
        windowClass.hIconSm = LoadIcon(windowClass.hInstance, IDI_APPLICATION);
        if (RegisterClassEx(&windowClass) == NULL)
        {
            MessageBox(NULL, L"Windows registration failed!", L"Error", NULL);
            return;
        }

        isEditShortcutsWindowRegistrationCompleted = true;
    }

    // Window Creation
    HWND _hWndEditShortcutsWindow = CreateWindow(
        szWindowClass,
        L"Edit Shortcuts",
        WS_OVERLAPPEDWINDOW | WS_VISIBLE,
        CW_USEDEFAULT,
        CW_USEDEFAULT,
        CW_USEDEFAULT,
        CW_USEDEFAULT,
        NULL,
        NULL,
        hInst,
        NULL);
    if (_hWndEditShortcutsWindow == NULL)
    {
        MessageBox(NULL, L"Call to CreateWindow failed!", L"Error", NULL);
        return;
    }

    // Store the newly created Edit Shortcuts window's handle.
    std::unique_lock<std::mutex> hwndLock(editShortcutsWindowMutex);
    hwndEditShortcutsNativeWindow = _hWndEditShortcutsWindow;
    hwndLock.unlock();

    // This DesktopWindowXamlSource is the object that enables a non-UWP desktop application
    // to host UWP controls in any UI element that is associated with a window handle (HWND).
    DesktopWindowXamlSource desktopSource;
    // Get handle to corewindow
    auto interop = desktopSource.as<IDesktopWindowXamlSourceNative>();
    // Parent the DesktopWindowXamlSource object to current window
    check_hresult(interop->AttachToWindow(_hWndEditShortcutsWindow));

    // Get the new child window's hwnd
    interop->get_WindowHandle(&hWndXamlIslandEditShortcutsWindow);
    // Update the xaml island window size becuase initially is 0,0
    SetWindowPos(hWndXamlIslandEditShortcutsWindow, 0, 0, 0, 400, 400, SWP_SHOWWINDOW);

    // Creating the Xaml content. xamlContainer is the parent UI element
    Windows::UI::Xaml::Controls::StackPanel xamlContainer;

    // Header for the window
    Windows::UI::Xaml::Controls::StackPanel header;
    header.Orientation(Windows::UI::Xaml::Controls::Orientation::Horizontal);
    header.Margin({ 10, 10, 10, 30 });
    header.Spacing(10);

    // Header text
    TextBlock headerText;
    headerText.Text(L"Edit Shortcuts");
    headerText.FontSize(30);
    headerText.Margin({ 0, 0, 100, 0 });

    // Cancel button
    Button cancelButton;
    cancelButton.Content(winrt::box_value(L"Cancel"));
    cancelButton.Click([&](winrt::Windows::Foundation::IInspectable const& sender, RoutedEventArgs const&) {
        // Close the window since settings do not need to be saved
        PostMessage(_hWndEditShortcutsWindow, WM_CLOSE, 0, 0);
    });

    // Table to display the shortcuts
    Windows::UI::Xaml::Controls::StackPanel shortcutTable;
    shortcutTable.Margin({ 10, 10, 10, 20 });
    shortcutTable.Spacing(10);

    // Header row of the shortcut table
    Windows::UI::Xaml::Controls::StackPanel tableHeaderRow;
    tableHeaderRow.Spacing(100);
    tableHeaderRow.Orientation(Windows::UI::Xaml::Controls::Orientation::Horizontal);

    // First header textblock in the header row of the shortcut table
    TextBlock originalShortcutHeader;
    originalShortcutHeader.Text(L"Original Shortcut:");
    originalShortcutHeader.FontWeight(Text::FontWeights::Bold());
    originalShortcutHeader.Margin({ 0, 0, 0, 10 });
    tableHeaderRow.Children().Append(originalShortcutHeader);

    // Second header textblock in the header row of the shortcut table
    TextBlock newShortcutHeader;
    newShortcutHeader.Text(L"New Shortcut:");
    newShortcutHeader.FontWeight(Text::FontWeights::Bold());
    newShortcutHeader.Margin({ 0, 0, 0, 10 });
    tableHeaderRow.Children().Append(newShortcutHeader);

    shortcutTable.Children().Append(tableHeaderRow);

    // Message to display success/failure of saving settings.
    Flyout applyFlyout;
    TextBlock settingsMessage;
    applyFlyout.Content(settingsMessage);

    // Store handle of edit shortcuts window
    ShortcutControl::EditShortcutsWindowHandle = _hWndEditShortcutsWindow;
    // Store keyboard manager state
    ShortcutControl::keyboardManagerState = &keyboardManagerState;
    KeyDropDownControl::keyboardManagerState = &keyboardManagerState;
    // Clear the shortcut remap buffer
    ShortcutControl::shortcutRemapBuffer.clear();
    // Vector to store dynamically allocated control objects to avoid early destruction
    std::vector<std::vector<std::unique_ptr<ShortcutControl>>> keyboardRemapControlObjects;

    // Load existing shortcuts into UI
    std::unique_lock<std::mutex> lock(keyboardManagerState.osLevelShortcutReMap_mutex);
    for (const auto& it : keyboardManagerState.osLevelShortcutReMap)
    {
        ShortcutControl::AddNewShortcutControlRow(shortcutTable, keyboardRemapControlObjects, it.first, it.second.targetShortcut);
    }
    lock.unlock();

    // Apply button
    Button applyButton;
    applyButton.Content(winrt::box_value(L"Apply"));
    applyButton.Flyout(applyFlyout);
    applyButton.Click([&](winrt::Windows::Foundation::IInspectable const& sender, RoutedEventArgs const&) {
        bool isSuccess = true;
        // Clear existing shortcuts
        keyboardManagerState.ClearOSLevelShortcuts();

        // Save the shortcuts that are valid and report if any of them were invalid
        for (int i = 0; i < ShortcutControl::shortcutRemapBuffer.size(); i++)
        {
            Shortcut originalShortcut = ShortcutControl::shortcutRemapBuffer[i][0];
            Shortcut newShortcut = ShortcutControl::shortcutRemapBuffer[i][1];

            if (originalShortcut.IsValidShortcut() && newShortcut.IsValidShortcut())
            {
                bool result = keyboardManagerState.AddOSLevelShortcut(originalShortcut, newShortcut);
                if (!result)
                {
                    isSuccess = false;
                }
            }
            else
            {
                isSuccess = false;
            }
        }

        // Save the updated key remaps to file.
        auto saveResult = keyboardManagerState.SaveConfigToFile();

        if (isSuccess && saveResult)
        {
            settingsMessage.Text(L"Remapping successful!");
        }
        else if (!isSuccess && saveResult)
        {
            settingsMessage.Text(L"All remappings were not successfully applied.");
        }
        else
        {
            settingsMessage.Text(L"Failed to save the remappings.");
        }
    });

    header.Children().Append(headerText);
    header.Children().Append(cancelButton);
    header.Children().Append(applyButton);

    // Add shortcut button
    Windows::UI::Xaml::Controls::Button addShortcut;
    FontIcon plusSymbol;
    plusSymbol.FontFamily(Xaml::Media::FontFamily(L"Segoe MDL2 Assets"));
    plusSymbol.Glyph(L"\xE109");
    addShortcut.Content(plusSymbol);
    addShortcut.Margin({ 10 });
    addShortcut.Click([&](winrt::Windows::Foundation::IInspectable const& sender, RoutedEventArgs const&) {
        ShortcutControl::AddNewShortcutControlRow(shortcutTable, keyboardRemapControlObjects);
    });

    xamlContainer.Children().Append(header);
    xamlContainer.Children().Append(shortcutTable);
    xamlContainer.Children().Append(addShortcut);
    xamlContainer.UpdateLayout();
    desktopSource.Content(xamlContainer);

    ////End XAML Island section
    if (_hWndEditShortcutsWindow)
    {
        ShowWindow(_hWndEditShortcutsWindow, SW_SHOW);
        UpdateWindow(_hWndEditShortcutsWindow);
    }

    // Message loop:
    MSG msg = {};
    while (GetMessage(&msg, NULL, 0, 0))
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
    desktopSource.Close();

    hWndXamlIslandEditShortcutsWindow = nullptr;
    hwndLock.lock();
    hwndEditShortcutsNativeWindow = nullptr;
}

LRESULT CALLBACK EditShortcutsWindowProc(HWND hWnd, UINT messageCode, WPARAM wParam, LPARAM lParam)
{
    RECT rcClient;
    switch (messageCode)
    {
    case WM_PAINT:
        GetClientRect(hWnd, &rcClient);
        SetWindowPos(hWndXamlIslandEditShortcutsWindow, 0, rcClient.left, rcClient.top, rcClient.right, rcClient.bottom, SWP_SHOWWINDOW);
        break;
    case WM_DESTROY:
        PostQuitMessage(0);
        break;
    default:
        return DefWindowProc(hWnd, messageCode, wParam, lParam);
        break;
    }

    return 0;
}

bool CheckEditShortcutsWindowActive()
{
    bool result = false;
    std::unique_lock<std::mutex> hwndLock(editShortcutsWindowMutex);
    if (hwndEditShortcutsNativeWindow != nullptr)
    {
        // Check if the window is minimized if yes then restore the window.
        if (IsIconic(hwndEditShortcutsNativeWindow))
        {
            ShowWindow(hwndEditShortcutsNativeWindow, SW_RESTORE);
        }

        // If there is an already existing window no need to create a new open bring it on foreground.
        SetForegroundWindow(hwndEditShortcutsNativeWindow);
        result = true;
    }

    return result;
}
