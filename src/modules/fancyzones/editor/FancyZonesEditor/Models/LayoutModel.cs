// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FancyZonesEditor.Models
{
    // Base LayoutModel
    //  Manages common properties and base persistence
    public abstract class LayoutModel : INotifyPropertyChanged
    {
        protected LayoutModel()
        {
            _guid = Guid.NewGuid();
            Type = LayoutType.Custom;
        }

        protected LayoutModel(string name)
            : this()
        {
            Name = name;
        }

        protected LayoutModel(string uuid, string name, LayoutType type)
            : this()
        {
            _guid = Guid.Parse(uuid);
            Name = name;
            Type = type;
        }

        protected LayoutModel(string name, LayoutType type)
            : this(name)
        {
            _guid = Guid.NewGuid();
            Type = type;
        }

        // Name - the display name for this layout model - is also used as the key in the registry
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (_name != value)
                {
                    _name = value;
                    FirePropertyChanged();
                }
            }
        }

        private string _name;

        public LayoutType Type { get; set; }

        public Guid Guid
        {
            get
            {
                return _guid;
            }
        }

        private Guid _guid;

        public string Uuid
        {
            get
            {
                return "{" + Guid.ToString().ToUpperInvariant() + "}";
            }
        }

        // IsSelected (not-persisted) - tracks whether or not this LayoutModel is selected in the picker
        // TODO: once we switch to a picker per monitor, we need to move this state to the view
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }

            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    FirePropertyChanged();
                }
            }
        }

        private bool _isSelected;

        public bool IsApplied
        {
            get
            {
                return _isApplied;
            }

            set
            {
                if (_isApplied != value)
                {
                    _isApplied = value;
                    FirePropertyChanged();
                }
            }
        }

        private bool _isApplied;

        // implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // FirePropertyChanged -- wrapper that calls INPC.PropertyChanged
        protected virtual void FirePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Removes this Layout from the registry and the loaded CustomModels list
        public void Delete()
        {
            var customModels = MainWindowSettingsModel.CustomModels;
            int i = customModels.IndexOf(this);
            if (i != -1)
            {
                customModels.RemoveAt(i);
            }
        }

        // Adds new custom Layout
        public void AddCustomLayout(LayoutModel model)
        {
            bool updated = false;
            var customModels = MainWindowSettingsModel.CustomModels;
            for (int i = 0; i < customModels.Count && !updated; i++)
            {
                if (customModels[i].Uuid == model.Uuid)
                {
                    customModels[i] = model;
                    updated = true;
                }
            }

            if (!updated)
            {
                customModels.Add(model);
            }

            App.FancyZonesEditorIO.SerializeZoneSettings();
        }

        // Callbacks that the base LayoutModel makes to derived types
        protected abstract void PersistData();

        public abstract LayoutModel Clone();

        public void Persist()
        {
            PersistData();
            Apply();
        }

        public void Apply()
        {
            MainWindowSettingsModel settings = ((App)App.Current).MainWindowSettings;
            settings.ResetAppliedModel();
            IsApplied = true;

            // update settings
            App.Overlay.CurrentLayoutSettings.ZonesetUuid = Uuid;
            App.Overlay.CurrentLayoutSettings.Type = Type;

            // update temp file
            App.FancyZonesEditorIO.SerializeZoneSettings();
        }
    }
}
