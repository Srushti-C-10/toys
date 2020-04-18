#pragma once
#include <keyboardmanager/common/KeyboardManagerState.h>
#include "KeyDropDownControl.h"

class SingleKeyRemapControl
{
private:
    // Drop down to display the selected remap key
    KeyDropDownControl singleKeyRemapDropDown;

    // Button to type the remap key
    Button typeKey;

    // StackPanel to parent the above controls
    StackPanel singleKeyRemapControlLayout;

public:
    // Handle to the current Edit Keyboard Window
    static HWND EditKeyboardWindowHandle;
    // Pointer to the keyboard manager state
    static KeyboardManagerState* keyboardManagerState;
    // Stores the current list of remappings
    static std::vector<std::vector<DWORD>> singleKeyRemapBuffer;

    SingleKeyRemapControl(const size_t rowIndex, const size_t colIndex) :
        singleKeyRemapDropDown(rowIndex, colIndex, singleKeyRemapBuffer)
    {
        typeKey.Content(winrt::box_value(winrt::to_hstring("Type Key")));
        typeKey.Background(Windows::UI::Xaml::Media::SolidColorBrush{ Windows::UI::Colors::LightGray() });
        typeKey.Foreground(Windows::UI::Xaml::Media::SolidColorBrush{ Windows::UI::Colors::Black() });
        typeKey.Click([&, rowIndex, colIndex](IInspectable const& sender, RoutedEventArgs const&) {
            keyboardManagerState->SetUIState(KeyboardManagerUIState::DetectSingleKeyRemapWindowActivated, EditKeyboardWindowHandle);
            // Using the XamlRoot of the typeKey to get the root of the XAML host
            createDetectKeyWindow(sender, sender.as<Button>().XamlRoot(), singleKeyRemapBuffer, *keyboardManagerState, rowIndex, colIndex);
        });

        singleKeyRemapControlLayout.Background(Windows::UI::Xaml::Media::SolidColorBrush{ Windows::UI::Colors::LightGray() });
        singleKeyRemapControlLayout.Margin({ 0, 0, 0, 10 });
        singleKeyRemapControlLayout.Spacing(10);

        singleKeyRemapControlLayout.Children().Append(typeKey);
        singleKeyRemapControlLayout.Children().Append(singleKeyRemapDropDown.GetComboBox());
        singleKeyRemapControlLayout.UpdateLayout();
    }

    // Function to add a new row to the remap keys table. If the originalKey and newKey args are provided, then the displayed remap keys are set to those values.
    static void AddNewControlKeyRemapRow(StackPanel& parent, std::vector<std::vector<std::unique_ptr<SingleKeyRemapControl>>>& keyboardRemapControlObjects, const DWORD originalKey = NULL, const DWORD newKey = NULL);

    // Function to return the stack panel element of the SingleKeyRemapControl. This is the externally visible UI element which can be used to add it to other layouts
    StackPanel getSingleKeyRemapControl();

    // Function to create the detect remap keys UI window
    void createDetectKeyWindow(IInspectable const& sender, XamlRoot xamlRoot, std::vector<std::vector<DWORD>>& singleKeyRemapBuffer, KeyboardManagerState& keyboardManagerState, const size_t rowIndex, const size_t colIndex);
};
