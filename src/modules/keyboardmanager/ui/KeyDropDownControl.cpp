#include "pch.h"
#include "KeyDropDownControl.h"
#include "keyboardmanager/common/Helpers.h"

// Initialized to null
KeyboardManagerState* KeyDropDownControl::keyboardManagerState = nullptr;

// Function to set properties apart from the SelectionChanged event handler
void KeyDropDownControl::SetDefaultProperties(bool isShortcut)
{
    dropDown.Width(100);
    dropDown.MaxDropDownHeight(200);
    // Initialise layout attribute
    previousLayout = GetKeyboardLayout(0);
    keyCodeList = keyboardManagerState->keyboardMap.GetKeyCodeList(isShortcut);
    dropDown.ItemsSource(KeyboardManagerHelper::ToBoxValue(keyboardManagerState->keyboardMap.GetKeyNameList(isShortcut)));
    // drop down open handler - to reload the items with the latest layout
    dropDown.DropDownOpened([&, isShortcut](winrt::Windows::Foundation::IInspectable const& sender, auto args) {
        ComboBox currentDropDown = sender.as<ComboBox>();
        CheckAndUpdateKeyboardLayout(currentDropDown, isShortcut);
    });
}

// Function to check if the layout has changed and accordingly update the drop down list
void KeyDropDownControl::CheckAndUpdateKeyboardLayout(ComboBox currentDropDown, bool isShortcut)
{
    // Get keyboard layout for current thread
    HKL layout = GetKeyboardLayout(0);

    // Check if the layout has changed
    if (previousLayout != layout)
    {
        keyCodeList = keyboardManagerState->keyboardMap.GetKeyCodeList(isShortcut);
        currentDropDown.ItemsSource(KeyboardManagerHelper::ToBoxValue(keyboardManagerState->keyboardMap.GetKeyNameList(isShortcut)));
        previousLayout = layout;
    }
}

// Function to set selection handler for single key remap drop down. Needs to be called after the constructor since the singleKeyControl StackPanel is null if called in the constructor
void KeyDropDownControl::SetSelectionHandler(Grid& table, StackPanel& singleKeyControl, int colIndex, std::vector<std::vector<DWORD>>& singleKeyRemapBuffer)
{
    // drop down selection handler
    dropDown.SelectionChanged([&, table, singleKeyControl, colIndex](winrt::Windows::Foundation::IInspectable const& sender, SelectionChangedEventArgs const& args) {
        ComboBox currentDropDown = sender.as<ComboBox>();
        int selectedKeyIndex = currentDropDown.SelectedIndex();
        // Get row index of the single key control
        uint32_t controlIndex;
        bool indexFound = table.Children().IndexOf(singleKeyControl, controlIndex);
        if (indexFound)
        {
            KeyboardManagerHelper::ErrorType errorType = KeyboardManagerHelper::ErrorType::NoError;
            int rowIndex = (controlIndex - 2) / 4;
            // Check if the element was not found or the index exceeds the known keys
            if (selectedKeyIndex != -1 && keyCodeList.size() > selectedKeyIndex)
            {
                // Check if the value being set is the same as the other column
                if (singleKeyRemapBuffer[rowIndex][std::abs(int(colIndex) - 1)] == keyCodeList[selectedKeyIndex])
                {
                    errorType = KeyboardManagerHelper::ErrorType::MapToSameKey;
                }

                if (errorType == KeyboardManagerHelper::ErrorType::NoError && colIndex == 0)
                {
                    // Check if the key is already remapped to something else
                    for (int i = 0; i < singleKeyRemapBuffer.size(); i++)
                    {
                        if (i != rowIndex)
                        {
                            KeyboardManagerHelper::ErrorType result = KeyboardManagerHelper::DoKeysOverlap(singleKeyRemapBuffer[i][0], keyCodeList[selectedKeyIndex]);
                            if (result != KeyboardManagerHelper::ErrorType::NoError)
                            {
                                errorType = result;
                                break;
                            }
                        }
                    }
                }

                // If there is no error, set the buffer
                if (errorType == KeyboardManagerHelper::ErrorType::NoError)
                {
                    singleKeyRemapBuffer[rowIndex][colIndex] = keyCodeList[selectedKeyIndex];
                }
                else
                {
                    singleKeyRemapBuffer[rowIndex][colIndex] = NULL;
                }
            }
            else
            {
                // Reset to null if the key is not found
                singleKeyRemapBuffer[rowIndex][colIndex] = NULL;
            }

            if (errorType != KeyboardManagerHelper::ErrorType::NoError)
            {
                SetDropDownError(currentDropDown, KeyboardManagerHelper::GetErrorMessage(errorType));
            }

            // If either of the keys are invalid and the tooltip content has been modified after initialization, display the warning
            if ((singleKeyRemapBuffer[rowIndex][0] == NULL || singleKeyRemapBuffer[rowIndex][1] == NULL) && toolTip.Content().as<winrt::Windows::Foundation::IPropertyValue>().GetString() != KeyboardManagerConstants::ToolTipInitialContent)
            {
                warning.Visibility(Visibility::Visible);
            }
            else
            {
                warning.Visibility(Visibility::Collapsed);
            }
        }
    });
}

