using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Constants = EditorConstantsForModuleSystem;

namespace ZFramework.Editor
{
    /// <summary>
    /// A window with all sorts of auxiliary functions for ZFramework modules (an extension for the Unity editor)
    /// </summary>
    public class ModuleHelpersWindow : EditorWindow
    {
        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Private fields (static)
        // ---------------------------------------------------------------------------------------------------------

        private static Dictionary<string, string> _settingStringCollection = new Dictionary<string, string>();

        #endregion

        #region App lifecycle

        [MenuItem("Window/ZFramework/Module Editor/Helpers")]
        private static void Init()
        {
            if(_settingStringCollection.Count == 0)
            {
                InitCollections();
            }

            ModuleHelpersWindow window = (ModuleHelpersWindow)EditorWindow.GetWindow(typeof(ModuleHelpersWindow), false, Constants.L_ModuleHelpersTitle);
            window.Show();
        }


        private static void InitCollections()
        {
            _settingStringCollection.Add("module-name", "");
        }

        #endregion

        #region Unity lifecycle & UI

        void OnGUI()
        {
            if (_settingStringCollection.Count == 0)
            {
                InitCollections();
            }

            GUILayout.Label(Constants.L_GenerateHash, EditorStyles.boldLabel);
            _settingStringCollection["module-name"] = EditorGUILayout.TextField("Module name:", _settingStringCollection["module-name"]);

            if (GUILayout.Button(Constants.L_GenerateModuleHash))
            {
                if (!string.IsNullOrEmpty(_settingStringCollection["module-name"]))
                {
                    Debug.Log(Constants.L_HashForModules + " \"" +
                        _settingStringCollection["module-name"] + "\": " +
                        CreateModuleWindow.GenerateModuleHash(_settingStringCollection["module-name"]));
                }
            }
        }

        #endregion
    }
}