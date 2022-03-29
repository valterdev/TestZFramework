using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ZFramework
{
    public partial class App : MonoBehaviour
    {
        // DEBUG,
        // WARNING,
        // ERROR,
        // DISABLED

        public static void Log(object source, object message, params object[] args)
        {
            if (App.GStore.Get<int>("LogLevel") > 0)
                return;

            var output = ComposeOutput(source, message == null ? string.Empty : message.ToString(), args);
            var unityObject = source as UnityEngine.Object;
            if (unityObject != null)
            {
                Debug.Log(output, unityObject);
            } else
            {
                Debug.Log(output);
            }
        }

        public static void LogWarning(object source, string message, params object[] args)
        {
            Debug.Log($"message {message}");
            if (App.GStore.Get<int>("LogLevel") > 1)
                return;

            var output = ComposeOutput(source, message, args);
            var unityObject = source as UnityEngine.Object;
            if (unityObject != null)
            {
                Debug.LogWarning(output, unityObject);
            } else
            {
                Debug.LogWarning(output);
            }
        }

        public static void LogException(object source, string message, params object[] args)
        {
            if (App.GStore.Get<int>("LogLevel") > 2)
                return;

            var output = ComposeOutput(source, message, args);
            // Crashlytics.Log(output);
            var unityObject = source as UnityEngine.Object;
            if (unityObject != null)
            {
                Debug.LogError(output, unityObject);
            } else
            {
                Debug.LogError(output);
            }
        }

        private static Dictionary<object, string> sectionCache = new Dictionary<object, string>();

        private static string ComposeSection(object source)
        {
            if (source is string)
                return (string)source;

            string result;
            if (sectionCache.TryGetValue(source, out result))
                return result;
            result = (source is Type) ? ((Type)source).Name : source.GetType().Name;
            sectionCache[source] = result;
            return result;
        }

        public static void LogException(object source, Exception exception)
        {
            var section = ComposeSection(source);
            var output = string.Format("{0}\nat {1}", exception.Message, exception.StackTrace);
            Debug.LogError(output);
            // Crashlytics.RecordCustomException(exception.Message, section, exception.StackTrace);
        }

        private static string ComposeText(string message, params object[] args)
        {
            return args.Length > 0 ? string.Format(message, args) : message;
        }

        private static string ComposeOutput(object source, string message, params object[] args)
        {
            var section = ComposeSection(source);
            var text = ComposeText(message, args);
            var editor = Application.isEditor && !Application.isBatchMode;
            return editor ?
                string.Format("[{0}] {1}", section, text) :
                string.Format("({0:0000.000}) [{1}] {2}", Time.realtimeSinceStartup, section, text);
        }
    }
}
