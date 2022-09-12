using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace ZFramework
{
    /// <summary>
    /// Provide data warehouse functionality
    /// </summary>
    public class GlobalStore : IGlobalStore
    {
        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Private fields
        // ---------------------------------------------------------------------------------------------------------

        [JsonProperty]
        private Dictionary<string, object> _store = new Dictionary<string, object>();

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods
        // ---------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns a variable from the storage with the given key, if there is no such one in the storage, then it returns the default value for type T
        /// </summary>
        /// <typeparam name="T">Variable type</typeparam>
        /// <param name="key">Variable name (It's key)</param>
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
        /// The function is used for internal purposes and does not return default values if there is no variable with such a key (it will return null in this case).
        /// </summary>
        /// <param name="key">Variable name (It's key)</param>
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
        /// Changes the variable in the storage with the given key, if there is no such variable, then adds it to the storage.
        /// </summary>
        /// <typeparam name="T">Variable type</typeparam>
        /// <param name="key">Variable name (It's key)</param>
        /// <param name="value">Variable value</param>
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

        #endregion
    }
}
