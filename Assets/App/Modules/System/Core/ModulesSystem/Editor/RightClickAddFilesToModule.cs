using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.IO;
using System.Linq;
using Constants = EditorConstantsForModuleSystem;

namespace ZFramework.Editor
{
    public class RightClickAddFilesToModule : EditorWindow
    {
        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Private fields (static)
        // ---------------------------------------------------------------------------------------------------------

        private static ProjectInfo _projectInfo;
        private static List<ModuleInfo> _findedModules = new List<ModuleInfo>();

        private static List<string> _filesPaths = new List<string>();
        private static List<string> _systemFileNames = new List<string>();

        // ---------------------------------------------------------------------------------------------------------
        // Private fields
        // ---------------------------------------------------------------------------------------------------------

        private VisualElement _mainPage;
        private DropdownField _moduleSelector;

        #endregion

        #region App lifecycle

        [MenuItem("Assets/ZFramework/Add Files To Module")]
        private static void FindAllSelectedFiles()
        {
            if (_systemFileNames.Count == 0)
            {
                _systemFileNames.Add("desktop.ini");
                _systemFileNames.Add(".meta");
                _systemFileNames.Add(".DS_Store");
                _systemFileNames.Add(".Spotlight");
                _systemFileNames.Add(".Trashes");
                _systemFileNames.Add("ehthumbs.db");
                _systemFileNames.Add("Thumbs.db");
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

            RightClickAddFilesToModule window = (RightClickAddFilesToModule)EditorWindow.GetWindow(typeof(RightClickAddFilesToModule), false, Constants.L_AddFilesToModulePageTitle);
            window.Show();
        }

        #endregion

        #region Unity lifecycle & UI

        private void CreateGUI()
        {
            if (_findedModules.Count == 0)
            {
                if (_projectInfo == null)
                {
                    LoadProjectManifest();
                }

                DirectoryInfo root = new DirectoryInfo(Path.Combine(Application.dataPath, Constants.RootPath));
                FindAllModules(root);
            }

            var visualTreeMainPage = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(Constants.PathToLayoutForAddFilesToModulePage));
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
            
            var listView = rootVisualElement.Q<ListView>();
            listView.makeItem = makeItem;
            listView.bindItem = bindItem;

            listView.itemsSource = _filesPaths;
            listView.selectionType = SelectionType.Single;

        }

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Private Methods (static)
        // ---------------------------------------------------------------------------------------------------------

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

                if (!ContainsSystemFileName(path))
                {
                    _filesPaths.Add(path);
                }
            }
        }


        private static bool ContainsSystemFileName(string path)
        {
            for (int i = 0; i < _systemFileNames.Count; i++)
            {
                if(path.Contains(_systemFileNames[i]))
                {
                    return true;
                }
            }

            return false;
        }


        private static void LoadProjectManifest()
        {
            var path = Path.Combine(Application.dataPath, Constants.PathToProjectManifest);

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

        // ---------------------------------------------------------------------------------------------------------
        // Private Methods
        // ---------------------------------------------------------------------------------------------------------

        private void AddFilesToModule()
        {
            if (_moduleSelector.value == string.Empty)
            {
                Debug.LogError(Constants.L_SelectModule);
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
            
            File.WriteAllText(Path.Combine(Application.dataPath, selectedModule.GetModulePath(), Constants.ModuleInfoFileName), JsonUtility.ToJson(selectedModule));
            Debug.Log(Constants.L_SuccessAddFiles);
        }

        #endregion
    }
}
