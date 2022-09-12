using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Newtonsoft.Json;

namespace ZFramework
{
    /// <summary>
    /// Global state store designed to reduce the connectivity of objects.
    /// </summary>
    public class GStore : SingletonCrossScene<GStore>
    {
        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Private fields
        // ---------------------------------------------------------------------------------------------------------

        private Dictionary<TypeCode, IGlobalStore> _globalStores = new Dictionary<TypeCode, IGlobalStore>();

        private List<GStoreClassContainerRef> _allContainerRefs = new List<GStoreClassContainerRef>();

        private Dictionary<string, Action> _observers = new Dictionary<string, Action>();


        private Dictionary<string, Action<object>> _preGet = new Dictionary<string, Action<object>>();
        private Dictionary<string, object> _preGetObjects = new Dictionary<string, object>();

        private Dictionary<string, Action<object>> _postGet = new Dictionary<string, Action<object>>();
        private Dictionary<string, object> _postGetObjects = new Dictionary<string, object>();

        #endregion

        #region App lifecycle

        /// <summary>
        /// Pre-initialization function
        /// </summary>
        public void PreInit()
        {
            RegisterStaticObject();
            InitStores();
        }


        public void Init()
        {
            
        }

        /// <summary>
        /// Registers (creates and initializes) a global static variable so that we can access the manager from any part of the code.
        /// The singleton pattern (thread-safe) is implemented.
        /// </summary>
        public void RegisterStaticObject()
        {
            App.GStore = GStore.Instance();
        }

        #endregion

        #region Methods
        // ---------------------------------------------------------------------------------------------------------
        // Public Methods
        // ---------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Collects information and creates stores with all variables from partial GSHelp
        /// </summary>
        public void InitStores()
        {
            var type = typeof(GSHelp);
            var fields = type.GetFields();

            foreach (FieldInfo field in fields)
            {
                if (field.GetCustomAttribute<GSStoreIgnoreAttribute>() == null)
                {
                    App.GStore.Set(field.Name, field.GetValue(null), false);
                }
            }
            // models
            // We collect information about all data models and save it
            var nestedTypes = type.GetNestedTypes();

            for (int i = 0; i < nestedTypes.Length; i++)
            {
                // collecting information about built-in methods
                ConstructorInfo constructor = nestedTypes[i].GetConstructor(Type.EmptyTypes);
                object classObject = constructor.Invoke(new object[] { });

                var nestedMethods = nestedTypes[i].GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

                if (nestedTypes[i].GetCustomAttribute<GSStoreIgnoreAttribute>() == null)
                {
                    GStoreClassContainerRef containerRef = new GStoreClassContainerRef(this, nestedTypes[i].Name);

                    // Collecting the fields of the nested class
                    var nestedFields = nestedTypes[i].GetFields();

                    foreach (FieldInfo field in nestedFields)
                    {
                        if (field.GetCustomAttribute<GSStoreIgnoreAttribute>() == null)
                        {
                            if (field.GetValue(null) != null)
                            {
                                // set default value
                                App.GStore.Set(nestedTypes[i].Name + "/" + field.Name, field.GetValue(null), false);
                            }

                            containerRef.AddField(field.Name, nestedTypes[i].Name + "/" + field.Name, Type.GetTypeCode(field.GetType()));

                            GSSRegPreHelperAttribute specialRegAttribute = field.GetCustomAttribute<GSSRegPreHelperAttribute>();
                            
                            if (specialRegAttribute != null)
                            {
                                foreach (MethodInfo method in nestedMethods)
                                {
                                    if(method.Name == specialRegAttribute.HelperMethodName)
                                    {
                                        var action = GSSActionBuilder.BuildAction<Action<object>>(method);
                                        RegisterPreGet(nestedTypes[i].Name + "/" + field.Name, action, classObject);
                                        //action(classObject);
                                    }
                                }
                            }

                            GSSRegPostHelperAttribute specialRegPostAttribute = field.GetCustomAttribute<GSSRegPostHelperAttribute>();

                            if (specialRegPostAttribute != null)
                            {
                                foreach (MethodInfo method in nestedMethods)
                                {
                                    if (method.Name == specialRegPostAttribute.HelperMethodName)
                                    {
                                        var action = GSSActionBuilder.BuildAction<Action<object>>(method);
                                        RegisterPostGet(nestedTypes[i].Name + "/" + field.Name, action, classObject);
                                        //action(classObject);
                                    }
                                }
                            }
                        }
                    }

                    _allContainerRefs.Add(containerRef);
                }
            }
        }