// Function to set selection handler for shortcut drop down. Needs to be called after the constructor since the shortcutControl StackPanel is null if called in the constructor
void KeyDropDownControl::SetSelectionHandler(Grid& table, StackPanel& shortcutControl, StackPanel parent, int colIndex, std::vector<std::vector<Shortcut>>& shortcutRemapBuffer, std::vector<std::unique_ptr<KeyDropDownControl>>& keyDropDownControlObjects)
{
    // drop down selection handler
    dropDown.SelectionChanged([&, table, shortcutControl, colIndex, parent](winrt::Windows::Foundation::IInspectable const& sender, SelectionChangedEventArgs const&) {
        ComboBox currentDropDown = sender.as<ComboBox>();
        int selectedKeyIndex = currentDropDown.SelectedIndex();
        uint32_t dropDownIndex = -1;
        bool dropDownFound = parent.Children().IndexOf(currentDropDown, dropDownIndex);
        // Get row index of the single key control
        uint32_t controlIndex;
        bool controlIindexFound = table.Children().IndexOf(shortcutControl, controlIndex);
        KeyboardManagerHelper::ErrorType errorType = KeyboardManagerHelper::ErrorType::NoError;

        if (controlIindexFound)
        {
            int rowIndex = (controlIndex - 2) / 4;
            if (selectedKeyIndex != -1 && keyCodeList.size() > selectedKeyIndex && dropDownFound)
            {
                // If only 1 drop down and action key is chosen: Warn that a modifier must be chosen
                if (parent.Children().Size() == 1 && !KeyboardManagerHelper::IsModifierKey(keyCodeList[selectedKeyIndex]))
                {
                    // warn and reset the drop down
                    errorType = KeyboardManagerHelper::ErrorType::ShortcutStartWithModifier;
                }
                // If it is the last drop down
                else if (dropDownIndex == parent.Children().Size() - 1)
                {
                    // If last drop down and a modifier is selected: add a new drop down (max of 5 drop downs should be enforced)
                    if (KeyboardManagerHelper::IsModifierKey(keyCodeList[selectedKeyIndex]) && parent.Children().Size() < 3)
                    {
                        // If it matched any of the previous modifiers then reset that drop down
                        if (CheckRepeatedModifier(parent, dropDownIndex, selectedKeyIndex, keyCodeList))
                        {
                            // warn and reset the drop down
                            errorType = KeyboardManagerHelper::ErrorType::ShortcutCannotHaveRepeatedModifier;
                        }
                        // If not, add a new drop down
                        else
                        {
                            AddDropDown(table, shortcutControl, parent, colIndex, shortcutRemapBuffer, keyDropDownControlObjects, warning, toolTip);
                        }
                    }
                    // If last drop down and a modifier is selected but there are already 5 drop downs: warn the user
                    else if (KeyboardManagerHelper::IsModifierKey(keyCodeList[selectedKeyIndex]) && parent.Children().Size() >= 3)
                    {
                        // warn and reset the drop down
                        errorType = KeyboardManagerHelper::ErrorType::ShortcutOneActionKey;
                    }
                    // If None is selected but it's the last index: warn
                    else if (keyCodeList[selectedKeyIndex] == 0)
                    {
                        // warn and reset the drop down
                        errorType = KeyboardManagerHelper::ErrorType::ShortcutOneActionKey;
                    }
                    // If none of the above, then the action key will be set
                }
                // If it is the not the last drop down
                else
                {
                    if (KeyboardManagerHelper::IsModifierKey(keyCodeList[selectedKeyIndex]))
                    {
                        // If it matched any of the previous modifiers then reset that drop down
                        if (CheckRepeatedModifier(parent, dropDownIndex, selectedKeyIndex, keyCodeList))
                        {
                            // warn and reset the drop down
                            errorType = KeyboardManagerHelper::ErrorType::ShortcutCannotHaveRepeatedModifier;
                        }
                        // If not, the modifier key will be set
                    }
                    // If None is selected and there are more than 2 drop downs
                    else if (keyCodeList[selectedKeyIndex] == 0 && parent.Children().Size() > 2)
                    {
                        // delete drop down
                        parent.Children().RemoveAt(dropDownIndex);
                        // delete drop down control object from the vector so that it can be destructed
                        keyDropDownControlObjects.erase(keyDropDownControlObjects.begin() + dropDownIndex);
                        parent.UpdateLayout();
                    }
                    else if (keyCodeList[selectedKeyIndex] == 0 && parent.Children().Size() <= 2)
                    {
                        // warn and reset the drop down
                        errorType = KeyboardManagerHelper::ErrorType::ShortcutAtleast2Keys;
                    }
                    // If the user tries to set an action key check if all drop down menus after this are empty if it is not the first key
                    else if (dropDownIndex != 0)
                    {
                        bool isClear = true;
                        for (int i = dropDownIndex + 1; i < (int)parent.Children().Size(); i++)
                        {
                            ComboBox ItDropDown = parent.Children().GetAt(i).as<ComboBox>();
                            if (ItDropDown.SelectedIndex() != -1)
                            {
                                isClear = false;
                                break;
                            }
                        }

                        if (isClear)
                        {
                            // remove all the drop down
                            int elementsToBeRemoved = parent.Children().Size() - dropDownIndex - 1;
                            for (int i = 0; i < elementsToBeRemoved; i++)
                            {
                                parent.Children().RemoveAtEnd();
                                keyDropDownControlObjects.erase(keyDropDownControlObjects.end() - 1);
                            }
                            parent.UpdateLayout();
                        }
                        else
                        {
                            // warn and reset the drop down
                            errorType = KeyboardManagerHelper::ErrorType::ShortcutNotMoreThanOneActionKey;
                        }
                    }
                    // If there an action key is chosen on the first drop down and there are more than one drop down menus
                    else
                    {
                        // warn and reset the drop down
                        errorType = KeyboardManagerHelper::ErrorType::ShortcutStartWithModifier;
                    }
                }
            }

            // After validating the shortcut, now for errors like remap to same shortcut, remap shortcut more than once, Win L and Ctrl Alt Del
            if (errorType == KeyboardManagerHelper::ErrorType::NoError)
            {
                Shortcut tempShortcut;
                tempShortcut.SetKeyCodes(GetKeysFromStackPanel(parent));
                // Check if the value being set is the same as the other column
                if (shortcutRemapBuffer[rowIndex][std::abs(int(colIndex) - 1)] == tempShortcut)
                {
                    errorType = KeyboardManagerHelper::ErrorType::MapToSameShortcut;
                }

                if (errorType == KeyboardManagerHelper::ErrorType::NoError && colIndex == 0)
                {
                    // Check if the key is already remapped to something else
                    for (int i = 0; i < shortcutRemapBuffer.size(); i++)
                    {
                        if (i != rowIndex)
                        {
                            KeyboardManagerHelper::ErrorType result = Shortcut::DoKeysOverlap(shortcutRemapBuffer[i][0], tempShortcut);
                            if (result != KeyboardManagerHelper::ErrorType::NoError)
                            {
                                errorType = result;
                                break;
                            }
                        }
                    }
                }

                if (errorType == KeyboardManagerHelper::ErrorType::NoError)
                {
                    errorType = tempShortcut.IsShortcutIllegal();
                }
            }

            if (errorType != KeyboardManagerHelper::ErrorType::NoError)
            {
                SetDropDownError(currentDropDown, KeyboardManagerHelper::GetErrorMessage(errorType));
            }
            // Reset the buffer based on the new selected drop down items
            shortcutRemapBuffer[rowIndex][colIndex].SetKeyCodes(GetKeysFromStackPanel(parent));

            // If either of the shortcuts are invalid and the tooltip content has been modified after initialization, display the warning
            if ((!shortcutRemapBuffer[rowIndex][0].IsValidShortcut() || !shortcutRemapBuffer[rowIndex][1].IsValidShortcut()) && toolTip.Content().as<winrt::Windows::Foundation::IPropertyValue>().GetString() != KeyboardManagerConstants::ToolTipInitialContent)
            {
                warning.Visibility(Visibility::Visible);
            }
            else
            {
                warning.Visibility(Visibility::Collapsed);
            }
        }

        // If the user searches for a key the selection handler gets invoked however if they click away it reverts back to the previous state. This can result in dangling references to added drop downs which were then reset.
        // We handle this by removing the drop down if it no longer a child of the parent
        for (long long i = keyDropDownControlObjects.size() - 1; i >= 0; i--)
        {
            uint32_t index;
            bool found = parent.Children().IndexOf(keyDropDownControlObjects[i]->GetComboBox(), index);
            if (!found)
            {
                keyDropDownControlObjects.erase(keyDropDownControlObjects.begin() + i);
            }
        }
    });
}

