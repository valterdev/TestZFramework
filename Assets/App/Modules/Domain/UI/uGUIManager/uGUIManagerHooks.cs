using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFramework
{
    public partial class App : MonoBehaviour
    {
        public static Hook<BasePopup> OnPopupOpened;// (How to use: App.OnPopupOpened += Action;)
        public static Hook<BasePopup> OnPopupClosed;
    }
}
