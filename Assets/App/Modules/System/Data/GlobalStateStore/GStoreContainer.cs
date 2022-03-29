using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ZFramework
{
    /// <summary>
    /// Используя данный класс можно получить отдельно ряд переменных из глобального хранилища связанных в один класс (внутренние классы объявленные в GSHelp)
    /// </summary>
    public class GStoreContainer
    {
        private GStore _store;
        private GStoreClassContainerRef _refStore;
        private Dictionary<TypeCode, IGlobalStore> _localStores = new Dictionary<TypeCode, IGlobalStore>();

        public GStoreContainer(GStore store, GStoreClassContainerRef gStoreClassContainerRef)
        {
            _store = store;
            _refStore = gStoreClassContainerRef;
            InitStore();
        }

        private void InitStore()
        {
            foreach(KeyValuePair<string, string> val in _refStore.MapNames)
            {
                if (!_localStores.ContainsKey(_refStore.MapTypes[val.Key]))
                {
                    // если хранилище для заданного типа отсутствует, то создаем его
                    _localStores.Add(_refStore.MapTypes[val.Key], new GlobalStore());

                }

                _localStores[_refStore.MapTypes[val.Key]].Set(val.Key, _store.Get(_refStore.MapTypes[val.Key], val.Value));
            }
        }
    }
}
