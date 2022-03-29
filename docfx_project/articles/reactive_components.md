# Реактивные компоненты

ZFramework поддерживает реактивность визуальных компонентов. Данные возможность обеспечивается при мощи наблюдения за изменениями в Глобальном хранилище данных.

Чтобы связать любой компонент с глобальным хранилищем нужно вызвать функцию App.GStore?.RegisterReactiveComponent(m_Key, UpdateReactive); Указав ключ переменной из хранилища данных и функцию, которая будет вызываться, в случае, если эта переменная будет изменена.

Пример рективного текста (TMP_Text):
```c#
using UnityEngine;
using TMPro;
using System;

namespace ZFramework.UI {
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
        }

        private void UpdateReactive()
        {
            if(string.IsNullOrEmpty(m_Locale))
            {
                if (string.IsNullOrEmpty(m_Format))
                {
                    _label.text = App.GStore.Get<bool>(m_Key).ToString();
                } else
                {
                    _label.text = string.Format(m_Format, App.GStore.Get<bool>(m_Key).ToString());
                }
            } else
            {
                throw new NotImplementedException("Need add multilanguages features");
                //_label.text = string.Format(m_Locale, App.GStore.Get<bool>(m_Key).ToString());
            }
            
        }
    }
}
```
