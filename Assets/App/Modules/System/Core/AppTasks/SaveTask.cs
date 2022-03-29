using System.Collections;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;

namespace ZFramework
{
    public class SaveTask : AppTask
    {
        public static string REMOTE_SAVE_KEY = "user/storage/key/set/";
        public static string LOCAL_DATA_SAVE_KEY = "app_data";

        private bool localSaveOnly;

        public SaveTask(bool localSaveOnly = false)
        {
            this.localSaveOnly = localSaveOnly;
        }

        public override IEnumerator Run()
        {
            // Запись о ID устройства с которого было произведено сохранение (для решения конфликтов разных стейтов)
            //App.User.Device = App.Config.installUniqueIdentifier;


            //StringBuilder jsonData = new StringBuilder();
            //jsonData.Append("{\n\"GlobalStore\":");

            //jsonData.Append(App.GStore.GetJsonAllData());
            //jsonData.Append(",\n\"User\":");
            //jsonData.Append(JsonConvert.SerializeObject(App.User));

            //jsonData.Append("\n}");
            //App.Log(this, jsonData);


            PlayerPrefs.SetString(LOCAL_DATA_SAVE_KEY, App.GStore.GetJsonAllData());
            //PlayerPrefs.Save();

            //var user = JsonUtility.ToJson(App.User);

            // Local
            //PlayerPrefs.SetString(LOCAL_SAVE_KEY, user);
            //PlayerPrefs.Save();

            // Remote
            //if (!localSaveOnly)
            //    App.ServerManager.Post(REMOTE_SAVE_KEY,
            //        "key", "state",
            //        "value", user
            //    );

            App.OnSaveData.Invoke();
            PlayerPrefs.Save();

            yield break;
        }
    }
}
