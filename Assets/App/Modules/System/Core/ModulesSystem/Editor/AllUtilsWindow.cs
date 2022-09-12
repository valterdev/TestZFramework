using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using System;
using Constants = EditorConstantsForModuleSystem;

namespace ZFramework.Editor
{
    public class AllUtilsWindow : EditorWindow
    {
        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Private fields (static)
        // ---------------------------------------------------------------------------------------------------------

        private static ProjectInfo _projectInfo;

        // ---------------------------------------------------------------------------------------------------------
        // Private fields
        // ---------------------------------------------------------------------------------------------------------

        private VisualElement _mainPage;

        #endregion

        #region App lifecycle

        [MenuItem("Window/ZFramework/Module Editor/All Utilities")]
        static void Init()
        {
            LoadProjectManifest();
            
            AllUtilsWindow window = (AllUtilsWindow)EditorWindow.GetWindow(typeof(AllUtilsWindow), false, Constants.L_AllUtilsPageTitle);
            window.Show();
        }

        #endregion

        #region Unity lifecycle & UI

        private void CreateGUI()
        {
            var visualTreeMainPage = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(Constants.PathToLayoutForAllUtilsPage));
            _mainPage = visualTreeMainPage.Instantiate();
            _mainPage.StretchToParentSize();

            rootVisualElement.Add(_mainPage);
            PopulateListView();
        }

        #endregion

        #region UI

        private void PopulateListView()
        {
            var listItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Constants.PathToLayoutForUtilityItem);

            Func<VisualElement> makeItem = () => listItem.Instantiate();

            Action<VisualElement, int> bindItem = (e, i) =>
            {
                if (_projectInfo.utils[i].is_editor)
                {
                    e.Q<VisualElement>("editor_label").style.display = DisplayStyle.Flex;
                } else
                {
                    e.Q<VisualElement>("editor_label").style.display = DisplayStyle.None;
                }

                e.Q<Label>("util_name").text = "<b>" + (i + 1).ToString() + ". " + _projectInfo.utils[i].name + "</b>";
                var descrLabel = e.Q<Label>("util_descr");
                descrLabel.text = _projectInfo.utils[i].description;

                if(_projectInfo.utils[i].files?.Count > 0)
                {
                    descrLabel.text += "\nFiles:";
                    foreach (var filePath in _projectInfo.utils[i].files)
                        descrLabel.text += "\n" + filePath;
                }

            };
            // ListView listView = rootVisualElement.Q<ListView>(className: "the-uxml-listview");
            var listView = rootVisualElement.Q<ListView>();
            listView.makeItem = makeItem;
            listView.bindItem = bindItem;

            listView.itemsSource = _projectInfo.utils;
            listView.selectionType = SelectionType.None;
            //listView.onItemsChosen += OnSelectionChanged;

        }

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Private Methods (static)
        // ---------------------------------------------------------------------------------------------------------

        private static void LoadProjectManifest()
        {
            var path = Path.Combine(Application.dataPath, Constants.PathToProjectManifest);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                _projectInfo = JsonUtility.FromJson<ProjectInfo>(json);
            }
        }

        #endregion
    }
}
