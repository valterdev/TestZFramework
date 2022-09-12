using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using UnityEditor;
using System.IO;

/// <summary>
/// Extentions' Methods for root page (UI Element uxml)
/// </summary>
public static class VisualElementBinder
{
    #region Extentions

    public static void BindBtn(this VisualElement elem, string btnName, Action action)
    {
        elem.Q<Button>(btnName).clickable.clicked += action;
    }

    public static void SetText(this VisualElement elem, string labelName, string value)
    {
        elem.Q<Label>(labelName).text = value;
    }

    public static void AddText(this VisualElement elem, string labelName, string value)
    {
        elem.Q<Label>(labelName).text += value;
    }

    public static Toggle SetToggle(this VisualElement elem, string toggleName, bool value, int id = 0)
    {
        var tgl = elem.Q<Toggle>(toggleName);
        tgl.value = value;
        tgl.userData = id;

        return tgl;
    }

    public static void Hide(this VisualElement elem)
    {
        elem.style.display = DisplayStyle.None;
    }

    public static void Show(this VisualElement elem)
    {
        elem.style.display = DisplayStyle.Flex;
    }

    #endregion
}
