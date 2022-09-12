using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace ZFramework
{
    /// <summary>
    /// This class stores information on all variables in nested classes within GSHelp.
    /// Allows you to create a functional and extract whole pieces from the store (variables logically combined into a class)
    /// </summary>
    public class GStoreClassContainerRef
    {
        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Public fields
        // ---------------------------------------------------------------------------------------------------------

        public string ClassName { get; }
        public Dictionary<string, string> MapNames { get; private set; }
        public Dictionary<string, TypeCode> MapTypes { get; private set; }

        // ---------------------------------------------------------------------------------------------------------
        // Private fields
        // ---------------------------------------------------------------------------------------------------------

        [JsonProperty]
        private GStore _store;

        #endregion

        #region Object lifecycle

        public GStoreClassContainerRef(GStore store, string className)
        {
            _store = store;
            ClassName = className;

            MapNames = new Dictionary<string, string>();
            MapTypes = new Dictionary<string, TypeCode>();
        }

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods
        // ---------------------------------------------------------------------------------------------------------

        public T Get<T>(string key)
        {
            return _store.Get<T>(MapNames[key]);
        }


        public void Set<T>(string key, T value)
        {
            _store.Set<T>(MapNames[key], value);
        }


        /// <summary>
        /// The function adds information about the fields (unsafe function, it does not check if such a field already exists)
        /// </summary>
        /// <param name="fieldName">The name of the field inside the class</param>
        /// <param name="globalFieldName">The name of the class containing this field + / + The name of the field inside the class (it is through this name-key that the variable is obtained)</param>
        /// <param name="typeCode"></param>
        public void AddField(string fieldName, string globalFieldName, TypeCode typeCode)
        {
            MapNames.Add(fieldName, globalFieldName);
            MapTypes.Add(fieldName, typeCode);
        }

        #endregion
    }
}