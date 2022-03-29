using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZFramework
{
    public class InitTask : AppTask
    {
        public override IEnumerator Run()
        {
            yield return Promise.Resolved()
                // После того, как зарегистрированы все модули, возможно часть из этих модулей захочет переконфигурировать другие
                // В этом случае, они подключаются к данному хуку и меняют параметры до вызова основного Init Task
                .Then(App.OnLoadData.Invoke)
                .Then(App.OnBeforeStart.Invoke)

                .Then(App.UI.CreateUI)
                .Then(App.OnStart.Invoke)

                .Then(App.Instance.MainLoop.NextInit);
        }
    }
}