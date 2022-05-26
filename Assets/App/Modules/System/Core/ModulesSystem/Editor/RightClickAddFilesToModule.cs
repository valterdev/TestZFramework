using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.IO;
using System.Linq;

namespace ZFramework.Editor
{
    public class RightClickAddFilesToModule : EditorWindow
    {
        private static ProjectInfo _projectInfo;
        private static List<ModuleInfo> _findedModules = new List<ModuleInfo>();

        private static List<string> _filesPaths = new List<string>();
        private VisualElement _mainPage;
        private DropdownField _moduleSelector;

        private static List<string> systemFileNames = new List<string>();

        [MenuItem("Assets/ZFramework/Add Files To Module")]
        private static void FindAllSelectedFiles()
        {
            if (systemFileNames.Count == 0)
            {
                systemFileNames.Add("desktop.ini");
                systemFileNames.Add(".meta");
                systemFileNames.Add(".DS_Store");
                systemFileNames.Add(".Spotlight");
                systemFileNames.Add(".Trashes");
                systemFileNames.Add("ehthumbs.db");
                systemFileNames.Add("Thumbs.db");
            }

            _filesPaths.Clear();

            foreach (var assetGUID in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGUID);

                FileAttributes attr = File.GetAttributes(path);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    DirectoryInfo root = new DirectoryInfo(path);
                    FindAllFilesRecursively(root);
                } else
                {
                    if (!ContainsSystemFileName(path))
                    {
                        _filesPaths.Add(path);
                    }
                }
            }

            RightClickAddFilesToModule window = (RightClickAddFilesToModule)EditorWindow.GetWindow(typeof(RightClickAddFilesToModule), false, "Add files to module");
            window.Show();
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

                if (!ContainsSystemFileName(path))
                {
                    _filesPaths.Add(path);
                }
            }
        }

        private static bool ContainsSystemFileName(string path)
        {
            for (int i = 0; i < systemFileNames.Count; i++)
            {
                if(path.Contains(systemFileNames[i]))
                {
                    return true;
                }
            }

            return false;
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

        private void AddFilesToModule()
        {
            if (_moduleSelector.value == string.Empty)
            {
                Debug.LogError("Select module");
                return;
            }

            var selectedModule = _findedModules.Where(module => module.name == _moduleSelector.value).FirstOrDefault();

            if (selectedModule.content_files != null)
            {
                for (int i = 0; i < _filesPaths.Count; i++)
                {
                    for (int j = 0; j < selectedModule.content_files.Count; j++)
                    {
                        if (_filesPaths[i] == selectedModule.content_files[j])
                        {
                            _filesPaths.RemoveAt(i);
                            i--;
                        }
                    }
                }

                if (_filesPaths.Count > 0)
                {
                    selectedModule.content_files.AddRange(_filesPaths);
                }
            } else
            {
                selectedModule.content_files = new List<string>(_filesPaths);
            }
            
            File.WriteAllText(Path.Combine(Application.dataPath, selectedModule.GetModulePath(), "module-info.json"), JsonUtility.ToJson(selectedModule));
            Debug.Log("Successfull add files!");
        }

        private void CreateGUI()
        {
            if (_findedModules.Count == 0)
            {
                if (_projectInfo == null)
                {
                    LoadProjectManifest();
                }

                DirectoryInfo root = new DirectoryInfo(Path.Combine(Application.dataPath, "App/"));
                FindAllModules(root);
            }

            var visualTreeMainPage = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine("Assets/App/Modules/System/Core/ModulesSystem/view/add_files_to_module.uxml"));
            _mainPage = visualTreeMainPage.Instantiate();
            _mainPage.StretchToParentSize();

            _moduleSelector = _mainPage.Q<DropdownField>("selected_module");
            _moduleSelector.choices = _findedModules.Select(v => v.name).ToList();

            _mainPage.Q<Button>("btn_add_files").clickable.clicked += AddFilesToModule;

            rootVisualElement.Add(_mainPage);
            PopulateListView();
        }

        private void PopulateListView()
        {
            Func<VisualElement> makeItem = () => new Label();

            Action<VisualElement, int> bindItem = (e, i) =>
            {
                var label = (Label)e;
                label.text = _filesPaths[i];
            };
            // ListView listView = rootVisualElement.Q<ListView>(className: "the-uxml-listview");
            var listView = rootVisualElement.Q<ListView>();
            listView.makeItem = makeItem;
            listView.bindItem = bindItem;

            listView.itemsSource = _filesPaths;
            listView.selectionType = SelectionType.Single;

            // Callback invoked when the user double clicks an item
            //listView.onItemsChosen += OnSelectionChanged;
            // Callback invoked when the user changes the selection inside the ListView
            //listView.onSelectionChange += OnSelectionChanged;

        }
    }
}
