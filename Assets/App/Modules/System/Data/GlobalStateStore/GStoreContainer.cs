using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ZFramework
{
    /// <summary>
    /// Using this class, you can separately get a number of variables from the global storage related to one class (internal classes declared in GSHelp)
    /// </summary>
    public class GStoreContainer
    {
        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Private fields
        // ---------------------------------------------------------------------------------------------------------

        private GStore _store;
        private GStoreClassContainerRef _refStore;
        private Dictionary<TypeCode, IGlobalStore> _localStores = new Dictionary<TypeCode, IGlobalStore>();

        #endregion

        #region Object lifecycle

        public GStoreContainer(GStore store, GStoreClassContainerRef gStoreClassContainerRef)
        {
            _store = store;
            _refStore = gStoreClassContainerRef;
            InitStore();
        }

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Private Methods
        // ---------------------------------------------------------------------------------------------------------

        private void InitStore()
        {
            foreach(KeyValuePair<string, string> val in _refStore.MapNames)
            {
                if (!_localStores.ContainsKey(_refStore.MapTypes[val.Key]))
                {
                    // if there is no storage for the given type, then we create it
                    _localStores.Add(_refStore.MapTypes[val.Key], new GlobalStore());

                }

                _localStores[_refStore.MapTypes[val.Key]].Set(val.Key, _store.Get(_refStore.MapTypes[val.Key], val.Value));
            }
        }

        #endregion
    }
}
