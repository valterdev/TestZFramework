using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFramework
{
    public partial class App : MonoBehaviour
    {
        public static Hook OnLoadData;
        public static Hook OnBeforeStart;
        public static Hook OnStart;

        public static Hook OnSaveData;
    }
}
