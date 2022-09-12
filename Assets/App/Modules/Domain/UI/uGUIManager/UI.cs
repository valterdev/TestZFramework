using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using System;

namespace ZFramework
{
    public class UI : UIBase
    {
        #region Events

        public Action OnClick = () => { };

        #endregion

        #region Fields

        public bool mouseDown;

        #endregion
        

        #region Properties

        // ---------------------------------------------------------------------------------------------------------
        // Public properties
        // ---------------------------------------------------------------------------------------------------------

        // Lock/Unlock Mouse Events
        public bool IsLocked
        {
            get
            {
                return (Root && Root.transform.childCount > 0) ? !Root.GetComponentInChildren<GraphicRaycaster>().enabled : false;
            }
            set
            {
                foreach (var rc in Root.GetComponentsInChildren<GraphicRaycaster>())
                    rc.enabled = !value;
            }
        }

        #endregion

        #region App lifecycle

        /// <summary>
        /// Pre-initialization function
        /// </summary>
        public void PreInit()
        {
            RegisterStaticObject();
            Change += OnChange;

            App.OnBeforeStart += InitValues;
            App.OnStart += ShowLifePanel;
            App.OnStart += ChangeTestVariable;
        }

        /// <summary>
        /// Registers (creates and initializes) a global static variable so that we can access the manager from any part of the code.
        /// The singleton pattern (thread-safe) is implemented.
        /// </summary>
        public void RegisterStaticObject()
        {
            App.UI = UI.Instance();
        }

        #endregion

        #region Unity lifecycle

        protected void Update()
        {
            if (!Root.gameObject.activeSelf)
                return;

            ProcessClick();
            ProcessEscape();
        }

        #endregion

        #region Methods For Sample

        private void InitValues()
        {
            App.GStore.Set<int>("UI/Life", 10);
        }


        private void ChangeTestVariable()
        {
            StartCoroutine(ttt());
        }


        private void ShowLifePanel()
        {
            App.UI.Get<TestLifePanel>().Require(this, +1);
        }


        private IEnumerator ttt()
        {
            yield return new WaitForSeconds(2f);

            App.GStore.Set<int>("UI/Life", 1);

            yield return new WaitForSeconds(2f);

            App.GStore.Set<int>("UI/Life", 2);

            yield return new WaitForSeconds(2f);

            HideLifePanel();

        }


        private void HideLifePanel()
        {
            App.UI.Get<TestLifePanel>().Require(this, -1);
        }

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods
        // ---------------------------------------------------------------------------------------------------------

        public Promise CreateUI()
        {
            if (App.GStore.Get<bool>("NoUI")) return Promise.Resolved();

            var promise = new Promise();
            App.Instance.StartCoroutine(LoadUISceneCoroutine(promise));
            return promise;
        }

        // ---------------------------------------------------------------------------------------------------------
        // Private Methods
        // ---------------------------------------------------------------------------------------------------------

        private IEnumerator LoadUISceneCoroutine(Promise promise)
        {
            yield return ImportUI("UI", "/UILayer");
            //App.OnInitProgress.Invoke(0.5f);
            promise.Fulfill();
            yield break;
        }


        private YieldInstruction ImportUI(string sceneName, string rootName)
        {
            return App.Instance.StartCoroutine(ImportUI_Coroutine(sceneName, rootName));
        }


        private IEnumerator ImportUI_Coroutine(string sceneName, string rootName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            var root = GameObject.Find(rootName);
            
            root.GetComponent<Canvas>().worldCamera = Camera.main;
            root.transform.SetParent(App.UI.Root);

            yield return SceneManager.UnloadSceneAsync(sceneName);
        }


        private void OnChange(UIElement element)
        {
            var isClosed = !element.IsShow && !element.IsTransit;
            if (isClosed)
                ResetAll(element);

            if (element is UIPopup)
            {
                var popup = (BasePopup)element;

                //if (popup.ShadeShow)
                //{
                //    Get<ShadePanel>().Require(popup, popup.IsShow ? 1 : 0);

                //    if (popup.IsTopmost)
                //    {
                //        if (popup.IsShow) Get<ShadePanel>().transform.SetSiblingIndex(queue[0].transform.GetSiblingIndex() - 1);
                //        else if (queue.Count > 1) Get<ShadePanel>().transform.SetSiblingIndex(queue[1].transform.GetSiblingIndex() - 1);
                //    }
                //}

                if (!element.IsTransit)
                {
                    if (popup.IsShow) App.OnPopupOpened.Invoke(popup);
                    else App.OnPopupClosed.Invoke(popup);
                }
            }
        }
        

        private void ProcessClick()
        {
            var mouseDownFrame = Input.GetMouseButtonDown(0);
            if (mouseDown != mouseDownFrame)
            {
                mouseDown = mouseDownFrame;

                if (!mouseDownFrame && !IsLocked && OnClick != null)
                    OnClick();
            }
        }


        private void ProcessEscape()
        {
            // Pressing Back button on android
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Sample using Panels:
                //if (Get<LockPanel>().IsShow) return;
                //if (Get<LoadPanel>().IsAnimating) return;
                //if (Has<PreloaderPanel>() && Get<PreloaderPanel>().IsShow) return;
                //if (Get<TutorPanel>().IsShow) return;

                var topmostPopup = GetTopmostPopup() as BasePopup;
                if (topmostPopup != null)
                {
                    if (topmostPopup.ShadeClickIgnore) return;
                    if (topmostPopup.IsTransit) return;
                    topmostPopup.OnExitButtonClick();
                }
            }
        }

        // Popups

        public TPopup OpenPopup<TPopup>(bool modal = true) where TPopup : BasePopup
        {
            return (TPopup)Get<TPopup>().Open(!modal);
        }

        #endregion
    }
}
