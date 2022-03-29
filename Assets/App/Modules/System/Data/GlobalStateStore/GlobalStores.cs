using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace ZFramework
{
    public interface IGlobalStore
    {
        public object Get<T>(string key);
        public object Get(string key);


        public void ProcessData(TypeCode typeCode);
        public void Set<T>(string key, T value);
    }

    /// <summary>
    /// Обеспечивать функционал хранилища данных
    /// </summary>
    public class GlobalStore : IGlobalStore
    {
        //private Dictionary<string, T> _store = new Dictionary<string, T>();
        [JsonProperty]
        private Dictionary<string, object> _store = new Dictionary<string, object>();

        /// <summary>
        /// Возвращает переменную из хранилища с заданым ключом, если такой в хранилище нет, то возвращает дефолтное значение для типа Т
        /// </summary>
        /// <typeparam name="T">Тип переменной</typeparam>
        /// <param name="key">Имя переменной (Ее ключ)</param>
        /// <returns></returns>
        public object Get<T>(string key)
        {
            if (_store.ContainsKey(key))
            {
                return _store[key];
            }

            return default(T);
        }

        /// <summary>
        /// Функция используется для внутренних целей и не возвращает дефолтных значений, если переменной с таким ключом нет (в этос лучае вернет null).
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            if (_store.ContainsKey(key))
            {
                return _store[key];
            }

            return null;
        }

        public void ProcessData(TypeCode typeCode)
        {
            switch(typeCode)
            {
            case TypeCode.Int32:
                _store = _store.ToDictionary(x => x.Key, x => (object)Convert.ToInt32(x.Value));
                break;

            case TypeCode.Int64:
                _store = _store.ToDictionary(x => x.Key, x => (object)Convert.ToInt64(x.Value));
                break;

            case TypeCode.Boolean:
                _store = _store.ToDictionary(x => x.Key, x => (object)Convert.ToBoolean(x.Value));
                break;

            case TypeCode.String:
                _store = _store.ToDictionary(x => x.Key, x => (object)Convert.ToString(x.Value));
                break;

            case TypeCode.DateTime:
                _store = _store.ToDictionary(x => x.Key, x => (object)Convert.ToDateTime(x.Value));
                break;

            case TypeCode.Byte:
                _store = _store.ToDictionary(x => x.Key, x => (object)Convert.ToByte(x.Value));
                break;

            case TypeCode.Char:
                _store = _store.ToDictionary(x => x.Key, x => (object)Convert.ToChar(x.Value));
                break;

            case TypeCode.Decimal:
                _store = _store.ToDictionary(x => x.Key, x => (object)Convert.ToDecimal(x.Value));
                break;

            case TypeCode.Double:
                _store = _store.ToDictionary(x => x.Key, x => (object)Convert.ToDouble(x.Value));
                break;

            case TypeCode.Single:
                _store = _store.ToDictionary(x => x.Key, x => (object)Convert.ToSingle(x.Value));
                break;
            }
            
        }

        /// <summary>
        /// Меняет переменную в хранилище с заданным ключом, если такой переменной нет, то добавляет ее в хранилище.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set<T>(string key, T value)
        {
            if (_store.ContainsKey(key))
            {
                _store[key] = value;
            } else
            {
                _store.Add(key, value);
            }
        }
    }
}