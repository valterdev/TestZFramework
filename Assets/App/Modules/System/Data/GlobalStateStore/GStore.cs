using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Newtonsoft.Json;

namespace ZFramework
{
    //[JsonObject(MemberSerialization.OptIn)]
    /// <summary>
    /// Глобальный стор состояний, призванный уменьшить связность объектов
    /// </summary>
    public class GStore : SingletonCrossScene<GStore>
    {
        //[JsonProperty]
        private Dictionary<TypeCode, IGlobalStore> _globalStores = new Dictionary<TypeCode, IGlobalStore>();

        private List<GStoreClassContainerRef> _allContainerRefs = new List<GStoreClassContainerRef>();

        private Dictionary<string, Action> _observers = new Dictionary<string, Action>();


        private Dictionary<string, Action<object>> _preGet = new Dictionary<string, Action<object>>();
        private Dictionary<string, object> _preGetObjects = new Dictionary<string, object>();

        private Dictionary<string, Action<object>> _postGet = new Dictionary<string, Action<object>>();
        private Dictionary<string, object> _postGetObjects = new Dictionary<string, object>();

        /// <summary>
        /// Функция предварительной инициализации
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
        /// Регистрирует (создает и инициализирует) глобальную статик переменную, чтобы у нас был доступ к стору из любого участка кода.
        /// Реализуется паттерн синглтона (потокобезопасный).
        /// </summary>
        public void RegisterStaticObject()
        {
            App.GStore = GStore.Instance();
        }

        /// <summary>
        /// Собирает информацию и создает сторы со всеми переменными из partial GSHelp
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
            // Собираем информацию о всех моделях данных и сохраняем ее
            var nestedTypes = type.GetNestedTypes();

            for (int i = 0; i < nestedTypes.Length; i++)
            {
                // собираем информацию о встроенных методах
                ConstructorInfo constructor = nestedTypes[i].GetConstructor(Type.EmptyTypes);
                object classObject = constructor.Invoke(new object[] { });

                var nestedMethods = nestedTypes[i].GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

                //foreach (MethodInfo method in nestedMethods)
                //{
                    
                    //if (method.Name.Contains("get_"))
                    //{


                    //    Debug.Log(method.Invoke(classObject, null));
                    //    Debug.Log(method.Name);
                    //}
                //}

                if (nestedTypes[i].GetCustomAttribute<GSStoreIgnoreAttribute>() == null)
                {
                    GStoreClassContainerRef containerRef = new GStoreClassContainerRef(this, nestedTypes[i].Name);

                    //Собираем поля встроенного класса
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
        /// Получаем данные из стора по ключу
        /// </summary>
        /// <param name="key">Ключ типа string</param>
        /// <returns>Возвращает полиморфный объект типа object.
        /// При этом если данного значения в хранилище нет, то вернет дефолтное значение для заданного типа T</returns>
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
                // если хранилище для заданного типа отсутствует, то создаем его
                CreateNewStoreWithSpecialType(typeCode);

                return (T)_globalStores[typeCode].Get<T>(key);
            }
        }

        /// <summary>
        /// Получаем данные из стора по ключу (используется для внутренних целей, если вам нужно получить переменную из стора воспользуйтесь дженерик версией Get<T>)
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(TypeCode typeCode, string key)
        {
            return _globalStores[typeCode].Get(key);
        }

        /// <summary>
        /// Меняет переменную в хранилище с заданным ключом и типом Т, если такой переменной нет, то добавляет ее в хранилище.
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
                // если хранилище для заданного типа отсутствует, то создаем его
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
            //settings.TypeNameHandling = TypeNameHandling.All;

            string json = JsonConvert.SerializeObject(_globalStores, Formatting.Indented, settings);
            return json;//JsonUtility.ToJson(_globalStores);
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
        /// Регистрирует объекты, которые будут обладать реактивностью (будут отслеживаться изменения данной переменной)
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

        /// <summary>
        /// Регистрирует и связывает с переменными функцию (используя атрибут [GSSRegPreHelper("")])
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
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
        /// Регистрирует и связывает с переменными функцию (используя атрибут [GSSRegPostHelper("")])
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
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
        /// Функция создает новое хранилище данных, с заданным типом
        /// </summary>
        /// <param name="type">Тип переменной которая нам нужна, указывается через typeof(тип переменной, например bool)</param>
        private void CreateNewStoreWithSpecialType(TypeCode type)
        {
            _globalStores.Add(type, new GlobalStore());
        }
    }
}
