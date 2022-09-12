using System;
using System.Collections.Generic;
using Constants = EditorConstantsForModuleSystem;

namespace ZFramework.Editor
{
    /// <summary>
    /// Class describing the meta data of ZFramework modules
    /// </summary>
    [Serializable]
    public class ModuleInfo
    {
        #region Fields

        /// <summary>
        /// The hash code is generated automatically and uses the name of the module, but modules with the same name will generate a different hash code.
        /// The hash code is set once when creating a module.
        /// </summary>
        public string hash;
        public string category;
        public string name;

        /// <summary>
        /// The name of the module manager that will be used when generating the Z0_PreInit.cs file
        /// </summary>
        public string custom_manager_name;

        public string version;
        public string author;
        public string site;
        public string email;

        /// <summary>
        /// Module type. It depends on it in which folder the module will be located (System or Domain).
        ///
        /// System/Core = 0
        /// System/Data = 1
        /// System/Network = 2
        /// System/Other = 3
        /// System/Tools = 4
        ///
        /// Domain/Mechanics_Features = 10
        /// Domain/Meta = 11
        /// Domain/UI = 12
        /// Domain/Other = 13
        /// Domain/Tools = 14
        /// </summary>
        public int type;
        public int state;
        public string description;

        public bool preinit;
        /// <summary>
        /// Hidden property that is set manually in the json file. If it is true, it means that the system will not work without this module (or it will, but this is not expected).
        /// </summary>
        public bool critical;

        /// <summary>
        /// Dependencies on other modules
        /// </summary>
        public List<string> dependencies;

        /// <summary>
        /// Independent scripts used in the module (placed in the Modules/System/Utils folder)
        /// </summary>
        public List<string> utils;

        /// <summary>
        /// unity plugins used in the module (UPM | .unitypackage | Packages/manifest.json)
        /// </summary>
        public List<string> plugins;

        /// <summary>
        /// files used in the module are located in the content folder, and not in the folder with the module
        /// </summary>
        public List<string> content_files;

        [NonSerialized]
        /// <summary>
        /// Hidden property that determines which modules are active and which are not. The active ones are in the Assets folder, and the non-active ones are in the ZFramework/DisabledModules/ folder in the root folder of the project.
        /// </summary>
        public bool active;

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods (static)
        // ---------------------------------------------------------------------------------------------------------

        public static string GetModuleCategoryByType(int value) =>
            value switch
            {
                0 => Constants.System_Core,
                1 => Constants.System_Data,
                2 => Constants.System_Network,
                3 => Constants.System_Other,
                4 => Constants.System_Tools,

                10 => Constants.Domain_Mechanics_Features,
                11 => Constants.Domain_Meta,
                12 => Constants.Domain_UI,
                13 => Constants.Domain_Other,
                14 => Constants.Domain_Tools,

                _ => string.Empty,
            };

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods
        // ---------------------------------------------------------------------------------------------------------

        public string GetModulePath()
        {
            return Constants.PathToAppModules + GetModuleCategoryByType(type) + "/" + name;
        }

        #endregion
    }
}