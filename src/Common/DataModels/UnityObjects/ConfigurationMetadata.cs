using Common.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DataModels.UnityObjects
{
    public class ConfigurationMetadata
    {

        public bool Refresh()
        {
            // First Refresh is always dirty.
            bool modified = this.Dirty;
            this.Dirty = false;

            return modified;
        }
        private bool Dirty { get; set; } = true;

        private GameModeId? _GameMode;
        public GameModeId? GameMode 
        { 
            get { return _GameMode; } 
            set 
            { 
                Dirty = true;
                _GameMode = value; 
            } 
        }
    }
}
