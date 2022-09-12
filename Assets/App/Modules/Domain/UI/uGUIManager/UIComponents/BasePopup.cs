using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFramework;
using System.Linq;

namespace ZFramework
{
    public class BasePopup : UIPopup
    {
        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Public fields
        // ---------------------------------------------------------------------------------------------------------

        public bool ShadeShow = true;
        public bool ShadeClickIgnore = false;

        // ---------------------------------------------------------------------------------------------------------
        // Protected fields
        // ---------------------------------------------------------------------------------------------------------

        protected bool m_AutoOpened;

        // ---------------------------------------------------------------------------------------------------------
        // Private fields
        // ---------------------------------------------------------------------------------------------------------

        private List<KeyValuePair<BasePanel, int>> upwarded = new List<KeyValuePair<BasePanel, int>>();

        #endregion

        #region Properties

        // ---------------------------------------------------------------------------------------------------------
        // Public properties
        // ---------------------------------------------------------------------------------------------------------

        public CanvasGroup canvasGroup { get { return GetComponent<CanvasGroup>(); } }
        public Animator animator { get { return GetComponent<Animator>(); } }
        public RectTransform rectTransform { get { return GetComponent<RectTransform>(); } }

        public bool interactable
        {
            get { return GetComponent<CanvasGroup>().interactable; }
            set { GetComponent<CanvasGroup>().interactable = value; }
        }

        public bool IsAutoOpened { get { return m_AutoOpened; } }

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods
        // ---------------------------------------------------------------------------------------------------------

        public override void Shade(bool enable) { }
        public virtual void OnExitButtonClick() { Close(); }


        public void SetAutoOpened(bool newValue = true)
        {
            m_AutoOpened = newValue;
        }


        public bool Contains(Transform child)
        {
            return GetComponentsInChildren<Transform>().Contains(child);
        }

        // ---------------------------------------------------------------------------------------------------------
        // Protected Methods
        // ---------------------------------------------------------------------------------------------------------

        protected IEnumerator PlayAndWaitForComplete(string stateName)
        {
            interactable = false;

            if (animator != null && animator.enabled)
            {
                animator.Play(stateName);

                // We are waiting for the length of the animation + 1 frame (so that the animator has time to switch to the state)
                yield return null;
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            }

            interactable = true;
        }


        protected IEnumerator WaitForPurchaseManagerOrClose()
        {
            if (animator)
                animator.enabled = false;
            if (canvasGroup)
                canvasGroup.alpha = 0;


            // Тут необходимо дождаться 1 кадра для того чтобы в случае с Close()
            // это был последний такт корутины и дальше процесс не пошёл
            yield return null;
        }


        protected override IEnumerator Show()
        {
            yield return base.Show();
            interactable = true;
        }
        

        protected void HidePanel<T>(bool animation = false) where T : BasePanel
        {
            App.UI.Get<T>().Require(this, -1);
        }
        

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

        #endregion
    }
}
