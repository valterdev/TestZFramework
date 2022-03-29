using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZFramework.Editor
{
    /// <summary>
    /// Окно со всякими вспомогательными функциями для модулей ZFramework(расширение для редактора Unity)
    /// </summary>
    public class ModuleHelpersWindow : EditorWindow
    {
        private static Dictionary<string, string> _settingStringCollection = new Dictionary<string, string>();

        [MenuItem("Window/ZFramework/Module Editor/Helpers")]
        static void Init()
        {
            if(_settingStringCollection.Count == 0)
            {
                InitCollections();
            }

            ModuleHelpersWindow window = (ModuleHelpersWindow)EditorWindow.GetWindow(typeof(ModuleHelpersWindow), false, "Module Helpers");
            window.Show();
        }

        static void InitCollections()
        {

            _settingStringCollection.Add("module-name", "");
        }

        void OnGUI()
        {
            if (_settingStringCollection.Count == 0)
            {
                InitCollections();
            }

            GUILayout.Label("Generate hash", EditorStyles.boldLabel);
            _settingStringCollection["module-name"] = EditorGUILayout.TextField("Module name:", _settingStringCollection["module-name"]);

            if (GUILayout.Button("Generate module hash"))
            {
                if (!string.IsNullOrEmpty(_settingStringCollection["module-name"]))
                {
                    Debug.Log("Hash for module with name \"" +
                        _settingStringCollection["module-name"] + "\": " +
                        CreateModuleWindow.GenerateModuleHash(_settingStringCollection["module-name"]));
                }
            }
        }
    }
}