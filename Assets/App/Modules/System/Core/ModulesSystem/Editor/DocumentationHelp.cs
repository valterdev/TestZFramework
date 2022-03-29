using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ZFramework.Editor
{
    public class DocumentationHelp : EditorWindow
    {
        [MenuItem("Window/ZFramework/Documentation")]
        public static void OpenDocumentation()
        {
            Application.OpenURL("file://" + Path.Combine(Application.dataPath, "../docfx_project/_site/index.html"));
        }
    }
}