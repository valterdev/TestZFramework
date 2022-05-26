using UnityEngine;
using TMPro;
using System;

namespace ZFramework {
    /// <summary>
    /// Данный класс обеспечивает реактивность для текстов UI
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class UIReactiveTextComponent : MonoBehaviour
    {
        public string m_Key;
        public string m_Format;
        public string m_Locale;

        private TMP_Text _label;

        private void Awake()
        {
            if(_label == null)
            {
                _label = GetComponent<TMP_Text>();
            }
        }

        private void Start()
        {
            App.GStore?.RegisterReactiveComponent(m_Key, UpdateReactive);
            UpdateReactive();
        }

        private void UpdateReactive()
        {
            if(string.IsNullOrEmpty(m_Locale))
            {
                if (string.IsNullOrEmpty(m_Format))
                {
                    _label.text = App.GStore.Get<int>(m_Key).ToString();
                } else
                {
                    _label.text = string.Format(m_Format, App.GStore.Get<int>(m_Key).ToString());
                }
            } else
            {
                throw new NotImplementedException("Need add multilanguages features");
            }
            
        }
    }
}