// Function to set the selected index of the drop down
void KeyDropDownControl::SetSelectedIndex(int32_t index)
{
    dropDown.SelectedIndex(index);
}

// Function to return the combo box element of the drop down
ComboBox KeyDropDownControl::GetComboBox()
{
    return dropDown;
}

// Function to add a drop down to the shortcut stack panel
void KeyDropDownControl::AddDropDown(Grid table, StackPanel shortcutControl, StackPanel parent, const int colIndex, std::vector<std::vector<Shortcut>>& shortcutRemapBuffer, std::vector<std::unique_ptr<KeyDropDownControl>>& keyDropDownControlObjects, FontIcon warning, ToolTip toolTip)
{
    keyDropDownControlObjects.push_back(std::move(std::unique_ptr<KeyDropDownControl>(new KeyDropDownControl(true, warning, toolTip))));
    parent.Children().Append(keyDropDownControlObjects[keyDropDownControlObjects.size() - 1]->GetComboBox());
    keyDropDownControlObjects[keyDropDownControlObjects.size() - 1]->SetSelectionHandler(table, shortcutControl, parent, colIndex, shortcutRemapBuffer, keyDropDownControlObjects);
    parent.UpdateLayout();
}

// Function to get the list of key codes from the shortcut combo box stack panel
std::vector<DWORD> KeyDropDownControl::GetKeysFromStackPanel(StackPanel parent)
{
    std::vector<DWORD> keys;
    std::vector<DWORD> keyCodeList = keyboardManagerState->keyboardMap.GetKeyCodeList(true);
    for (int i = 0; i < (int)parent.Children().Size(); i++)
    {
        ComboBox currentDropDown = parent.Children().GetAt(i).as<ComboBox>();
        int selectedKeyIndex = currentDropDown.SelectedIndex();
        if (selectedKeyIndex != -1 && keyCodeList.size() > selectedKeyIndex)
        {
            // If None is not the selected key
            if (keyCodeList[selectedKeyIndex] != 0)
            {
                keys.push_back(keyCodeList[selectedKeyIndex]);
            }
        }
    }

    return keys;
}

// Function to check if a modifier has been repeated in the previous drop downs
bool KeyDropDownControl::CheckRepeatedModifier(StackPanel parent, uint32_t dropDownIndex, int selectedKeyIndex, const std::vector<DWORD>& keyCodeList)
{
    // check if modifier has already been added before in a previous drop down
    std::vector<DWORD> currentKeys = GetKeysFromStackPanel(parent);
    bool matchPreviousModifier = false;
    for (int i = 0; i < currentKeys.size(); i++)
    {
        // Skip the current drop down
        if (i != dropDownIndex)
        {
            // If the key type for the newly added key matches any of the existing keys in the shortcut
            if (KeyboardManagerHelper::GetKeyType(keyCodeList[selectedKeyIndex]) == KeyboardManagerHelper::GetKeyType(currentKeys[i]))
            {
                matchPreviousModifier = true;
                break;
            }
        }
    }

    return matchPreviousModifier;
}

// Function to set the warning message
void KeyDropDownControl::SetDropDownError(ComboBox currentDropDown, hstring message)
{
    currentDropDown.SelectedIndex(-1);
    toolTip.Content(box_value(message));
}
