﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Microsoft.PowerToys.Settings.UI.Lib
{
    /// <summary>
    /// PowerToys runner expects a json text that contains one of the following attributes: refresh, general and powertoys.
    /// The one for general settings is placed in the General settings model. This class reprentes the json text that starts with the "powertoys" attribute.
    /// this will tell the runner that we are sending settings for a powertoy module and not for general settings.
    /// </summary>
    /// <typeparam name="M">M stands for the Model of PT Module Settings to be sent.</typeparam>
    public class ModuleSettings<M>
    {
        public M powertoys { get; set; }

        public ModuleSettings(M ptModuleSettings)
        {
            this.powertoys = ptModuleSettings;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
