using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine.UIElements;
using System.Linq;
using Constants = EditorConstantsForModuleSystem;

namespace ZFramework.Editor
{
    /// <summary>
    /// Collects and displays information about all modules present in the system.
    /// </summary>
    public class AllModulesWindow : EditorWindow
    {
        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Private fields (static)
        // ---------------------------------------------------------------------------------------------------------

        private static ProjectInfo s_projectInfo;
        private static List<ModuleInfo> s_findedModules = new List<ModuleInfo>();
        private static List<string> s_filesToDelete = new List<string>();

        // ---------------------------------------------------------------------------------------------------------
        // Private fields
        // ---------------------------------------------------------------------------------------------------------

        private VisualElement _mainPage;
        private VisualElement _modulePage;

        private bool _activateMode;

        #endregion

        #region Unity lifecycle & UI

        private void CreateGUI()
        {
            _mainPage = LoadUXMLFromPath(Constants.PathToLayoutForAllModulesPage);
            _mainPage.StretchToParentSize();

            _mainPage.SetText("ProjectTitle", s_projectInfo.name);
            rootVisualElement.Add(_mainPage);
            PopulateListView();

            _mainPage.BindBtn("btn_activ", ActivateDeactivateMode);
            _mainPage.BindBtn("btn_import_project", CloseModuleDetails);
            _mainPage.BindBtn("btn_import_module", ImportModule);

            _modulePage = LoadUXMLFromPath(Constants.PathToLayoutForModuleDetailsPage);
            _modulePage.BindBtn("btn_back", CloseModuleDetails);

            rootVisualElement.Add(_modulePage);
            _modulePage.Hide();
        }

        #endregion

        #region App Lifecycle

        [MenuItem("Window/ZFramework/Module Editor/All Modules")]
        static void Init()
        {
            LoadProjectManifest();

            s_findedModules.Clear();
            DirectoryInfo root = new DirectoryInfo(Path.Combine(Application.dataPath, Constants.RootPath));
            FindAllModules(root);
            
            AllModulesWindow window = (AllModulesWindow)EditorWindow.GetWindow(typeof(AllModulesWindow), false, Constants.L_AllModulesPageTitle);
            window.Show();
        }


        [MenuItem("Window/ZFramework/Module Editor/Update PreInit script")]
        public static void UpdatePreInit()
        {
            if (s_findedModules.Count == 0)
            {
                if (s_projectInfo == null)
                {
                    LoadProjectManifest();
                }

                DirectoryInfo root = new DirectoryInfo(Path.Combine(Application.dataPath, Constants.RootPath));
                FindAllModules(root);
            }

            StringBuilder allInits = new StringBuilder();

            foreach (ModuleInfo module in s_findedModules)
            {
                if (module.preinit && module.active)
                {
                    allInits.Append("\t\t\t");
                    if (string.IsNullOrEmpty(module.custom_manager_name))
                    {
                        allInits.Append(module.name);
                        allInits.Append("Manager");
                    } else
                    {
                        allInits.Append(module.custom_manager_name);
                    }

                    allInits.Append(".Instance().PreInit();\n");
                }
            }

            string tmpl = File.ReadAllText(Path.Combine(Application.dataPath, Constants.PathToPreInitTemplate));
            tmpl = tmpl.Replace("{0}", allInits.ToString());
            File.WriteAllText(Path.Combine(Application.dataPath, Constants.PathToPreInitScript), tmpl);

            AssetDatabase.Refresh();
        }

        #endregion

        #region UI

        private void PopulateListView()
        {
            var listItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Constants.PathToLayoutForModuleItem);

            Func<VisualElement> makeItem = () => listItem.Instantiate();

            Action<VisualElement, int> bindItem = (e, i) =>
            {
                if (s_findedModules[i].active)
                {
                    e.RemoveFromClassList("disable_module");
                } else
                {
                    e.AddToClassList("disable_module");
                }

                e.SetText(
                    "mname",
                    $"<b>{i + 1}. {s_findedModules[i].name}</b>\n{s_findedModules[i].description}"
                    );
                e.SetText("mversion", s_findedModules[i].version);
                e.SetText("mtype", s_findedModules[i].type == 0 ? Constants.System : Constants.Domain);
                e.SetText("mstate", GetStateName(s_findedModules[i].state));

                var toggle = e.SetToggle("mactive", s_findedModules[i].active, i);

                toggle.UnregisterCallback<ClickEvent>(ActivateDeactivateCallback);
                toggle.RegisterCallback<ClickEvent>(ActivateDeactivateCallback);
            };
            
            var listView = rootVisualElement.Q<ListView>();
            listView.makeItem = makeItem;
            listView.bindItem = bindItem;

            listView.itemsSource = s_findedModules;
            listView.selectionType = SelectionType.Single;

            // Callback invoked when the user double clicks an item
            listView.onItemsChosen += OnSelectionChanged;
            // Callback invoked when the user changes the selection inside the ListView
            //listView.onSelectionChange += OnSelectionChanged;
            
        }

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Private Methods
        // ---------------------------------------------------------------------------------------------------------

