using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZFramework
{
    public class InitTask : AppTask
    {
        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods (override)
        // ---------------------------------------------------------------------------------------------------------

        public override IEnumerator Run()
        {
            yield return Promise.Resolved()
                // After all modules are registered, some of these modules may want to reconfigure others.
                // In this case, they connect to this hook and change the parameters before calling the main Init Task
                .Then(App.OnLoadData.Invoke)
                .Then(App.OnBeforeStart.Invoke)

                .Then(App.UI.CreateUI)
                .Then(App.OnStart.Invoke)

                .Then(App.Instance.MainLoop.NextInit);
        }

        #endregion
    }
}