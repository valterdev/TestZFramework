using System.Collections;

namespace ZFramework
{
    public abstract class AppTask
    {
        public readonly Promise Promise = new Promise();
        public abstract IEnumerator Run();
    }
}
