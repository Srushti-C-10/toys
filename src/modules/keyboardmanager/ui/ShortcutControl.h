#pragma once
#include <keyboardmanager/common/KeyboardManagerState.h>
#include <keyboardManager/common/Helpers.h>
#include <keyboardmanager/common/Shortcut.h>
#include "KeyDropDownControl.h"

class ShortcutControl
{
private:
    // Textblock to display the selected shortcut
    TextBlock shortcutText;

    // Stack panel for the drop downs to display the selected shortcut
    StackPanel shortcutDropDownStackPanel;

    // Button to type the shortcut
    Button typeShortcut;

    // StackPanel to parent the above controls
    StackPanel shortcutControlLayout;

public:
    // Handle to the current Edit Shortcuts Window
    static HWND EditShortcutsWindowHandle;
    // Pointer to the keyboard manager state
    static KeyboardManagerState* keyboardManagerState;
    // Stores the current list of remappings
    static std::vector<std::vector<Shortcut>> shortcutRemapBuffer;
    // Vector to store dynamically allocated KeyDropDownControl objects to avoid early destruction
    std::vector<std::unique_ptr<KeyDropDownControl>> keyDropDownControlObjects;

    ShortcutControl(const size_t rowIndex, const size_t colIndex)
    {
        shortcutDropDownStackPanel.Spacing(10);
        shortcutDropDownStackPanel.Orientation(Windows::UI::Xaml::Controls::Orientation::Horizontal);
        KeyDropDownControl::AddDropDown(shortcutDropDownStackPanel, rowIndex, colIndex, shortcutRemapBuffer, keyDropDownControlObjects);

        typeShortcut.Content(winrt::box_value(winrt::to_hstring("Type Shortcut")));
        typeShortcut.Click([&, rowIndex, colIndex](winrt::Windows::Foundation::IInspectable const& sender, RoutedEventArgs const&) {
            keyboardManagerState->SetUIState(KeyboardManagerUIState::DetectShortcutWindowActivated, EditShortcutsWindowHandle);
            // Using the XamlRoot of the typeShortcut to get the root of the XAML host
            createDetectShortcutWindow(sender, sender.as<Button>().XamlRoot(), shortcutRemapBuffer, *keyboardManagerState, rowIndex, colIndex);
        });

        shortcutControlLayout.Margin({ 0, 0, 0, 10 });
        shortcutControlLayout.Spacing(10);

        shortcutControlLayout.Children().Append(typeShortcut);
        shortcutControlLayout.Children().Append(shortcutDropDownStackPanel);
        shortcutControlLayout.UpdateLayout();
    }

    // Function to add a new row to the shortcut table. If the originalKeys and newKeys args are provided, then the displayed shortcuts are set to those values.
    static void AddNewShortcutControlRow(StackPanel& parent, std::vector<std::vector<std::unique_ptr<ShortcutControl>>>& keyboardRemapControlObjects, Shortcut originalKeys = Shortcut(), Shortcut newKeys = Shortcut());

    // Function to add a shortcut to the shortcut control as combo boxes
    void AddShortcutToControl(Shortcut& shortcut, StackPanel parent, KeyboardManagerState& keyboardManagerState, const size_t rowIndex, const size_t colIndex);

    // Function to return the stack panel element of the ShortcutControl. This is the externally visible UI element which can be used to add it to other layouts
    StackPanel getShortcutControl();

    // Function to create the detect shortcut UI window
    void createDetectShortcutWindow(winrt::Windows::Foundation::IInspectable const& sender, XamlRoot xamlRoot, std::vector<std::vector<Shortcut>>& shortcutRemapBuffer, KeyboardManagerState& keyboardManagerState, const size_t rowIndex, const size_t colIndex);
};
