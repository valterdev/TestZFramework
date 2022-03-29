using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFramework
{
    public class PreInitTask : AppTask
    {
        public override IEnumerator Run()
        {
            yield return Promise.Resolved()
                .Then(GStore.Instance().PreInit)
                .Then(GStore.Instance().InitStores)
                .Then(GStore.Instance().Init)

                .Then(App.Instance.PreInit)
                .Then(App.Do<LoadTask>().Begin)

                .Then(App.Instance.MainLoop.NextInit);
        }
    }
}