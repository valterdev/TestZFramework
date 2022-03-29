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
        private static void CleanUpMemory()
        {
            GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        public static Promise Do(AppTask task)
        {
            App.Instance.StartCoroutine(DoCoroutine(task));
            return task.Promise;
        }

        private static IEnumerator DoCoroutine(AppTask task)
        {
            // Run task logic
            yield return task.Run();

            // In case task Run() doesn't close pormise
            task.Promise.Fulfill();
        }

        public static T Do<T>() where T : ITask
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

        public static long Timestamp => DateTimeOffset.UtcNow.ToUnixTimeSeconds() + App.GStore.Get<int>("TimestampOffset");
        public static long TimestampZoneOffset => (long)TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalSeconds;

        public static App Instance { get; private set; }
        #endregion

        public MainLoop MainLoop { get; private set; }

        private App()
        {
            Instance = this;
            MainLoop = new MainLoop();
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
            Do(new PreInitTask()).Catch(OnErrorPre);
            //PreInit();
        }

        public IEnumerator Start()
        {
            while (MainLoop.CurState < MainLoop.State.Init)
            {
                yield return new WaitForEndOfFrame();
            }

            //Init();
            Do(new InitTask()).Catch(OnError);

            StartCoroutine(ttt());
        }

        private void OnErrorPre(Exception e)
        {
            LogException(typeof(PreInitTask), e);
        }

        private void OnError(Exception e)
        {
            LogException(typeof(InitTask), e);

            //if (e is FirstRunManifestException)
            //{
            //    App.UI.Get<ErrorPopup>().Open(ErrorPopupType.FirstRunManifest, ErrorPopupButton.Support, ErrorPopupSource.Preloader);
            //} else
            //{
            //    App.UI.Get<ErrorPopup>().Open(ErrorPopupType.Unknown, ErrorPopupButton.Support, ErrorPopupSource.Preloader);
            //}
        }

        IEnumerator ttt()
        {
            yield return new WaitForSeconds(2f);

            App.GStore.Set<int>("User/Testtt", 1);

            yield return new WaitForSeconds(2f);

            App.GStore.Set<int>("User/Testtt", -3);
        }
    }



}