        private VisualElement LoadUXMLFromPath(string path)
        {
#if !UNITY_EDITOR
        throw new NotImplementedException();
#endif
            var visualTreeMainPage = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(path));
            return visualTreeMainPage.Instantiate();
        }


        private static void LoadProjectManifest()
        {
            var path = Path.Combine(Application.dataPath, Constants.PathToProjectManifest);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                s_projectInfo = JsonUtility.FromJson<ProjectInfo>(json);
            }
        }


        private void ImportModule()
        {
            var path = EditorUtility.OpenFilePanel(Constants.L_ImportModuleWindowTitle, "", "unitypackage");

            if (path == String.Empty) return;

            AssetDatabase.ImportPackage(path, true);
        }


        private void ActivateDeactivateCallback(ClickEvent evt)
        {
            var elem = (VisualElement)evt.target;

            if (elem.name == "mactive")
            {
                var curToggle = (Toggle)evt.currentTarget;
                int curModuleID = (int)curToggle.userData;
                ActivateDeactivateModule(curModuleID, curToggle.value);
            }
        }


        private void ActivateDeactivateModule(int curModuleID, bool value)
        {
            s_findedModules[curModuleID].active = value;
            rootVisualElement.Q<ListView>().RefreshItems();
            //var zipFileName = _findedModules[curModuleID].name + "_" + _findedModules[curModuleID].hash + ".zip";
            var packageName = s_findedModules[curModuleID].name + "_" + s_findedModules[curModuleID].hash + ".unitypackage";

            if (value == true)
            {
                AssetDatabase.ImportPackage(Path.Combine(Application.dataPath, Constants.PathToDisabledModules, packageName), false);
                //ExtractZipContent(
                //    Path.Combine(Application.dataPath, Constants.PathToDisabledModules, zipFileName),
                //    Path.Combine(Application.dataPath, _findedModules[curModuleID].GetModulePath())
                //    );

                // update info about disabled modules
                for (int i = 0; i < s_projectInfo.not_active_modules.Count; i++)
                {
                    if (s_projectInfo.not_active_modules[i].hash == s_findedModules[curModuleID].hash)
                    {
                        File.Delete(
                            Path.Combine(
                            Application.dataPath,
                            Constants.PathToAppModules,
                            ModuleInfo.GetModuleCategoryByType(s_findedModules[curModuleID].type),
                            s_projectInfo.not_active_modules[i].name,
                            Constants.ModuleDisableFlagFileName
                            )
                        );

                        s_projectInfo.not_active_modules.RemoveAt(i);
                        File.WriteAllText(Path.Combine(Application.dataPath, Constants.PathToProjectManifest), JsonUtility.ToJson(s_projectInfo));
                    }
                }
            } else
            {
                if (!Directory.Exists(Path.Combine(Application.dataPath, Constants.PathToZFrameworkServiceFolder)))
                {
                    Directory.CreateDirectory(Path.Combine(Application.dataPath, Constants.PathToZFrameworkServiceFolder));
                }

                if (!Directory.Exists(Path.Combine(Application.dataPath, Constants.PathToDisabledModules)))
                {
                    Directory.CreateDirectory(Path.Combine(Application.dataPath, Constants.PathToDisabledModules));
                }

                // we enter information about the deactivated module in the manifest
                var disabledModule = new ModuleDisabledInfo
                {
                    archive_name = packageName,

                    hash = s_findedModules[curModuleID].hash,
                    name = s_findedModules[curModuleID].name,
                    version = s_findedModules[curModuleID].version,

                    type = s_findedModules[curModuleID].type,
                    state = s_findedModules[curModuleID].state,
                    description = s_findedModules[curModuleID].description,
                };
                s_projectInfo.not_active_modules.Add(disabledModule);

                File.WriteAllText(Path.Combine(Application.dataPath, Constants.PathToProjectManifest), JsonUtility.ToJson(s_projectInfo));

                // Create a UnityPackage with the module outside the project assets folder so that it doesn't get into the build
                ExportModuleToUnityPackage(curModuleID,
                    Path.Combine(Constants.Assets, s_findedModules[curModuleID].GetModulePath()),
                    Path.Combine(Application.dataPath, Constants.PathToDisabledModules, packageName));

                // Create an archive with the module outside the project assets folder so that it doesn't get into the build
                //CompressDirectory(
                //Path.Combine(Application.dataPath, _findedModules[curModuleID].GetModulePath()),
                //Path.Combine(Application.dataPath, "../ZFramework/DisabledModules/", zipFileName));

                // Removing all unnecessary
                DirectoryInfo root = new DirectoryInfo(Path.Combine(Application.dataPath, s_findedModules[curModuleID].GetModulePath()));
                FindAllFilesRecursively(root);

                s_filesToDelete.AddRange(s_findedModules[curModuleID].content_files);

                foreach (string file in s_filesToDelete)
                {
                    File.Delete(file);
                }

                File.Create(
                    Path.Combine(
                        Application.dataPath, Constants.PathToAppModules,
                        ModuleInfo.GetModuleCategoryByType(s_findedModules[curModuleID].type),
                        s_findedModules[curModuleID].name,
                        Constants.ModuleDisableFlagFileName
                        )
                );
            }

            UpdatePreInit();
        }


        private static void FindAllFilesRecursively(DirectoryInfo root)
        {
            var subDirs = root.GetDirectories();

            foreach (DirectoryInfo dirInfo in subDirs)
            {
                FindAllFilesRecursively(dirInfo);
            }

            var files = root.GetFiles();

            foreach (FileInfo file in files)
            {
                var filename = file.FullName.Split(Constants.Assets);
                var path = Constants.Assets + filename[1];

                if (!path.Contains(Constants.ModuleInfoFileName))
                {
                    s_filesToDelete.Add(path);
                }
            }
        }


        private void OnSelectionChanged(IEnumerable<object> modules)
        {
            foreach (ModuleInfo module in modules)
            {
                OpenModuleDetails(module);
            }
        }


        private void ActivateDeactivateMode()
        {
            _activateMode = !_activateMode;

            if (_activateMode)
            {
                _mainPage.Query<Toggle>().Class("module_check").ForEach((elem) => {
                    elem.Show();
                });
            } else
            {
                _mainPage.Query<Toggle>().Class("module_check").ForEach((elem) => {
                    elem.Hide();
                });
            }
            
        }


        private void OpenModuleDetails(ModuleInfo module)
        {
            _mainPage.Hide();
            _modulePage.Show();

            //rootVisualElement.Remove(_mainPage);
            //rootVisualElement.Add(_modulePage);

            _modulePage.SetText("critical", Constants.L_CriticalForWork + (module.critical ? Constants.L_Yes : Constants.L_No));
            _modulePage.SetText("title", module.name);
            _modulePage.SetText("path", Constants.L_Path + Path.Combine(Constants.Assets, module.GetModulePath()));
            _modulePage.SetText("version", Constants.L_Version + module.version);

            _modulePage.SetText("type", Constants.L_Type + (module.type == 0 ? Constants.System : Constants.Domain));
            _modulePage.SetText("state", Constants.L_State + GetStateName(module.state));

            _modulePage.SetText("author", string.Empty);

            if (module.author != string.Empty)
            {
                _modulePage.AddText("author", Constants.L_Author + module.author);

                if (module.site != string.Empty || module.email != string.Empty)
                {
                    _modulePage.AddText("author", "\n(");
                }

                if (module.site != string.Empty)
                {
                    _modulePage.AddText("author", Constants.L_Email + module.email);
                }

                if (module.site != string.Empty)
                {
                    _modulePage.AddText("author", Constants.L_Site + module.site);
                }

                if (module.site != string.Empty || module.email != string.Empty)
                {
                    _modulePage.AddText("author", " )");
                }
            }

            _modulePage.SetText("descr", module.description);
        }


        private void CloseModuleDetails()
        {
            _mainPage.Show();
            _modulePage.Hide();
        }


        private string GetStateName(int state) =>
            state switch
            {
                0  => Constants.NotCompleted,
                10 => Constants.Alpha,
                20 => Constants.Beta,
                30 => Constants.Stable,
                90 => Constants.Migrated,
                _  => throw new ArgumentException("Invalid enum value for state", nameof(state)),
            };


        /// <summary>
        /// Recursively walk through all directories where there is a module-info.json file and is a module
        /// </summary>
        /// <param name="root">Path to folder Assets/App, which is the root directory for the entire application</param>
        private static void FindAllModules(DirectoryInfo root)
        {
            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            try
            {
                files = root.GetFiles(Constants.ModuleInfoFileName);
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.Log(e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Debug.Log(e.Message);
            }

            if (files.Length > 0)
            {
                string json = File.ReadAllText(files[0].FullName);
                ModuleInfo moduleInfo = JsonUtility.FromJson<ModuleInfo>(json);
                moduleInfo.active = true;
                s_findedModules.Add(moduleInfo);
            }

            subDirs = root.GetDirectories();

            foreach (DirectoryInfo dirInfo in subDirs)
            {
                FindAllModules(dirInfo);
            }

            s_findedModules = s_findedModules.OrderBy(x => x.type).ToList();


            s_findedModules.ForEach(module => {
                s_projectInfo.not_active_modules.ForEach(
                    (elem) => {
                        if (elem.hash == module.hash)
                        {
                            module.active = false;
                        }
                    }
                );
            });   
        }


        private static void ExportModuleToUnityPackage(int curModuleID, string directoryPath, string outputFilePath)
        {
            if (!Directory.Exists(Path.Combine(Application.dataPath, Constants.PathToZFrameworkServiceFolder)))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, Constants.PathToZFrameworkServiceFolder));
            }

            if (!Directory.Exists(Path.Combine(Application.dataPath, Constants.PathToDisabledModules)))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, Constants.PathToDisabledModules));
            }

            var exportedPackageAssetList = new List<string>();

            exportedPackageAssetList.Add(directoryPath);

            if (s_findedModules[curModuleID].content_files?.Count > 0)
            {
                exportedPackageAssetList.AddRange(s_findedModules[curModuleID].content_files);
            }
            
            AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), outputFilePath,
                ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
        }

        #endregion
    }
}
