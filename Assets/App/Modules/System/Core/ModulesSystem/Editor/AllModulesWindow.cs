using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using System.Linq;
using ICSharpCode.SharpZipLib.Core;

namespace ZFramework.Editor
{
    /// <summary>
    /// Собирает и показывает информацию о всех модулях присутствующих в системе.
    /// </summary>
    public class AllModulesWindow : EditorWindow
    {
        private static ProjectInfo _projectInfo;
        private static List<ModuleInfo> _findedModules = new List<ModuleInfo>();
        private static List<string> _filesToDelete = new List<string>();

        private VisualElement _mainPage;
        private VisualElement _modulePage;

        private bool _activateMode;

        [MenuItem("Window/ZFramework/Module Editor/All Modules")]
        static void Init()
        {
            LoadProjectManifest();

            _findedModules.Clear();
            DirectoryInfo root = new DirectoryInfo(Path.Combine(Application.dataPath, "App/"));
            FindAllModules(root);
            //UpdatePreInit();
            AllModulesWindow window = (AllModulesWindow)EditorWindow.GetWindow(typeof(AllModulesWindow), false, "All modules");
            window.Show();
        }

        private static void LoadProjectManifest()
        {
            var path = Path.Combine(Application.dataPath, "App/ZProjectManifest.json");

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                _projectInfo = JsonUtility.FromJson<ProjectInfo>(json);
            }
        }

        private void CreateGUI()
        {
            var visualTreeMainPage = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine("Assets/App/Modules/System/Core/ModulesSystem/view/all_modules.uxml"));
            _mainPage = visualTreeMainPage.Instantiate();
            _mainPage.StretchToParentSize();

            _mainPage.Q<Label>("ProjectTitle").text = _projectInfo.name;
            rootVisualElement.Add(_mainPage);
            PopulateListView();

            //rootVisualElement.Query<Toggle>().Class("module_check").ForEach((elem) => {
            //    Debug.Log(elem);
            //    elem.RegisterCallback<ClickEvent>((evt) =>
            //    {
            //        var elem = (VisualElement)evt.target;

            //        if (elem.name == "mactive")
            //        {
            //            var curToggle = (Toggle)evt.currentTarget;
            //            int curModuleID = (int)curToggle.userData;
            //            ActivateDeactivateModule(curModuleID, !curToggle.value);
            //        }
            //    });
            //});

            _mainPage.Q<Button>("btn_activ").clickable.clicked += ActivateDeactivateMode;
            _mainPage.Q<Button>("btn_import_project").clickable.clicked += CloseModuleDetails;
            _mainPage.Q<Button>("btn_import_module").clickable.clicked += ImportModule;

