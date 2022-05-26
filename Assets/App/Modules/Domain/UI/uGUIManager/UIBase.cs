using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZFramework
{
    public class UIBase : SingletonCrossScene<UI>
    {
        public event Action<UIElement> Change;

        [SerializeField] protected Transform root;
        private Dictionary<Type, UIElement> elements;
        public List<UIPopup> queue; // переведен в public для фикса некоторый багов (нужно было узнать вызван ли BankPopup, когда он еще не показывался в Topmost)
        protected UIPopup topmost;

        private List<Relation> relations;
        private bool relationsIsDirty;

        public UIBase()
        {
            elements = new Dictionary<Type, UIElement>();
            queue = new List<UIPopup>();
            relations = new List<Relation>();
        }

        public void Destroy<T>() where T : UIElement
        {
            var key = typeof(T);
            if (elements.ContainsKey(key) == true)
            {
                var element = elements[key];
                elements.Remove(key);
                DestroyImmediate(element.gameObject, true);
            }
        }

        public Transform Root
        {
            get {
                if(root == null)
                {
                    root = GameObject.Find("/App/UI").transform;
                }

                return root;
            }
        }

        public bool Has<T>() where T : UIElement
        {
            return elements.ContainsKey(typeof(T));
        }

        public T Get<T>() where T : UIElement
        {
            // Editor mode
            if (!Application.isPlaying)
            {
                return GameObject.Find("UILayer_2").GetComponentInChildren<T>(true);
            }

            var key = typeof(T);

            if (elements.ContainsKey(key) == false)
            {
                if (root == null)
                    throw new Exception("Root transform doesn't set");

                var value = root.GetComponentInChildren<T>(true);
                if (value == null)
                    throw new Exception("UIElement '" + key.Name + "' doesn't found");

                value.UIBase = this;
                elements[key] = value;
            }

            return (T)elements[key];
        }

        public UIPopup GetTopmostPopup()
        {
            return topmost;
        }

        public void ResetAll(object token)
        {
            foreach (var relation in relations)
                if (relation.token == token)
                    relation.priority = 0;

            relationsIsDirty = true;
        }

        internal void Require(UIPanel panel, object token, int priority, bool immediately)
        {
            var relation = relations.FirstOrDefault(x => x.panel == panel && x.token == token);
            if (relation == null && priority != 0)
            {
                // Create new relation
                relationsIsDirty = true;
                relations.Add(relation = new Relation()
                {
                    panel = panel,
                    token = token,
                    priority = priority
                });
            }
            else if (relation != null && relation.priority != priority)
            {
                // Update exist relation
                relationsIsDirty = true;
                relation.priority = priority;

                // Move to end of the list
                relations.Remove(relation);
                relations.Add(relation);
            }

            if (immediately) LateUpdate();
        }

        internal void Enqueue(UIPopup popup, bool enqueue)
        {
            if (queue.IndexOf(popup) == -1)
            {
                if (enqueue) queue.Add(popup);
                else queue.Insert(0, popup);

                StartCoroutine(ProcessPopup());
            }
        }

        internal void Dequeue(UIPopup popup)
        {
            popup.IsShow = false;
        }

        internal bool IsAwait(UIPopup popup)
        {
            return queue.IndexOf(popup) != -1   // Попап в очереди
                || popup.IsShow == true         // или сейчас отображается
                || popup.IsTransit == true;     // или сейчас проигрывается анимация закрытия
        }

        private IEnumerator ProcessPopup()
        {
            while (queue.Count > 0)
            {
                var popup = queue[0];
                if (popup.IsShow || popup.IsTransit) break;

                // Shade
                var shaded = queue.Count > 1 && queue[1].IsShow ? queue[1] : null;
                if (shaded != null) shaded.Shade(true);

                topmost = popup;

                yield return PlayRoutineWhile(popup, popup.ShowRoutine(), true);    // Show
                while (popup.IsShow) yield return null;                             // Await for Dequeue()
                yield return PlayRoutineWhile(popup, popup.HideRoutine(), false);   // Hide

                // Unshade
                if (shaded != null && shaded.IsShow)
                    shaded.Shade(false);
            }
        }

        internal void OnChangeState(UIElement element)
        {
            if (element is UIPopup)
            {
                var popup = (UIPopup)element;
                if (popup.IsShow == false && popup.IsTransit == false)
                    queue.Remove(popup);

                topmost = queue.Count > 0 ? queue?[0] : null;

                if (topmost != null)
                {
                    topmost.Shade(false);
                }
            }

            if (Change != null)
                Change(element);
        }

        private IEnumerator PlayRoutineWhile(UIPopup popup, IEnumerator routine, bool value)
        {
            var coroutine = StartCoroutine(routine);

            while (popup.IsTransit)
            {
                if (popup.IsShow == value)
                {
                    yield return null;
                }
                else
                {
                    StopCoroutine(coroutine);
                    yield break;
                }
            }
        }

        private void LateUpdate()
        {
            if (relationsIsDirty)
            {
                relationsIsDirty = false;

                // Calculate UIPanels strongest relation
                var dominants = new Dictionary<UIPanel, Relation>();
                foreach (var relation in relations)
                {
                    var needUpdate = !dominants.ContainsKey(relation.panel)
                                    || !dominants[relation.panel].IsDominant(relation);

                    if (needUpdate) dominants[relation.panel] = relation;
                }

                // Apply relations
                foreach (var relation in dominants.Values)
                    relation.Apply();

                // Remove all zero relations (only for clean-up relations list, not actually doing anything else)
                for (var i = 0; i < relations.Count; i++)
                    if (relations[i].priority == 0)
                        relations.RemoveAt(i--);
            }
        }

        private class Relation
        {
            public object token;
            public UIPanel panel;
            public int priority;

            public void Apply()
            {
                panel.UIBase.StartCoroutine(Apply_Routine());
            }

            private IEnumerator Apply_Routine()
            {
                if (priority > 0 && !panel.IsShow)
                    yield return panel.ShowRoutine();
                else if (priority <= 0 && panel.IsShow)
                    yield return panel.HideRoutine();
            }

            public bool IsDominant(Relation other)
            {
                return panel == other.panel
                    && Mathf.Abs(priority) > Mathf.Abs(other.priority);
            }
        }

        #region Base Classes

        [RequireComponent(typeof(RectTransform))]
        [RequireComponent(typeof(CanvasGroup))]
        public abstract class UIElement : MonoBehaviour
        {
            public bool IsShow { get; internal set; }
            public bool IsTransit { get; internal set; }

            internal UIBase UIBase { get; set; }

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

            protected virtual IEnumerator Show() { yield break; }
            protected virtual IEnumerator Hide() { yield break; }
        }

        public abstract class UIPanel : UIElement
        {
            // Отобразить (priority > 0) / Cкрыть (priority <= 0)
            // выполено ввиде приоритетов из-за того что разные элементы могут накладывать разные
            // требования на отображение/сокрытие панели, и через приоритеты разрешаются конфликты
            public void Require(object token, int priority, bool immediately = false) { UIBase.Require(this, token, priority, immediately); }
            public void Open() { Require(this, +1, true); }
            public void Close() { Require(this, 0, true); }
        }

        public abstract class UIPopup : UIElement, IEnumerator
        {
            public UIPopup Open(bool enqueue = false) { UIBase.Enqueue(this, enqueue); return this; }
            public UIPopup Close() { UIBase.Dequeue(this); return this; }
            public bool IsTopmost { get { return this == UIBase.GetTopmostPopup(); } }
            public abstract void Shade(bool enable);

            #region IEnumerator: for Unity Coroutine
            void IEnumerator.Reset() { }
            bool IEnumerator.MoveNext() { return UIBase.IsAwait(this); }
            object IEnumerator.Current { get { return null; } }
            #endregion
        }

        #endregion
    }
}