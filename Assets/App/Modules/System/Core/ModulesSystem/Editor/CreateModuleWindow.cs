using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ZFramework.Editor
{
    /// <summary>
    /// Окно позволяющее создавать и задавать мета данные для новых модулей ZFramework (Расширение для редактора Unity)
    /// </summary>
    public class CreateModuleWindow : EditorWindow
    {
        enum MStateName
        {
            NotCompleted = 0,
            Alpha = 10,
            Beta = 20,
            Stable = 30,
            Migrated = 90,
            MigrateNotNormalAdapted = 100
        }

        enum MTypeName
        {
            System = 0,
            Domain = 10
        }

        private static Dictionary<string, bool> _settingBoolCollection = new Dictionary<string, bool>();

        private MTypeName _moduleType;
        private MStateName _moduleState;
        private static ModuleInfo _moduleInfo;

        [MenuItem("Window/ZFramework/Module Editor/Create Module")]
        static void Init()
        {
            if (_moduleInfo == null)
            {
                InitCollections();
            }

            CreateModuleWindow window = (CreateModuleWindow)EditorWindow.GetWindow(typeof(CreateModuleWindow), false, "Create module");
            window.Show();
        }

        static void InitCollections()
        {
            _moduleInfo = new ModuleInfo();
            _moduleInfo.description = "Description";
            _moduleInfo.preinit = true;

            _settingBoolCollection.Add("singleton", true);
            _settingBoolCollection.Add("hooks", false);
            _settingBoolCollection.Add("store", false);
        }

        void OnGUI()
        {
            if (_moduleInfo == null)
            {
                InitCollections();
            }

            GUILayout.Label("Settings", EditorStyles.boldLabel);
            _moduleInfo.name = EditorGUILayout.TextField("Module name:", _moduleInfo.name);
            _moduleInfo.version = EditorGUILayout.TextField("Module version:", _moduleInfo.version);

            _moduleType = (MTypeName)EditorGUILayout.EnumPopup("Type:", _moduleType);
            _moduleInfo.type = (int)_moduleType;
            _moduleState = (MStateName)EditorGUILayout.EnumPopup("State:", _moduleState);
            _moduleInfo.state = (int)_moduleState;
            GUILayout.Space(10);

            _moduleInfo.author = EditorGUILayout.TextField("Module author:", _moduleInfo.author);
            _moduleInfo.site = EditorGUILayout.TextField("Support site:", _moduleInfo.site);
            _moduleInfo.email = EditorGUILayout.TextField("Support email:", _moduleInfo.email);
            GUILayout.Space(10);
            _moduleInfo.description = GUILayout.TextArea(_moduleInfo.description, GUILayout.Height(150));

            GUILayout.BeginHorizontal();
            _settingBoolCollection["singleton"] = EditorGUILayout.Toggle("Singleton manager:", _settingBoolCollection["singleton"]);
            _moduleInfo.preinit = EditorGUILayout.Toggle("PreInit:", _moduleInfo.preinit);

            if(!_settingBoolCollection["singleton"])
            {
                _moduleInfo.preinit = false;
            }

            GUILayout.EndHorizontal();

            if(_settingBoolCollection["singleton"])
            {
                _moduleInfo.custom_manager_name = EditorGUILayout.TextField("Manager's script name:", _moduleInfo.custom_manager_name);
            }

            _settingBoolCollection["hooks"] = EditorGUILayout.Toggle("Hooks:", _settingBoolCollection["hooks"]);
            _settingBoolCollection["store"] = EditorGUILayout.Toggle("Store:", _settingBoolCollection["store"]);

            if (GUILayout.Button("Create module"))
            {
                if (!string.IsNullOrEmpty(_moduleInfo.name))
                {
                    CreateModule();
                    AllModulesWindow.UpdatePreInit();
                }
            }
        }

        void CreateModule()
        {
            string moduleFolder;

            if (_moduleType == MTypeName.System)
            {
                moduleFolder = "System";
            } else
            {
                moduleFolder = "Domain";
            }

            string filePath = "App/Modules/" + moduleFolder + "/" + _moduleInfo.name;
            string[] fileNames = {
        "module-info.json",
        _moduleInfo.name + "Parts.cs",
        _moduleInfo.name + "Manager.cs",
        _moduleInfo.name + "Hooks.cs",
        _moduleInfo.name + "Store.cs",
        };

            if (!Directory.Exists(Path.Combine(Application.dataPath, filePath)))
            {
                // module-info.json
                Directory.CreateDirectory(Path.Combine(Application.dataPath, filePath));
                _moduleInfo.hash = GenerateModuleHash(_moduleInfo.name);

                File.WriteAllText(Path.Combine(Application.dataPath, filePath, fileNames[0]), JsonUtility.ToJson(_moduleInfo));

                if (_settingBoolCollection["singleton"])
                {
                    // Parts
                    string partTmpl = File.ReadAllText(Path.Combine(Application.dataPath, "App/Modules/System/ModulesSystem/templates/Parts.cs.tmpl"));
                    partTmpl = partTmpl.Replace("{0}", _moduleInfo.name);
                    File.WriteAllText(Path.Combine(Application.dataPath, filePath, fileNames[1]), partTmpl);

                    // Manager
                    string managerTmpl = File.ReadAllText(Path.Combine(Application.dataPath, "App/Modules/System/ModulesSystem/templates/Manager.cs.tmpl"));
                    managerTmpl = managerTmpl.Replace("{0}", _moduleInfo.name);
                    File.WriteAllText(Path.Combine(Application.dataPath, filePath, fileNames[2]), managerTmpl);

                }

                if (_settingBoolCollection["hooks"])
                {
                    // Hooks
                    string hooksTmpl = File.ReadAllText(Path.Combine(Application.dataPath, "App/Modules/System/ModulesSystem/templates/Hooks.cs.tmpl"));
                    hooksTmpl = hooksTmpl.Replace("{0}", _moduleInfo.name);
                    File.WriteAllText(Path.Combine(Application.dataPath, filePath, fileNames[3]), hooksTmpl);
                }

                if (_settingBoolCollection["store"])
                {
                    // Store
                    string storeTmpl = File.ReadAllText(Path.Combine(Application.dataPath, "App/Modules/System/ModulesSystem/templates/Store.cs.tmpl"));
                    storeTmpl = storeTmpl.Replace("{0}", _moduleInfo.name);
                    File.WriteAllText(Path.Combine(Application.dataPath, filePath, fileNames[4]), storeTmpl);
                }

                AssetDatabase.Refresh();
            } else
            {
                Debug.LogError("Module with this name is exist.");
            }
        }

        public static string GenerateModuleHash(string moduleName)
        {
            int random = Random.Range(0, int.MaxValue);
            int random2 = Random.Range(0, int.MaxValue);
            string key = random + moduleName + random2;

            return Encode.MD52(key);
        }
    }
}