using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZFramework.Editor
{
    /// <summary>
    /// Global Definitions Management Functionality (Unity Editor Extension)
    /// </summary>
    public class DefineInspectorWindow : EditorWindow
    {
        // Constants
        private const string _filename = "App/Modules/System/Config/ProjectGlobalDefines.txt";

        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Private fields (static)
        // ---------------------------------------------------------------------------------------------------------

        private static List<DefinePair> _defineSymbols = new List<DefinePair>();
        private static string _pathToFile;
        private static bool _hasReadSymbols;

        // ---------------------------------------------------------------------------------------------------------
        // Private fields
        // ---------------------------------------------------------------------------------------------------------

        private string _newDefine = "";

        #endregion

        #region Nested Types

        public class DefinePair
        {
            public string defineSymbol;
            public bool on;

            public static DefinePair ParseLine(string line)
            {
                string[] split = line.Split(',');
                if (split.Length != 2)
                {
                    return null;
                }
                var pair = new DefinePair
                {
                    defineSymbol = split[0],
                    on = split[1] == "1",
                };
                return pair;
            }

            public string ToFileString()
            {
                return defineSymbol + "," + (on ? "1" : "0");
            }
        }

        #endregion

        #region Unity lifecycle & UI

        private void OnGUI()
        {
            CheckToReadPairsFromFile();

            GUILayout.Label("Defines", EditorStyles.boldLabel);
            int i = 0;
            var definesToDelete = new List<int>();
            foreach (DefinePair pair in _defineSymbols)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(pair.defineSymbol, GUILayout.Width(200));
                bool newOn = EditorGUILayout.Toggle("", pair.on, GUILayout.MinWidth(30));
                if (newOn != pair.on)
                {
                    pair.on = newOn;
                    UpdatePairInFile(pair);
                }

                if (GUILayout.Button("Delete", GUILayout.Width(100)))
                {
                    definesToDelete.Add(i);
                }
                GUILayout.EndHorizontal();
                i++;
            }

            foreach (int deleteIndex in definesToDelete)
            {
                DeletePairFromFile(_defineSymbols[deleteIndex]);
                _defineSymbols.RemoveAt(deleteIndex);
            }

            GUILayout.BeginHorizontal();
            _newDefine = GUILayout.TextField(_newDefine);
            if (GUILayout.Button("Add New", GUILayout.Width(100)) && !string.IsNullOrEmpty(_newDefine))
            {
                var newPair = new DefinePair
                {
                    defineSymbol = _newDefine,
                    on = false,
                };
                _defineSymbols.Add(newPair);
                AddPairToFile(newPair);
                SortDefineSymbols();
                _newDefine = "";
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Compile"))
            {
                SetScriptDefines();
            }
        }

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods (static)
        // ---------------------------------------------------------------------------------------------------------

        [MenuItem("Window/ZFramework/Global Define Inspector")]
        public static void ShowWindow()
        {
            GetWindow(typeof(DefineInspectorWindow));
            CheckToReadPairsFromFile();
        }

        // ---------------------------------------------------------------------------------------------------------
        // Private Methods (static)
        // ---------------------------------------------------------------------------------------------------------

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            CheckToReadPairsFromFile();
        }


        private static void CheckToReadPairsFromFile()
        {
            if (!_hasReadSymbols)
            {
                _pathToFile = Path.Combine(Application.dataPath, _filename);
                _defineSymbols = ParseFileToPairs(_pathToFile);
                SortDefineSymbols();
                _hasReadSymbols = true;
            }
        }


        private static List<DefinePair> ParseFileToPairs(string filepath)
        {
            if (!File.Exists(filepath))
            {
                File.Create(filepath);
            }
            string text = File.ReadAllText(filepath);
            string[] lines = text.Split(';');
            var pairs = new List<DefinePair>();
            foreach (string line in lines)
            {
                var pair = DefinePair.ParseLine(line);
                if (pair != null)
                {
                    pairs.Add(pair);
                }
            }
            return pairs;
        }


        private static void SortDefineSymbols()
        {
            _defineSymbols = _defineSymbols.OrderBy(o => o.defineSymbol).ToList();
        }


        private static void AddPairToFile(DefinePair pair)
        {
            string pairStr = pair.ToFileString();
            string appendStr = pairStr + ";";
            File.AppendAllText(_pathToFile, appendStr);
        }


        private static void DeletePairFromFile(DefinePair pair)
        {
            string needle = pair.defineSymbol;
            string text = File.ReadAllText(_pathToFile);
            int index = text.IndexOf(needle);
            int delimiterIndex = text.IndexOf(";", index + 1);
            if (delimiterIndex < 0)
            {
                text = text.Remove(index);
            } else
            {
                text = text.Remove(index, delimiterIndex - index + 1);
            }
            File.WriteAllText(_pathToFile, text);
        }

        // ---------------------------------------------------------------------------------------------------------
        // Private Methods
        // ---------------------------------------------------------------------------------------------------------

        private void UpdatePairInFile(DefinePair pair)
        {
            DeletePairFromFile(pair);
            AddPairToFile(pair);
        }


        private void SetScriptDefines()
        {
            var targetBuildGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = CreateDefinesString();
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetBuildGroup, defines);
        }


        private string CreateDefinesString()
        {
            string str = "";
            foreach (DefinePair pair in _defineSymbols)
            {
                if (pair.on)
                    str += pair.defineSymbol + ";";
            }
            return str;
        }

        #endregion
    }
}