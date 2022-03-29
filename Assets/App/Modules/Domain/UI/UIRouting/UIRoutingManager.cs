using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZFramework
{
    public class UIRoutingManager : SingletonCrossScene<UIRoutingManager>
    {
        private VisualElement _rootUI;

        private Dictionary<string, string> _pagesMap = new Dictionary<string, string>();

        private string _currentPageLink = string.Empty;
        
        /// <summary>
        /// Функция предварительной инициализации
        /// </summary>
        public void PreInit()
        {
            RegisterStaticObject();
        }

        /// <summary>
        /// Регистрирует (создает и инициализирует) глобальную статик переменную, чтобы у нас был доступ к стору из любого участка кода.
        /// Реализуется паттерн синглтона (потокобезопасный).
        /// </summary>
        public void RegisterStaticObject()
        {
            App.UIRouter = UIRoutingManager.Instance();
        }

        public void Init(VisualElement rootUI)
        {
            _rootUI = rootUI;
        }

        public void RegisterLink(string link, string pageKey)
        {
            _pagesMap.Add(link, pageKey);
        }

        public void Navigate(string pageLink)
        {
            if(_currentPageLink != string.Empty)
            {
                App.UI.ClosePage(_pagesMap[_currentPageLink]);
            }

            _currentPageLink = pageLink;
            App.UI.OpenPage(_pagesMap[_currentPageLink]);
        }
    }
}
