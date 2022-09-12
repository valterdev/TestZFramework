using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using System.Linq;
using Constants = EditorConstantsForModuleSystem;

namespace ZFramework.Editor
{
    /// <summary>
    /// A window that allows you to create and set meta data for new ZFramework modules (Extension for the Unity editor)
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
            System_Tools,

            Domain_Mechanics_Features = 10,
            Domain_Meta,
            Domain_UI,
            Domain_Other,
            Domain_Tools
        }

        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Private fields (static)
        // ---------------------------------------------------------------------------------------------------------

        private static ModuleInfo _moduleInfo;

        // ---------------------------------------------------------------------------------------------------------
        // Private fields
        // ---------------------------------------------------------------------------------------------------------

        private MTypeName _moduleTypes;
        private MStateName _moduleStates;
        private VisualElement _createModulePage;

        private TextField _moduleName;
        private TextField _moduleVersion;

        private DropdownField _moduleType;
        private DropdownField _moduleState;

        private TextField _moduleSupportSite;
        private TextField _moduleSupportEmail;

        private TextField _moduleDescr;
        private TextField _moduleScriptMname;

        private Dictionary<string, Toggle> _settingBoolToggles = new();

        #endregion

        #region Unity lifecycle & UI

        private void CreateGUI()
        {
            if (_moduleInfo == null)
            {
                InitCollections();
            }

            var visualTreeMainPage = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(Constants.PathToLayoutForCreateModulePage));
            _createModulePage = visualTreeMainPage.Instantiate();
            _createModulePage.StretchToParentSize();

            #region Fill Data
            _moduleName = _createModulePage.Q<TextField>("module_name");
            _moduleVersion = _createModulePage.Q<TextField>("module_version");

            _moduleType = _createModulePage.Q<DropdownField>("module_type");
            
            _moduleType.choices = new() {
                Constants.Domain_Mechanics_Features,
                Constants.Domain_Meta,
                Constants.Domain_UI,
                Constants.Domain_Tools,
                Constants.Domain_Other,

                Constants.System_Core,
                Constants.System_Data,
                Constants.System_Network,
                Constants.System_Tools,
                Constants.System_Other
            };

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

            _createModulePage.BindBtn("btn_create_module", CreateModule);
            
            #endregion

            rootVisualElement.Add(_createModulePage);
        }

        #endregion

        #region App lifecycle

        [MenuItem("Window/ZFramework/Module Editor/Create Module")]
        static void Init()
        {
            if (_moduleInfo == null)
            {
                InitCollections();
            }

            CreateModuleWindow window = (CreateModuleWindow)EditorWindow.GetWindow(typeof(CreateModuleWindow), false, Constants.L_CreateModuleTitle);
            window.Show();
        }

        static void InitCollections()
        {
            _moduleInfo = new ModuleInfo();
            _moduleInfo.description = Constants.Description;
            _moduleInfo.preinit = true;
        }

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods
        // ---------------------------------------------------------------------------------------------------------

        public static string GenerateModuleHash(string moduleName)
        {
            int random  = Random.Range(0, int.MaxValue);
            int random2 = Random.Range(0, int.MaxValue);
            string key  = random + moduleName + random2;

            return Encode.MD52(key);
        }

        // ---------------------------------------------------------------------------------------------------------
        // Private Methods
        // ---------------------------------------------------------------------------------------------------------

        private void CreateModule()
        {
            #region Get data form UI
            if(_moduleName.value == string.Empty)
            {
                Debug.LogError(Constants.L_EnterModuleName);
                return;
            }

            _moduleInfo.name = _moduleName.value;

            if (_moduleVersion.value == string.Empty)
            {
                Debug.LogError(Constants.L_EnterModuleVersion);
                return;
            }

            _moduleInfo.version = _moduleVersion.value;

            if (_moduleType.value == null)
            {
                Debug.LogError(Constants.L_SelectModuleType);
                return;
            }

            _moduleInfo.type = GetModuleTypeIDBySelectString(_moduleType.value);

            if (_moduleState.value == null)
            {
                Debug.LogError(Constants.L_SelectModuleState);
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
            string filePath = Constants.PathToAppModules + moduleFolder + "/" + _moduleInfo.name;
            var managerName = _moduleInfo.custom_manager_name == string.Empty ? _moduleInfo.name + Constants.Manager : _moduleInfo.custom_manager_name;

            string[] fileNames = {
                Constants.ModuleInfoFileName,
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
                    WriteFileBaseOnTemplate(Constants.PathToPartsScriptTemplate, filePath, fileNames[1], managerName);

                    // Manager
                    WriteFileBaseOnTemplate(Constants.PathToManagerScriptTemplate, filePath, fileNames[2], managerName);
                }

                if (_settingBoolToggles["hooks"].value)
                {
                    // Hooks
                    WriteFileBaseOnTemplate(Constants.PathToHooksScriptTemplate, filePath, fileNames[3], _moduleInfo.name);
                }

                if (_settingBoolToggles["store"].value)
                {
                    // Store
                    WriteFileBaseOnTemplate(Constants.PathToStoreScriptTemplate, filePath, fileNames[4], _moduleInfo.name);
                }

                AllModulesWindow.UpdatePreInit(); 
            } else
            {
                Debug.LogError(Constants.L_ModuleWithNameExist);
            }
        }


        private void WriteFileBaseOnTemplate(string tmplPath, string filePath, string fileName, string replaceString)
        {
            string tmpl = File.ReadAllText(Path.Combine(Application.dataPath, tmplPath));
            tmpl = tmpl.Replace("{0}", replaceString);
            File.WriteAllText(Path.Combine(Application.dataPath, filePath, fileName), tmpl);
        }


        private int GetModuleTypeIDBySelectString(string value) =>
            value switch
            {
                Constants.System_Core               => 0,
                Constants.System_Data               => 1,
                Constants.System_Network            => 2,
                Constants.System_Other              => 3,
                Constants.System_Tools              => 4,

                Constants.Domain_Mechanics_Features => 10,
                Constants.Domain_Meta               => 11,
                Constants.Domain_UI                 => 12,
                Constants.Domain_Other              => 13,
                Constants.Domain_Tools              => 14,

                _                                   => -1,
            };

        #endregion
    }
}