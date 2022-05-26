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
        public bool mouseDown;
        public Action OnClick = () => { };

        /// <summary>
        /// Функция предварительной инициализации
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
        /// Регистрирует (создает и инициализирует) глобальную статик переменную, чтобы у нас был доступ к стору из любого участка кода.
        /// Реализуется паттерн синглтона (потокобезопасный).
        /// </summary>
        public void RegisterStaticObject()
        {
            App.UI = UI.Instance();
        }

        public Promise CreateUI()
        {
            if (App.GStore.Get<bool>("NoUI")) return Promise.Resolved();

            var promise = new Promise();
            App.Instance.StartCoroutine(LoadUISceneCoroutine(promise));
            return promise;
        }

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

        IEnumerator ttt()
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

        protected void Update()
        {
            if (!Root.gameObject.activeSelf)
                return;

            ProcessClick();
            ProcessEscape();
        }

        /// Lock/Unlock Mouse Events
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
            // Нажатие Back button на андроид
            if (Input.GetKeyDown(KeyCode.Escape))
            {
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
                } else
                {
                    //if (App.LevelManager.BlockExit)
                    //    return;

                    // Если на данный момент есть SettingPanel, то открыть окно выхода
                    //var settingsPanel = App.UI.Get<SettingsPanel>();
                    //if (settingsPanel.interactable)
                    //{
                    //    if (!App.UI.Get<ExitMapPopup>().IsShow)
                    //    {
                    //        if (App.MapManager.IsActive) App.UI.Get<ExitMapPopup>().Open();
                    //        else settingsPanel.OnExitClick();
                    //    }
                    //}
                }
            }
        }

        // Popups

        public TPopup OpenPopup<TPopup>(bool modal = true) where TPopup : BasePopup
        {
            return (TPopup)Get<TPopup>().Open(!modal);
        }
    }

    public class BasePanel : UIBase.UIPanel
    {
        public enum AnimationType { None, Custom, Horizontal, Vertical, VerticalBounce }

        [SerializeField]
        protected AnimationType animationType = AnimationType.None;
        [SerializeField]
        protected float animationTime = 0.25f;

        protected Vector2? animationPosition = null;

        private Stack<int> transformIndexStack = new Stack<int>();

        protected override IEnumerator Show()
        {
            yield return PlayAnimation(true);
        }

        protected override IEnumerator Hide()
        {
            yield return PlayAnimation(false);
        }


        public void Forward(int siblingIndex)
        {
            if (transformIndexStack.Count > 0)
            {
                transformIndexStack.Push(siblingIndex);
                transform.SetSiblingIndex(siblingIndex);
            }
        }

        public void Backwrad()
        {
            if (transformIndexStack.Count > 0)
            {
                var index = transformIndexStack.Pop();
                transform.SetSiblingIndex(index);
            } else
            {
                App.LogWarning(this, "Can't Backward - sibling index stack is empty");
            }
        }

        #region Animations

        protected IEnumerator PlayAnimation(bool active)
        {
            switch (animationType)
            {
            case AnimationType.Custom:
                yield return PlayCustom(active, true);
                break;
            case AnimationType.Vertical:
                yield return PlayVertical(active, true);
                break;
            case AnimationType.Horizontal:
                yield return PlayHorizontal(active, true);
                break;
            case AnimationType.VerticalBounce:
                yield return PlayVerticalBounce(active, true);
                break;
            }
        }

        protected virtual IEnumerator PlayCustom(bool active, bool animate)
        {
            yield break;
        }

        protected virtual IEnumerator PlayVertical(bool active, bool animate)
        {
            yield return PlayDefault(false, true, active, animate, false);
        }

        protected virtual IEnumerator PlayVerticalBounce(bool active, bool animate)
        {
            yield return PlayDefault(false, true, active, animate, true);
        }

        protected virtual IEnumerator PlayHorizontal(bool active, bool animate)
        {
            yield return PlayDefault(true, false, active, animate, false);
        }

        private IEnumerator PlayDefault(bool hor, bool ver, bool active, bool animate, bool bounce)
        {
            // При первом вызове - сохраняем позицию элемента
            if (animationPosition == null)
                SetAnimationPosition(rectTransform.anchoredPosition);

            // Вычисление точек
            var from = animationPosition.Value;
            var to = animationPosition.Value;

            if (hor)
            {
                to.x += rectTransform.sizeDelta.x * Mathf.Sign(to.x);
                to.x *= -1;
            }

            if (ver)
            {
                to.y += rectTransform.sizeDelta.y * Mathf.Sign(to.y);
                to.y *= -1;
            }

            if (active)
            {
                var temp = to;
                to = from;
                from = temp;
            }

            // Анимация
            rectTransform.anchoredPosition = from;
            rectTransform.DOKill();

            if (bounce)
            {
                yield return rectTransform.DOAnchorPos(to, animate ? animationTime : 0f).SetEase(Ease.OutBack).WaitForCompletion();
            } else
            {
                yield return rectTransform.DOAnchorPos(to, animate ? animationTime : 0f).SetEase(Ease.InOutSine).WaitForCompletion();
            }
        }

        public void SetAnimationPositionX(float x)
        {
            if (animationPosition == null)
                return;
            animationPosition = new Vector2(x, animationPosition.Value.y);
        }
        public void SetAnimationPositionY(float y)
        {
            if (animationPosition == null)
                return;
            animationPosition = new Vector2(animationPosition.Value.x, y);
        }
        public void SetAnimationPosition(Vector2 position)
        {
            animationPosition = position;
        }

        #endregion

        //
        // Delegate
        //

        public RectTransform rectTransform { get { return GetComponent<RectTransform>(); } }
        public Animator animator { get { return GetComponent<Animator>(); } }

        public bool interactable
        {
            get { return GetComponent<CanvasGroup>().interactable; }
            set { GetComponent<CanvasGroup>().interactable = value; }
        }

        public bool blocksRaycasts
        {
            get { return GetComponent<CanvasGroup>().blocksRaycasts; }
            set { GetComponent<CanvasGroup>().blocksRaycasts = value; }
        }

        public float alpha
        {
            get { return GetComponent<CanvasGroup>().alpha; }
            set { GetComponent<CanvasGroup>().alpha = value; }
        }

        protected IEnumerator PlayAndWaitForComplete(string stateName)
        {
            if (animator != null && animator.enabled)
            {
                animator.Play(stateName);

                // Ждём длинну анимации + 1 кадр (для того чтобы аниматор успел перейти на стейт)
                yield return null;
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            }
        }
    }

    public class BasePopup : UIBase.UIPopup
    {
        protected IEnumerator PlayAndWaitForComplete(string stateName)
        {
            interactable = false;

            if (animator != null && animator.enabled)
            {
                animator.Play(stateName);

                // Ждём длинну анимации + 1 кадр (для того чтобы аниматор успел перейти на стейт)
                yield return null;
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            }

            interactable = true;
        }

        protected IEnumerator WaitForPurchaseManagerOrClose()
        {
            if (animator) animator.enabled = false;
            if (canvasGroup) canvasGroup.alpha = 0;

            //yield return App.UI.Get<LockPanel>().Open(App.PurchaseManager.Initialize()).Then(() =>
            //{
            //    if (animator) animator.enabled = true;
            //    if (canvasGroup) canvasGroup.alpha = 1;
            //}, e =>
            //{
            //    App.UI.Get<ErrorPopup>().Open(
            //        e is InternetException
            //            ? ErrorPopupType.WiFi
            //            : ErrorPopupType.Unknown,
            //        ErrorPopupButton.Cancel);

            //    Close();
            //});

            // Тут необходимо дождаться 1 кадра для того чтобы в случае с Close()
            // это был последний такт корутины и дальше процесс не пошёл
            yield return null;
        }

        public override void Shade(bool enable) { }

        //
        // Close
        //

        public virtual void OnExitButtonClick() { Close(); }

        protected override IEnumerator Show()
        {
            yield return base.Show();
            interactable = true;
        }

        //
        // Properties
        //
        public bool Contains(Transform child)
        {
            return GetComponentsInChildren<Transform>().Contains(child);
        }

        protected void HidePanel<T>(bool animation = false) where T : BasePanel
        {
            App.UI.Get<T>().Require(this, -1);
        }

        private List<KeyValuePair<BasePanel, int>> upwarded = new List<KeyValuePair<BasePanel, int>>();

        protected void UpwardPanel<T>() where T : BasePanel
        {
            var panel = App.UI.Get<T>();

            // if (upwarded.Any(x => x.Key == panel)) return;

            if (upwarded.Any(x => x.Key == panel) == false)
            {
                upwarded.Add(new KeyValuePair<BasePanel, int>(
                    panel,
                    panel.transform.GetSiblingIndex()
                ));
            }

            panel.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
        }

        protected void Restore()
        {
            foreach (var p in upwarded)
                p.Key.transform.SetSiblingIndex(p.Value);

            upwarded.Clear();
        }


        public CanvasGroup canvasGroup { get { return GetComponent<CanvasGroup>(); } }
        public Animator animator { get { return GetComponent<Animator>(); } }
        public RectTransform rectTransform { get { return GetComponent<RectTransform>(); } }
        public bool interactable
        {
            get { return GetComponent<CanvasGroup>().interactable; }
            set { GetComponent<CanvasGroup>().interactable = value; }
        }




        public bool ShadeShow = true;
        public bool ShadeClickIgnore = false;

        protected bool m_AutoOpened;

        public void SetAutoOpened(bool newValue = true)
        {
            m_AutoOpened = newValue;
        }

        public bool IsAutoOpened { get { return m_AutoOpened; } }
    }
}