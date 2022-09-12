using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using ZFramework;

namespace ZFramework
{
    public class BasePanel : UIPanel
    {
        public enum AnimationType { None, Custom, Horizontal, Vertical, VerticalBounce }

        #region Serialized Fields

        [SerializeField]
        protected AnimationType animationType = AnimationType.None;

        [SerializeField]
        protected float animationTime = 0.25f;

        #endregion

        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Protected fields
        // ---------------------------------------------------------------------------------------------------------

        protected Vector2? animationPosition = null;

        // ---------------------------------------------------------------------------------------------------------
        // Private fields
        // ---------------------------------------------------------------------------------------------------------

        private Stack<int> transformIndexStack = new Stack<int>();

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Protected Methods (override)
        // ---------------------------------------------------------------------------------------------------------

        protected override IEnumerator Show()
        {
            yield return PlayAnimation(true);
        }

        protected override IEnumerator Hide()
        {
            yield return PlayAnimation(false);
        }

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods
        // ---------------------------------------------------------------------------------------------------------

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

        public void Forward(int siblingIndex)
        {
            if (transformIndexStack.Count > 0)
            {
                transformIndexStack.Push(siblingIndex);
                transform.SetSiblingIndex(siblingIndex);
            }
        }

        #endregion

        #region Animations

        // ---------------------------------------------------------------------------------------------------------
        // Public properties
        // ---------------------------------------------------------------------------------------------------------

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

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods
        // ---------------------------------------------------------------------------------------------------------

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

        // ---------------------------------------------------------------------------------------------------------
        // Protected Methods
        // ---------------------------------------------------------------------------------------------------------

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

        #endregion
    }
}
