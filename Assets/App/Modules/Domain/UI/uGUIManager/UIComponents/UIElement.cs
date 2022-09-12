using System.Collections;
using UnityEngine;

namespace ZFramework
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIElement : MonoBehaviour
    {
        #region Properties

        // ---------------------------------------------------------------------------------------------------------
        // Public properties
        // ---------------------------------------------------------------------------------------------------------

        public bool IsShow { get; internal set; }
        public bool IsTransit { get; internal set; }

        // ---------------------------------------------------------------------------------------------------------
        // Internal properties
        // ---------------------------------------------------------------------------------------------------------

        internal UIBase UIBase { get; set; }

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Internal Methods
        // ---------------------------------------------------------------------------------------------------------

        internal IEnumerator ShowRoutine()
        {
            gameObject.SetActive(true);
            IsShow = true;
            IsTransit = true;
            UIBase.OnChangeState(this);

            yield return Show();

            IsTransit = false;
            UIBase.OnChangeState(this);
        }


        internal IEnumerator HideRoutine()
        {
            IsShow = false;
            IsTransit = true;
            UIBase.OnChangeState(this);

            yield return Hide();

            IsTransit = false;
            gameObject.SetActive(false);
            UIBase.OnChangeState(this);
        }

        // ---------------------------------------------------------------------------------------------------------
        // Protected Methods
        // ---------------------------------------------------------------------------------------------------------

        protected virtual IEnumerator Show()
        {
            yield break;
        }


        protected virtual IEnumerator Hide()
        {
            yield break;
        }

        #endregion
    }
}
