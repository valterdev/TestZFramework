using System;
using System.Collections.Generic;

namespace ZFramework.Editor
{
    [Serializable]
    public class ProjectInfo
    {
        #region Fields

        // Public Fields
        public string hash;
        public string name;

        public List<string> plugins = new List<string>();
        public List<UtilityInfo> utils = new List<UtilityInfo>();
        public List<ModuleDisabledInfo> not_active_modules = new List<ModuleDisabledInfo>();

        #endregion
    }
}