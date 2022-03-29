using UnityEngine;

namespace ZFramework
{
    public partial class App : MonoBehaviour
    {
        public void PreInit()
        {
			CardManager.Instance().PreInit();
			UIRoutingManager.Instance().PreInit();
			UIToolkitManager.Instance().PreInit();

        }
    }
}