        /// <summary>
        /// Getting data from the store by key
        /// </summary>
        /// <param name="key">Key of type string</param>
        /// <returns>Returns a polymorphic object of type object.
        /// Moreover, if this value is not in the storage, it will return the default value for the given type T</returns>
        public T Get<T>(string key)
        {
            TypeCode typeCode = Type.GetTypeCode(typeof(T));

            if (_globalStores.ContainsKey(typeCode))
            {
                if (_preGet.ContainsKey(key))
                {
                    _preGet[key].Invoke(_preGetObjects[key]);
                }

                return (T)_globalStores[typeCode].Get<T>(key);
            } else
            {
                // if there is no storage for the given type, then we create it
                CreateNewStoreWithSpecialType(typeCode);

                return (T)_globalStores[typeCode].Get<T>(key);
            }
        }


        /// <summary>
        /// We get data from the store by key (used for internal purposes, if you need to get a variable from the store, use the generic version of Get<T>)
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(TypeCode typeCode, string key)
        {
            return _globalStores[typeCode].Get(key);
        }


        /// <summary>
        /// Changes the variable in the storage with the given key and type T, if there is no such variable, then adds it to the storage.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set<T>(string key, T value)
        {
            Set<T>(key, value, true);
        }


        public void Set<T>(string key, T value, bool withPost)
        {
            TypeCode typeCode;

            if (value == null)
            {
                typeCode = Type.GetTypeCode(typeof(T));
            } else
            {
                typeCode = Type.GetTypeCode(value.GetType());
            }

            if (!_globalStores.ContainsKey(typeCode))
            {
                // if there is no storage for the given type, then we create it
                CreateNewStoreWithSpecialType(typeCode);
            }

            _globalStores[typeCode].Set(key, value);

            if (withPost)
            {
                if (_postGet.ContainsKey(key))
                {
                    _postGet[key].Invoke(_postGetObjects[key]);
                }
            }

            CheckObserverAndUpdateIfNeeded(key);
        }


        public string GetJsonAllData()
        {
            var settings = new JsonSerializerSettings();

            string json = JsonConvert.SerializeObject(_globalStores, Formatting.Indented, settings);
            return json;
        }


        public void ProcessLoadedData()
        {
            foreach(KeyValuePair<TypeCode, IGlobalStore> store in _globalStores)
            {
                store.Value.ProcessData(store.Key);
            }
        }


        public void ImportFromJson(string json)
        {
            var settings = new JsonSerializerSettings();
            //settings.TypeNameHandling = TypeNameHandling.All;
            settings.Converters.Add(new GlobalStoreConverter());

            _globalStores = JsonConvert.DeserializeObject<Dictionary<TypeCode, IGlobalStore>>(json, settings);
            //JsonConvert.PopulateObject(json, _globalStores);
        }


        public class GlobalStoreConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(IGlobalStore);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return serializer.Deserialize(reader, typeof(GlobalStore));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, value);
            }
        }


        /// <summary>
        /// Registers objects that will be reactive (changes to this variable will be tracked)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public void RegisterReactiveComponent(string key, Action action)
        {
            if (_observers.ContainsKey(key))
            {
                _observers[key] += action;
            } else
            {
                _observers.Add(key, action);
            }
        }

        // ---------------------------------------------------------------------------------------------------------
        // Private Methods
        // ---------------------------------------------------------------------------------------------------------

        private void CheckObserverAndUpdateIfNeeded(string key)
        {
            if (_observers.ContainsKey(key))
            {
                foreach (Action action in _observers[key].GetInvocationList())
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception e)
                    {
                        App.LogException(action.Method.Name, e);
                    }
                }
            }
        }


        /// <summary>
        /// Registers and associates a function with variables (using attribute [GSSRegPreHelper("")])
        /// </summary>
        /// <param name="key">Variable key</param>
        /// <param name="action">Function</param>
        /// <param name="classObject"></param>
        private void RegisterPreGet(string key, Action<object> action, object classObject)
        {
            if (!_preGet.ContainsKey(key))
            {
                _preGet.Add(key, action);
                _preGetObjects.Add(key, classObject);
            }
        }


        /// <summary>
        /// Registers and associates a function with variables (using attribute [GSSRegPostHelper("")])
        /// </summary>
        /// <param name="key">Variable key</param>
        /// <param name="action">Function</param>
        /// <param name="classObject"></param>
        private void RegisterPostGet(string key, Action<object> action, object classObject)
        {
            if (!_postGet.ContainsKey(key))
            {
                _postGet.Add(key, action);
                _postGetObjects.Add(key, classObject);
            }
        }


        /// <summary>
        /// The function creates a new data store, with the given type
        /// </summary>
        /// <param name="type">The type of the variable that we need is indicated through typeof (variable type, for example bool)</param>
        private void CreateNewStoreWithSpecialType(TypeCode type)
        {
            _globalStores.Add(type, new GlobalStore());
        }

        #endregion
    }
}
