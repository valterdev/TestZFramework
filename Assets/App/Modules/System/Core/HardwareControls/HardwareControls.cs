using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFramework
{
    public partial class App : MonoBehaviour
    {
        #region Unity lifecycle

        void OnApplicationPause(bool state)
		{
			
			if (state)
			{
				if (!App.GStore.Get<bool>("NoSave"))
				{
					App.Do(new SaveTask());
				}
				
			}
		}

		void OnApplicationQuit()
		{
			if (!App.GStore.Get<bool>("NoSave"))
			{
				App.Do(new SaveTask());
			}
		}

        #endregion
    }
}
