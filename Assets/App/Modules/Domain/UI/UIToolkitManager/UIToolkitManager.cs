using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.AddressableAssets;
using System;
using System.Linq;
using DG.Tweening;

namespace ZFramework
{
    public class UIToolkitManager : SingletonCrossScene<UIToolkitManager>
    {
        private VisualElement _rootUI;
        public App.Hook<string, VisualElement> OnRenderPage;

        private Dictionary<string, VisualElement> _pages = new Dictionary<string, VisualElement>();
        private Dictionary<string, VisualTreeAsset> _components = new Dictionary<string, VisualTreeAsset>();
        
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
            App.UI = UIToolkitManager.Instance();
            App.OnStart += () => App.UIRouter.Navigate("/index");
        }

        public Promise CreateUI()
        {
            _rootUI = App.Instance.GetComponent<UIDocument>().rootVisualElement;
            App.UIRouter.Init(_rootUI);

            var promise = new Promise();
            App.Instance.StartCoroutine(LoadUXMLView(promise,
                "ui_pages", RegisterUIPages,
                "ui_components", RegisterUIComponents));

            return promise;
        }

        private IEnumerator LoadUXMLView(Promise promise,
            string pagesLabel, Action<string, VisualElement> callbackPages,
            string componentsLabel, Action<string, VisualTreeAsset> callbackComponent)
        {
            var loadHandle = Addressables.LoadAssetsAsync<VisualTreeAsset>(pagesLabel, addressable => {
                VisualElement element = addressable.Instantiate();
                callbackPages.Invoke(addressable.name, element);
            });

            yield return loadHandle;

            loadHandle = Addressables.LoadAssetsAsync<VisualTreeAsset>(componentsLabel, addressable => {
                callbackComponent.Invoke(addressable.name, addressable);
            });

            yield return loadHandle;

            if (loadHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                promise.Fulfill();
            }
        }

        private void RegisterUIPages(string routeLink, VisualElement page)
        {
            page.StretchToParentSize();
            _pages.Add(routeLink, page);
            page.style.display = DisplayStyle.None;
            _rootUI.Add(page);

            App.UIRouter.RegisterLink("/" + routeLink.ToLower(), routeLink);
        }

        private void RegisterUIComponents(string routeLink, VisualTreeAsset component)
        {
            _components.Add(routeLink, component);
        }

        public void OpenPage(string link)
        {
            // TODO: Так как всего одна страница хук OnRenderPage можно вызвать тут
            OnRenderPage.Invoke(link, _pages[link]);

            _pages[link].style.display = DisplayStyle.Flex;
        }

        public void ClosePage(string link)
        {
            _pages[link].style.display = DisplayStyle.None;
        }

        public TemplateContainer GetUIComponentInstance(string componentName)
        {
            return _components[componentName].Instantiate();
        }
    }
}