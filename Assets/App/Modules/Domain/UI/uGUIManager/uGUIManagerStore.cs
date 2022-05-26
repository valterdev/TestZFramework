using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFramework
{
    public static partial class GSHelp // uGUIManagerStore
    {
        // For autocomplete use this: GSHelp.
        // Using:
        // App.GStore.Get<int>("uGUIManagerTestUniqGlobalVariable");
        // App.GStore.Get<bool>("uGUIManager/uGUIManagerTestGlobalBoolVariable");
        // App.GStore.Set<bool>("uGUIManager/uGUIManagerTestGlobalBoolVariable", true);


        public class UI
        {
            public static int Life = 10;
        }
    }
}