            var visualTreeModuleDetails = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine("Assets/App/Modules/System/Core/ModulesSystem/view/module_details.uxml"));
            _modulePage = visualTreeModuleDetails.Instantiate();
            _modulePage.Q<Button>("btn_back").clickable.clicked += CloseModuleDetails;

            rootVisualElement.Add(_modulePage);
            _modulePage.style.display = DisplayStyle.None;
        }

        private void PopulateListView()
        {
            var listItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/App/Modules/System/Core/ModulesSystem/view/module_item.uxml");

            Func<VisualElement> makeItem = () => listItem.Instantiate();

            Action<VisualElement, int> bindItem = (e, i) =>
            {
                if (_findedModules[i].active)
                {
                    e.RemoveFromClassList("disable_module");
                } else
                {
                    e.AddToClassList("disable_module");
                }

                e.Q<Label>("mname").text = "<b>" + (i + 1).ToString() + ". " + _findedModules[i].name + "</b>\n" + _findedModules[i].description;
                e.Q<Label>("mversion").text = _findedModules[i].version;
                e.Q<Label>("mtype").text = _findedModules[i].type == 0 ? "System" : "Domain";
                e.Q<Label>("mstate").text = GetStateName(_findedModules[i].state);

                var toggle = e.Q<Toggle>("mactive");
                toggle.value = _findedModules[i].active;
                toggle.userData = i;

                toggle.UnregisterCallback<ClickEvent>(ActivateDeactivateCallback);
                toggle.RegisterCallback<ClickEvent>(ActivateDeactivateCallback);
                //if (i < _findedModules.Count)
                //{
                //    e.RemoveFromClassList("disable_module");
                //    e.Q<Label>("mname").text = "<b>" + (i + 1).ToString() + ". " + _findedModules[i].name + "</b>\n" + _findedModules[i].description;
                //    e.Q<Label>("mversion").text = _findedModules[i].version;
                //    e.Q<Label>("mtype").text = _findedModules[i].type == 0 ? "System" : "Domain";
                //    e.Q<Label>("mstate").text = GetStateName(_findedModules[i].state);

                //    var toggle = e.Q<Toggle>("mactive");
                //    toggle.value = _findedModules[i].active;
                //    toggle.userData = i;

                //    toggle.UnregisterCallback<ClickEvent>(ActivateDeactivateCallback);
                //    toggle.RegisterCallback<ClickEvent>(ActivateDeactivateCallback);
                //} else
                //{
                //    var current = i - _findedModules.Count;
                //    e.AddToClassList("disable_module");
                //    e.Q<Label>("mname").text = "<b>" + (i + 1).ToString() + ". " + _projectInfo.not_active_modules[current].name + "</b>\n" + _projectInfo.not_active_modules[current].description;
                //    e.Q<Label>("mversion").text = _projectInfo.not_active_modules[current].version;
                //    e.Q<Label>("mtype").text = _projectInfo.not_active_modules[current].type == 0 ? "System" : "Domain";
                //    e.Q<Label>("mstate").text = GetStateName(_projectInfo.not_active_modules[current].state);

                //    var toggle = e.Q<Toggle>("mactive");
                //    toggle.value = false;
                //    toggle.userData = i;

                //    toggle.UnregisterCallback<ClickEvent>(ActivateDeactivateCallback);
                //    toggle.RegisterCallback<ClickEvent>(ActivateDeactivateCallback);
                //} 
            };
            // ListView listView = rootVisualElement.Q<ListView>(className: "the-uxml-listview");
            var listView = rootVisualElement.Q<ListView>();
            listView.makeItem = makeItem;
            listView.bindItem = bindItem;

            listView.itemsSource = _findedModules;
            listView.selectionType = SelectionType.Single;

            // Callback invoked when the user double clicks an item
            listView.onItemsChosen += OnSelectionChanged;
            // Callback invoked when the user changes the selection inside the ListView
            //listView.onSelectionChange += OnSelectionChanged;
            
        }

        private void ImportModule()
        {
            var path = EditorUtility.OpenFilePanel("Select module's packages", "", "unitypackage");
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
            _findedModules[curModuleID].active = value;
            rootVisualElement.Q<ListView>().RefreshItems();//.Rebuild();
            //var zipFileName = _findedModules[curModuleID].name + "_" + _findedModules[curModuleID].hash + ".zip";
            var packageName = _findedModules[curModuleID].name + "_" + _findedModules[curModuleID].hash + ".unitypackage";

            if (value == true)
            {
                AssetDatabase.ImportPackage(Path.Combine(Application.dataPath, "../ZFramework/DisabledModules/", packageName), false);
                //ExtractZipContent(
                //    Path.Combine(Application.dataPath, "../ZFramework/DisabledModules/", zipFileName),
                //    Path.Combine(Application.dataPath, _findedModules[curModuleID].GetModulePath())
                //    );

                // обновляем информацию о выключенных модулях
                for (int i = 0; i < _projectInfo.not_active_modules.Count; i++)
                {
                    if (_projectInfo.not_active_modules[i].hash == _findedModules[curModuleID].hash)
                    {
                        File.Delete(
                            Path.Combine(
                            Application.dataPath,
                            "App/Modules/",
                            ModuleInfo.GetModuleCategoryByType(_findedModules[curModuleID].type),
                            _projectInfo.not_active_modules[i].name,
                            "module_disabled"
                            )
                        );

                        _projectInfo.not_active_modules.RemoveAt(i);
                        File.WriteAllText(Path.Combine(Application.dataPath, "App/ZProjectManifest.json"), JsonUtility.ToJson(_projectInfo));
                    }
                }
            } else
            {
                if (!Directory.Exists(Path.Combine(Application.dataPath, "../ZFramework")))
                {
                    Directory.CreateDirectory(Path.Combine(Application.dataPath, "../ZFramework"));
                }

                if (!Directory.Exists(Path.Combine(Application.dataPath, "../ZFramework/DisabledModules")))
                {
                    Directory.CreateDirectory(Path.Combine(Application.dataPath, "../ZFramework/DisabledModules"));
                }

                // заносим информацию о деактивированном модуле в манифест
                var disabledModule = new ModuleDisabledInfo
                {
                    archive_name = packageName,

                    hash = _findedModules[curModuleID].hash,
                    name = _findedModules[curModuleID].name,
                    version = _findedModules[curModuleID].version,

                    type = _findedModules[curModuleID].type,
                    state = _findedModules[curModuleID].state,
                    description = _findedModules[curModuleID].description,
                };
                _projectInfo.not_active_modules.Add(disabledModule);

                File.WriteAllText(Path.Combine(Application.dataPath, "App/ZProjectManifest.json"), JsonUtility.ToJson(_projectInfo));

                // создаем UnityPackage с модулем вне папки с ассетами проекта, чтобы он не попадал в билд
                
                
                ExportModuleToUnityPackage(curModuleID,
                    Path.Combine("Assets", _findedModules[curModuleID].GetModulePath()),
                    Path.Combine(Application.dataPath, "../ZFramework/DisabledModules/", packageName));

                // создаем архив с модулем вне папки с ассетами проекта, чтобы он не попадал в билд
                //CompressDirectory(
                //Path.Combine(Application.dataPath, _findedModules[curModuleID].GetModulePath()),
                //Path.Combine(Application.dataPath, "../ZFramework/DisabledModules/", zipFileName));

                // Удаляем все лишнее
                DirectoryInfo root = new DirectoryInfo(Path.Combine(Application.dataPath, _findedModules[curModuleID].GetModulePath()));
                FindAllFilesRecursively(root);

                _filesToDelete.AddRange(_findedModules[curModuleID].content_files);

                foreach (string file in _filesToDelete)
                {
                    File.Delete(file);
                }

                File.Create(
                    Path.Combine(
                        Application.dataPath, "App/Modules/",
                        ModuleInfo.GetModuleCategoryByType(_findedModules[curModuleID].type),
                        _findedModules[curModuleID].name,
                        "module_disabled"
                        )
                );
            }

            UpdatePreInit();

            //AssetDatabase.Refresh();
        }

        static void FindAllFilesRecursively(DirectoryInfo root)
        {
            var subDirs = root.GetDirectories();

            foreach (DirectoryInfo dirInfo in subDirs)
            {
                FindAllFilesRecursively(dirInfo);
            }

            var files = root.GetFiles();

            foreach (FileInfo file in files)
            {
                var filename = file.FullName.Split("Assets");
                var path = "Assets" + filename[1];

                if (!path.Contains("module-info.json"))
                {
                    _filesToDelete.Add(path);
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
                    elem.style.display = DisplayStyle.Flex;
                });
            } else
            {
                _mainPage.Query<Toggle>().Class("module_check").ForEach((elem) => {
                    elem.style.display = DisplayStyle.None;
                });
            }
            
        }

        private void OpenModuleDetails(ModuleInfo module)
        {
            _mainPage.style.display = DisplayStyle.None;
            _modulePage.style.display = DisplayStyle.Flex;

            //rootVisualElement.Remove(_mainPage);
            //rootVisualElement.Add(_modulePage);

            _modulePage.Q<Label>("critical").text = "Критичность для работы системы: " + (module.critical ? "<color=#ff0000><b>Да<b></color>" : "<color=#00ff00>Нет</color>");

            _modulePage.Q<Label>("title").text = module.name;
            _modulePage.Q<Label>("path").text = "<b>Path:</b> Assets/App/Modules/" + (module.type == 0 ? "System" : "Domain") + "/" + module.name + "/";
            _modulePage.Q<Label>("version").text = "<b>Version:</b> " + module.version;

            _modulePage.Q<Label>("type").text = "<b>Type:</b> " + (module.type == 0 ? "System" : "Domain");
            _modulePage.Q<Label>("state").text = "<b>State:</b> " + GetStateName(module.state);

            _modulePage.Q<Label>("author").text = string.Empty;

            if (module.author != string.Empty)
            {
                _modulePage.Q<Label>("author").text += "<b>Author:</b> " + module.author;

                if (module.site != string.Empty || module.email != string.Empty)
                {
                    _modulePage.Q<Label>("author").text += "\n(";
                }

                if (module.site != string.Empty)
                {
                    _modulePage.Q<Label>("author").text += " Email: " + module.email;
                }

                if (module.site != string.Empty)
                {
                    _modulePage.Q<Label>("author").text += " Site: " + module.site;
                }

                if (module.site != string.Empty || module.email != string.Empty)
                {
                    _modulePage.Q<Label>("author").text += " )";
                }
            }

            _modulePage.Q<Label>("descr").text = module.description;
        }

        private void CloseModuleDetails()
        {
            //rootVisualElement.Remove(_modulePage);
            //rootVisualElement.Add(_mainPage);
            _mainPage.style.display = DisplayStyle.Flex;
            _modulePage.style.display = DisplayStyle.None;
        }

        private string GetStateName(int state)
        {
            switch(state)
            {
            case 10:
                return "Alpha";

            case 20:
                return "Beta";

            case 30:
                return "Stable";

            case 90:
                return "Migrated";

            default: // 0
                return "NotCompleted";
            }
        }

        //private void OnStationViewSized(GeometryChangedEvent ge)
        //{
        //    Debug.Log(((VisualElement)ge.target).resolvedStyle.height);
        //}

        /// <summary>
        /// Рекурсивный обход по всем директориям,
        /// директория, где есть файл module-info.json и является модулем
        /// </summary>
        /// <param name="root">Путь до папки Assets/App, которая является корневой директории для всего приложения</param>
        private static void FindAllModules(DirectoryInfo root)
        {
            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            try
            {
                files = root.GetFiles("module-info.json");
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
                _findedModules.Add(moduleInfo);
            }

            subDirs = root.GetDirectories();

            foreach (DirectoryInfo dirInfo in subDirs)
            {
                FindAllModules(dirInfo);
            }

            _findedModules = _findedModules.OrderBy(x => x.type).ToList();



            _findedModules.ForEach(module => {
                _projectInfo.not_active_modules.ForEach(
                    (elem) => {
                        if (elem.hash == module.hash)
                        {
                            module.active = false;
                        }
                    }
                );
            });   
        }

        [MenuItem("Window/ZFramework/Module Editor/Update PreInit script")]
        public static void UpdatePreInit()
        {
            if (_findedModules.Count == 0)
            {
                if(_projectInfo == null)
                {
                    LoadProjectManifest();
                }

                DirectoryInfo root = new DirectoryInfo(Path.Combine(Application.dataPath, "App/"));
                FindAllModules(root);
            }

            StringBuilder allInits = new StringBuilder();

            foreach (ModuleInfo module in _findedModules)
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

            string filePath = "App/Modules/System/00_InitializationCycle/Z0_PreInit.cs";

            string tmpl = File.ReadAllText(Path.Combine(Application.dataPath, "App/Modules/System/Core/ModulesSystem/templates/PreInit.cs.tmpl"));
            tmpl = tmpl.Replace("{0}", allInits.ToString());
            File.WriteAllText(Path.Combine(Application.dataPath, filePath), tmpl);

            AssetDatabase.Refresh();
        }

        //private void ExportPackage()
        //{
        //    var exportedPackageAssetList = new List<string>();
        //    //Find all shaders that have "Surface" in their names and add them to the list
        //    foreach (var guid in AssetDatabase.FindAssets("t:Shader Surface", new[] { "Assets/Shaders" }))
        //    {
        //        var path = AssetDatabase.GUIDToAssetPath(guid);
        //        exportedPackageAssetList.Add(path);
        //    }

        //    //Add Prefabs folder into the asset list
        //    exportedPackageAssetList.Add("Assets/Prefabs");
        //    //Export Shaders and Prefabs with their dependencies into a .unitypackage
        //    AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), "ShadersAndPrefabsWithDependencies.unitypackage",
        //        ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);

        //    //AssetDatabase.ImportPackage
        //}

        private static void ExportModuleToUnityPackage(int curModuleID, string directoryPath, string outputFilePath)
        {
            if (!Directory.Exists(Path.Combine(Application.dataPath, "../ZFramework")))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "../ZFramework"));
            }

            if (!Directory.Exists(Path.Combine(Application.dataPath, "../ZFramework/DisabledModules")))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "../ZFramework/DisabledModules"));
            }

            var exportedPackageAssetList = new List<string>();

            exportedPackageAssetList.Add(directoryPath);

            if (_findedModules[curModuleID].content_files?.Count > 0)
            {
                exportedPackageAssetList.AddRange(_findedModules[curModuleID].content_files);
            }
            
            AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), outputFilePath,
                ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);

            //AssetDatabase.ImportPackage
        }

        private void CompressDirectory(string directoryPath, string outputFilePath, int compressionLevel = 9)
        {
            try
            {
                string[] filenames = Directory.GetFiles(directoryPath);

                using (ZipOutputStream OutputStream = new ZipOutputStream(File.Create(outputFilePath)))
                {
                    // 0 - store only to 9 - means best compression
                    OutputStream.SetLevel(compressionLevel);

                    byte[] buffer = new byte[4096];

                    foreach (string file in filenames)
                    {
                        ZipEntry entry = new ZipEntry(Path.GetFileName(file));

                        // Setup the entry data as required.
                        entry.DateTime = DateTime.Now;
                        OutputStream.PutNextEntry(entry);

                        using (FileStream fs = File.OpenRead(file))
                        {
                            int sourceBytes;

                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                OutputStream.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                        }
                    }

                    OutputStream.Finish();
                    OutputStream.Close();

                    Debug.Log("Files successfully compressed");
                }
            }
            catch (Exception ex)
            {
                Debug.Log(string.Format("Exception during processing {0}", ex));
            }
        }

        /// <summary>
        /// Extracts the content from a .zip file inside an specific folder.
        /// </summary>
        /// <param name="FileZipPath"></param>
        /// <param name="OutputFolder"></param>
        public void ExtractZipContent(string FileZipPath, string OutputFolder)// <param name="password"></param>
        {
            ZipFile file = null;
            try
            {
                FileStream fs = File.OpenRead(FileZipPath);
                file = new ZipFile(fs);

                //if (!String.IsNullOrEmpty(password))
                //{
                //    // AES encrypted entries are handled automatically
                //    file.Password = password;
                //}

                foreach (ZipEntry zipEntry in file)
                {
                    if (!zipEntry.IsFile)
                    {
                        // Ignore directories
                        continue;
                    }

                    string entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    // 4K is optimum
                    byte[] buffer = new byte[4096];
                    Stream zipStream = file.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    string fullZipToPath = Path.Combine(OutputFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);

                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (file != null)
                {
                    file.IsStreamOwner = true; // Makes close also shut the underlying stream
                    file.Close(); // Ensure we release resources
                }
            }
        }
    }
}
