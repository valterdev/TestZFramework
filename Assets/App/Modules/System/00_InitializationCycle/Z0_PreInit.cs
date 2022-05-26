using UnityEngine;

namespace ZFramework
{
    public partial class App : MonoBehaviour
    {
        public void PreInit()
        {
			UI.Instance().PreInit();

        }
    }
}