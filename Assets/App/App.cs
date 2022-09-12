using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ZFramework
{
    public partial class App : MonoBehaviour
    {
        #region System API

        // ---------------------------------------------------------------------------------------------------------
        // public properties (static)
        // ---------------------------------------------------------------------------------------------------------

        public static App Instance { get; private set; }

        public static long Timestamp => DateTimeOffset.UtcNow.ToUnixTimeSeconds() + App.GStore.Get<int>("TimestampOffset");
        public static long TimestampZoneOffset => (long)TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalSeconds;

        // ---------------------------------------------------------------------------------------------------------
        // public properties 
        // ---------------------------------------------------------------------------------------------------------

        public MainLoop MainLoop { get; private set; }

        // ---------------------------------------------------------------------------------------------------------
        // Methods
        // ---------------------------------------------------------------------------------------------------------

        public static Promise Do(AppTask task)
        {
            App.Instance.StartCoroutine(DoCoroutine(task));
            return task.Promise;
        }


        public static T Do<T>() where T : ITask
        {
            return (T)Activator.CreateInstance(typeof(T));
        }


        private static IEnumerator DoCoroutine(AppTask task)
        {
            // Run task logic
            yield return task.Run();

            // In case task Run() doesn't close pormise
            task.Promise.Fulfill();
        }


        private static void CleanUpMemory()
        {
            GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        #endregion

        #region Object lifecycle

        private App()
        {
            Instance = this;
            MainLoop = new MainLoop();
        }


        private void OnDestroy()
        {
            Instance = null;
        }

        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            DontDestroyOnLoad(this);
            Do(new PreInitTask()).Catch(OnErrorPre);
        }


        private IEnumerator Start()
        {
            while (MainLoop.CurState < MainLoop.State.Init)
            {
                yield return new WaitForEndOfFrame();
            }

            Do(new InitTask()).Catch(OnError);
        }

        #endregion

        private void OnErrorPre(Exception e)
        {
            LogException(typeof(PreInitTask), e);
        }

        private void OnError(Exception e)
        {
            LogException(typeof(InitTask), e);
        }
    }



}