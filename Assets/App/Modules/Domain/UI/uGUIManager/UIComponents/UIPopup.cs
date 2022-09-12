using System.Collections;

namespace ZFramework
{
    public abstract class UIPopup : UIElement, IEnumerator
    {
        #region Properties

        // ---------------------------------------------------------------------------------------------------------
        // Public properties
        // ---------------------------------------------------------------------------------------------------------

        public bool IsTopmost { get { return this == UIBase.GetTopmostPopup(); } }

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods (Abstract)
        // ---------------------------------------------------------------------------------------------------------

        public abstract void Shade(bool enable);

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods
        // ---------------------------------------------------------------------------------------------------------

        public UIPopup Open(bool enqueue = false) { UIBase.Enqueue(this, enqueue); return this; }
        public UIPopup Close() { UIBase.Dequeue(this); return this; }

        #endregion

        #region IEnumerator: for Unity Coroutine

        // ---------------------------------------------------------------------------------------------------------
        // Internal properties
        // ---------------------------------------------------------------------------------------------------------

        object IEnumerator.Current { get { return null; } }

        // ---------------------------------------------------------------------------------------------------------
        // Internal Methods
        // ---------------------------------------------------------------------------------------------------------

        void IEnumerator.Reset() { }
        bool IEnumerator.MoveNext() { return UIBase.IsAwait(this); }

        #endregion
    }
}
