using System;
using System.Collections.Generic;

namespace ZFramework.Editor
{
    [Serializable]
    public class ProjectInfo
    {
        public string hash;
        public string name;

        public List<string> plugins = new List<string>();
        public List<ModuleDisabledInfo> not_active_modules = new List<ModuleDisabledInfo>();
    }
}