using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using System.Linq;

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
            System_Core = 0,
            System_Data,
            System_Network,
            System_Other,

            Domain_Mechanics_Features = 10,
            Domain_Meta,
            Domain_UI,
            Domain_Other
        }

        private MTypeName _moduleTypes;
        private MStateName _moduleStates;
        private static ModuleInfo _moduleInfo;
        private VisualElement _createModulePage;

        private TextField _moduleName;
        private TextField _moduleVersion;

        private DropdownField _moduleType;
        private DropdownField _moduleState;

        private TextField _moduleSupportSite;
        private TextField _moduleSupportEmail;

        private TextField _moduleDescr;
        private TextField _moduleScriptMname;

        private Dictionary<string, Toggle> _settingBoolToggles = new Dictionary<string, Toggle>();

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
        }

        private void CreateGUI()
        {
            if (_moduleInfo == null)
            {
                InitCollections();
            }

            var visualTreeMainPage = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine("Assets/App/Modules/System/Core/ModulesSystem/view/create_module.uxml"));
            _createModulePage = visualTreeMainPage.Instantiate();
            _createModulePage.StretchToParentSize();

            #region Fill Data
            _moduleName = _createModulePage.Q<TextField>("module_name");
            _moduleVersion = _createModulePage.Q<TextField>("module_version");

            _moduleType = _createModulePage.Q<DropdownField>("module_type");
            //_moduleType.choices = System.Enum.GetValues(typeof(MTypeName)).Cast<MTypeName>().Select(v => v.ToString()).ToList();
            _moduleType.choices = new List<string>();
            _moduleType.choices.Add("Domain/Mechanics_Features");
            _moduleType.choices.Add("Domain/Meta");
            _moduleType.choices.Add("Domain/UI");
            _moduleType.choices.Add("Domain/Other");

            _moduleType.choices.Add("System/Core");
            _moduleType.choices.Add("System/Data");
            _moduleType.choices.Add("System/Network");
            _moduleType.choices.Add("System/Other");

            _moduleState = _createModulePage.Q<DropdownField>("module_state");
            _moduleState.choices = System.Enum.GetValues(typeof(MStateName)).Cast<MStateName>().Select(v => v.ToString()).ToList();

            _moduleSupportSite = _createModulePage.Q<TextField>("module_site");
            _moduleSupportEmail = _createModulePage.Q<TextField>("module_email");

            _moduleDescr = _createModulePage.Q<TextField>("module_descr");
            _moduleScriptMname = _createModulePage.Q<TextField>("module_script_mname");

            _settingBoolToggles.Add("singleton", _createModulePage.Q<Toggle>("module_singleton"));
            _settingBoolToggles.Add("preinit", _createModulePage.Q<Toggle>("module_preinit"));

            _settingBoolToggles.Add("hooks", _createModulePage.Q<Toggle>("module_hooks"));
            _settingBoolToggles.Add("store", _createModulePage.Q<Toggle>("module_store"));

            _settingBoolToggles["singleton"].RegisterValueChangedCallback(x => {
                if (x.newValue == false) {
                    _settingBoolToggles["preinit"].value = false;
                }
            });

            _createModulePage.Q<Button>("btn_create_module").clickable.clicked += CreateModule;
            
            #endregion

            rootVisualElement.Add(_createModulePage);
        }

        private void CreateModule()
        {
            #region Get data form UI
            if(_moduleName.value == string.Empty)
            {
                Debug.LogError("Enter module name");
                return;
            }

            _moduleInfo.name = _moduleName.value;

            if (_moduleVersion.value == string.Empty)
            {
                Debug.LogError("Enter module version");
                return;
            }

            _moduleInfo.version = _moduleVersion.value;

            if (_moduleType.value == null)
            {
                Debug.LogError("Select module type");
                return;
            }

            _moduleInfo.type = GetModuleTypeIDBySelectString(_moduleType.value);

            if (_moduleState.value == null)
            {
                Debug.LogError("Select module state");
                return;
            }

            System.Enum.TryParse(_moduleState.value, out MStateName curState);
            _moduleInfo.state = (int)curState;

            _moduleInfo.author = _moduleVersion.value;
            _moduleInfo.site = _moduleSupportSite.value;
            _moduleInfo.email = _moduleSupportEmail.value;
            _moduleInfo.description = _moduleDescr.value;
            _moduleInfo.custom_manager_name = _moduleScriptMname.value;

            if(!_settingBoolToggles["singleton"].value)
            {
                _moduleInfo.preinit = false;
            } else
            {
                _moduleInfo.preinit = _settingBoolToggles["preinit"].value;
            }
            
            #endregion

            string moduleFolder = _moduleType.value;
            string filePath = "App/Modules/" + moduleFolder + "/" + _moduleInfo.name;

            var managerName = _moduleInfo.custom_manager_name == string.Empty ? _moduleInfo.name + "Manager" : _moduleInfo.custom_manager_name;


            string[] fileNames = {
        "module-info.json",
        _moduleInfo.name + "Parts.cs",
        managerName + ".cs",
        _moduleInfo.name + "Hooks.cs",
        _moduleInfo.name + "Store.cs",
        };

            if (!Directory.Exists(Path.Combine(Application.dataPath, filePath)))
            {
                // module-info.json
                Directory.CreateDirectory(Path.Combine(Application.dataPath, filePath));
                _moduleInfo.hash = GenerateModuleHash(_moduleInfo.name);

                File.WriteAllText(Path.Combine(Application.dataPath, filePath, fileNames[0]), JsonUtility.ToJson(_moduleInfo));

                if (_settingBoolToggles["singleton"].value)
                {
                    // Parts
                    string partTmpl = File.ReadAllText(Path.Combine(Application.dataPath, "App/Modules/System/Core/ModulesSystem/templates/Parts.cs.tmpl"));
                    partTmpl = partTmpl.Replace("{0}", managerName);
                    File.WriteAllText(Path.Combine(Application.dataPath, filePath, fileNames[1]), partTmpl);

                    // Manager
                    string managerTmpl = File.ReadAllText(Path.Combine(Application.dataPath, "App/Modules/System/Core/ModulesSystem/templates/Manager.cs.tmpl"));
                    managerTmpl = managerTmpl.Replace("{0}", managerName);
                    File.WriteAllText(Path.Combine(Application.dataPath, filePath, fileNames[2]), managerTmpl);

                }

                if (_settingBoolToggles["hooks"].value)
                {
                    // Hooks
                    string hooksTmpl = File.ReadAllText(Path.Combine(Application.dataPath, "App/Modules/System/Core/ModulesSystem/templates/Hooks.cs.tmpl"));
                    hooksTmpl = hooksTmpl.Replace("{0}", _moduleInfo.name);
                    File.WriteAllText(Path.Combine(Application.dataPath, filePath, fileNames[3]), hooksTmpl);
                }

                if (_settingBoolToggles["store"].value)
                {
                    // Store
                    string storeTmpl = File.ReadAllText(Path.Combine(Application.dataPath, "App/Modules/System/Core/ModulesSystem/templates/Store.cs.tmpl"));
                    storeTmpl = storeTmpl.Replace("{0}", _moduleInfo.name);
                    File.WriteAllText(Path.Combine(Application.dataPath, filePath, fileNames[4]), storeTmpl);
                }

                AllModulesWindow.UpdatePreInit();
                //AssetDatabase.Refresh();
               
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

        public int GetModuleTypeIDBySelectString(string _value)
        {
            switch(_value)
            {
            case "System/Core":
                return 0;

            case "System/Data":
                return 1;

            case "System/Network":
                return 2;

            case "System/Other":
                return 3;

            case "Domain/Mechanics_Features":
                return 10;

            case "Domain/Meta":
                return 11;

            case "Domain/UI":
                return 12;

            case "Domain/Other":
                return 13;

            default: return -1;
            }
        }
    }
}