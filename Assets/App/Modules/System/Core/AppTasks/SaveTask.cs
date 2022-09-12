using System.Collections;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;

namespace ZFramework
{
    public class SaveTask : AppTask
    {
        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Public fields (static)
        // ---------------------------------------------------------------------------------------------------------

        public static string REMOTE_SAVE_KEY = "user/storage/key/set/";
        public static string LOCAL_DATA_SAVE_KEY = "app_data";

        private bool localSaveOnly;

        #endregion

        #region Object lifecycle

        public SaveTask(bool localSaveOnly = false)
        {
            this.localSaveOnly = localSaveOnly;
        }

        #endregion

        #region Methods

        public override IEnumerator Run()
        {
            PlayerPrefs.SetString(LOCAL_DATA_SAVE_KEY, App.GStore.GetJsonAllData());

            App.OnSaveData.Invoke();
            PlayerPrefs.Save();

            yield break;
        }

        #endregion
    }
}
