using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace ZFramework
{
    /// <summary>
    /// Данный класс хранит инфомрацию по всем переменным во встроенных (nested) классах внутри GSHelp.
    /// Позволяет создать функционал и извлекать целые куски из стора (переменные логически объединенные в класс)
    /// </summary>
    public class GStoreClassContainerRef
    {
        public string ClassName { get; }
        public Dictionary<string, string> MapNames { get; private set; }
        public Dictionary<string, TypeCode> MapTypes { get; private set; }

        [JsonProperty]
        private GStore _store;

        public GStoreClassContainerRef(GStore store, string className)
        {
            _store = store;
            ClassName = className;

            MapNames = new Dictionary<string, string>();
            MapTypes = new Dictionary<string, TypeCode>();
        }

        public T Get<T>(string key)
        {
            return _store.Get<T>(MapNames[key]);
        }

        public void Set<T>(string key, T value)
        {
            _store.Set<T>(MapNames[key], value);
        }

        /// <summary>
        /// Функция добавляет информацию о полях (небезопасная функция, она не проверяет на то присутствует ли уже такое поле)
        /// </summary>
        /// <param name="fieldName">Название поля внутри класса</param>
        /// <param name="globalFieldName">Название класса содержащего это поле + / + Название поля внутри класса (именно через это название-ключ получают переменную)</param>
        /// <param name="typeCode"></param>
        public void AddField(string fieldName, string globalFieldName, TypeCode typeCode)
        {
            MapNames.Add(fieldName, globalFieldName);
            MapTypes.Add(fieldName, typeCode);
        }
    }
}