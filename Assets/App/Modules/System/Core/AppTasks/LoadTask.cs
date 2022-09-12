using System;
using UnityEngine;

namespace ZFramework
{
    public class LoadTask : ITask
    {
        public Promise Begin()
        {
            // From Server
            //App.AppUser user = null;

            // From Local Save
            var jsonData = PlayerPrefs.GetString(SaveTask.LOCAL_DATA_SAVE_KEY);
            if (!string.IsNullOrEmpty(jsonData))
            {
                try {
                    App.GStore.ImportFromJson(jsonData);
                    App.GStore.ProcessLoadedData();
                }
                catch (Exception e) { App.LogException(this, e); }
            }

            var promise = new Promise();
            promise.Fulfill();
            return promise;
        }
    }
}